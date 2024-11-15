using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using BubbleBots.Match3.Models;
using BubbleBots.Match3.Data;

namespace BubbleBots.Match3.Controllers
{
    public class BoardController
    {
        private BoardModel boardModel;
        private LevelData levelData;
        private MatchPrecedence matchPrecedence;

        private enum SwipeDirection
        {
            None,
            Horizontal,
            Vertical
        }
        public BoardModel GetBoardModel()
        {
            return boardModel;
        }

        public LevelData GetLevelData()
        {
            return levelData;
        }

        public void Initialize(LevelData _levelData, MatchPrecedence _matchPrecedence)
        {
            levelData = _levelData;
            matchPrecedence = _matchPrecedence;
            boardModel = new BoardModel(levelData.width, levelData.height);
        }

        public void PopulateBoardWithPredefinedGems(LevelDesign levelDesign)
        {
            //levelDesign.LoadFromJson("Assets/Data/Levels/LevelDesign/level.json");

            for (int i = 0; i < boardModel.width; i++)
                for (int j = 0; j < boardModel.height; ++j)
                {
                    boardModel[i][j].SetGem(new BoardGem(levelDesign.rows[j][i], GemType.Normal));
                }

        }


        public void PopulateBoardWithSeed(int seed)
        {
            //should use a custom random class
            Random.InitState(seed);
            bool canSpawnBubbles = levelData.bubbleSpawnChance > 0;
            for (int i = 0; i < boardModel.width; i++)
                for (int j = 0; j < boardModel.height; ++j)
                {
                    //if (canSpawnBubbles && Random.Range(0, 100) < levelData.bubbleSpawnChance)
                    //{
                    //    boardModel[i][j].SetGem(new BoardGem("14", GemType.Bubble));
                    //}
                    //else
                    //{
                    boardModel[i][j].SetGem(new BoardGem(levelData.gemSet[Random.Range(0, levelData.gemSet.Count)].gemId));
                    //}
                }

            bool matchesExist = true;

            while (matchesExist)
            {
                matchesExist = false;
                for (int i = 0; i < boardModel.width && !matchesExist; i++)
                    for (int j = 0; j < boardModel.height; ++j)
                    {
                        if (CheckForMatchOnPosition(i, j))
                        {
                            //randomize gem and restart check
                            boardModel.RandomizeGem(i, j, levelData.gemSet);
                            matchesExist = true;
                            break;
                        }
                    }
            }
        }

        private bool CheckForMatchOnPosition(int posX, int posY)
        {
            MatchTestResult matchTestResult = boardModel.TestForMatchOnPosition(posX, posY, matchPrecedence.matches, null);
            List<Vector2Int> match = (matchTestResult != null) ? matchTestResult.match : null;
            return match != null;
        }

        public bool IsSwapAllowed(int startX, int startY, int releaseX, int releaseY)
        {
            return boardModel.IsSwapAllowed(startX, startY, releaseX, releaseY);
        }

        public bool CanSwap(int startX, int startY, int releaseX, int releaseY)
        {
            return boardModel.CanSwap(startX, startY, releaseX, releaseY, matchPrecedence.matches);
        }

        public bool HasPossibleMove()
        {
            return boardModel.HasPossibleMove(matchPrecedence.matches);
        }

        public Hint GetHint(bool invalidateExistingOne = false)
        {
            if (invalidateExistingOne)
            {
                boardModel.UpdateHint(matchPrecedence.matches);
            }
            return boardModel.GetHint();
        }

        public NewSwapResult NewSwapGems(int startX, int startY, int releaseX, int releaseY)
        {
            boardModel.SwapGems(startX, startY, releaseX, releaseY);
            NewSwapResult swapResult = new NewSwapResult()
            {
                explodeEvents = new List<ExplodeEvent>()
            };

            //process swap explosions
            HashSet<ToExplode> toExplode = new HashSet<ToExplode>(ExplodeSpecialSpecial(ref swapResult, startX, startY, releaseX, releaseY));
            if (toExplode.Count == 0) // no special-special swap
            {
                toExplode = new HashSet<ToExplode>(ExplodeSpecialNormal(ref swapResult, startX, startY, releaseX, releaseY));
                if (toExplode.Count == 0) // no special-normal swap
                {
                    toExplode = new HashSet<ToExplode>(ExplodeSpecialBubble(ref swapResult, startX, startY, releaseX, releaseY));
                    if (toExplode.Count == 0) // no special-bubble swap
                    {
                        toExplode = new HashSet<ToExplode>(ExplodeNormalNormal(ref swapResult, startX, startY, releaseX, releaseY));
                    }
                }
            }

            //end process swap explosions
            if (toExplode.Count == 0) //fail on can swap check
            {

            }
            else
            {
                //process chain explosions
                Queue<ToExplode> specialsToProcess = new Queue<ToExplode>();
                List<Vector2Int> processed = new List<Vector2Int>();

                //add already exploded specials from swap 
                if (boardModel[startX][startY].gem.IsSpecial())
                {
                    processed.Add(new Vector2Int(startX, startY));
                }
                if (boardModel[releaseX][releaseY].gem.IsSpecial())
                {
                    processed.Add(new Vector2Int(releaseX, releaseY));
                }


                foreach (ToExplode explosion in toExplode)
                {
                    if (boardModel[explosion.position.x][explosion.position.y].gem.IsSpecial() &&
                        !processed.Contains(explosion.position))
                    {
                        specialsToProcess.Enqueue(explosion);
                    }
                    else
                    {
                        processed.Add(new Vector2Int(explosion.position.x, explosion.position.y));
                    }
                }


                while (specialsToProcess.Count > 0)
                {
                    ToExplode currentSpecial = specialsToProcess.Dequeue();
                    processed.Add(currentSpecial.position);
                    HashSet<ToExplode> specialOutcome = ExplodeSpecial(ref swapResult, currentSpecial, processed);


                    foreach (ToExplode outcome in specialOutcome)
                    {
                        if (processed.Contains(outcome.position))
                        {
                            continue;
                        }
                        if (boardModel[outcome.position.x][outcome.position.y].gem.IsSpecial())
                        {
                            specialsToProcess.Enqueue(outcome);
                        }
                        else
                        {
                            processed.Add(outcome.position);
                            toExplode.Add(outcome);
                        }
                    }
                }
                //end process chain explosions
            }

            for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
            {
                boardModel.RemoveGems(swapResult.explodeEvents[i].toExplode);
            }
            for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
            {
                boardModel.CreateGems(swapResult.explodeEvents[i].toCreate);
                ColorChangeEvent colorChangeEvent = swapResult.explodeEvents[i] as ColorChangeEvent;
                if (colorChangeEvent != null)
                {
                    boardModel.ApplyColorChanges(colorChangeEvent.toChange, colorChangeEvent.targetColor);
                }
            }
            return swapResult;
        }

        public NewSwapResult CheckForMatches()
        {
            NewSwapResult swapResult = new NewSwapResult();
            swapResult.explodeEvents = new List<ExplodeEvent>();

            List<Vector2Int> processed = new List<Vector2Int>();

            for (int i = 0; i < boardModel.width; i++)
                for (int j = 0; j < boardModel.height; ++j)
                {
                    if (processed.Contains(new Vector2Int(i, j)))
                        continue;

                    MatchTestResult matchTestResult = boardModel.TestForMatchOnPosition(i, j, matchPrecedence.matches, processed);
                    if (matchTestResult != null)
                    {
                        //fun bug
                        if (boardModel[matchTestResult.match[0].x][matchTestResult.match[0].y].gem.IsSpecial())
                        {
                            List<Explosion> boardBlast = boardModel.BoardBlast();
                            BoardBlastEvent boardBlastEventFunBug = new BoardBlastEvent()
                            {
                                blastPosition = new Vector2Int(i, j),
                                toCreate = null,
                                toExplode = boardBlast
                            };
                            swapResult.explodeEvents.Clear();
                            swapResult.explodeEvents.Add(boardBlastEventFunBug);
                            return swapResult;
                        }

                        ExplodeEvent explodeEvent = new ExplodeEvent();
                        explodeEvent.toExplode = new List<Explosion>();

                        foreach (Vector2Int pos in matchTestResult.match)
                        {
                            explodeEvent.toExplode.Add(new Explosion()
                            {
                                position = pos,
                                id = matchTestResult.id
                            });
                        }


                        if (matchTestResult.outcome != null)
                        {
                            explodeEvent.toCreate = new List<GemCreate>();
                            explodeEvent.toCreate.Add(matchTestResult.outcome);
                        }
                        if (swapResult.explodeEvents == null)
                        {
                            swapResult.explodeEvents = new List<ExplodeEvent>();
                        }
                        processed.AddRange(matchTestResult.match);
                        swapResult.explodeEvents.Add(explodeEvent);
                    }
                }

            for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
            {
                boardModel.RemoveGems(swapResult.explodeEvents[i].toExplode);
            }
            for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
            {
                boardModel.CreateGems(swapResult.explodeEvents[i].toCreate);
            }

            return swapResult;
        }

        private HashSet<ToExplode> ExplodeSpecial(ref NewSwapResult swapResult, ToExplode explosion, List<Vector2Int> processed) // specials exploded on chain reactions
        {
            HashSet<ToExplode> toExplode = new HashSet<ToExplode>();
            string specialId = boardModel[explosion.position.x][explosion.position.y].gem.GetId();
            boardModel.DisableSpecialBug();

            switch (specialId)
            {
                case "11": //bomb
                    Vector2Int bombPosition = explosion.position;

                    List<Explosion> bombBlast = boardModel.BombBlast(bombPosition.x, bombPosition.y, explosion.bombRadius);

                    BombBlastEvent bombBlastEvent = new BombBlastEvent();
                    bombBlastEvent.toExplode = new List<Explosion>();
                    bombBlastEvent.toCreate = null;

                    foreach (Explosion bombExplosion in bombBlast)
                    {
                        if (processed == null || !processed.Contains(bombExplosion.position))
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(bombExplosion.position.x, bombExplosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.BombBlast,
                            });
                            bombBlastEvent.toExplode.Add(bombExplosion);
                        }
                    }
                    if (bombBlastEvent.toExplode.Count > 0)
                    {
                        swapResult.explodeEvents.Add(bombBlastEvent);
                    }
                    break;
                case "9": //lightning
                    bool lineBlast = explosion.explosionSource == ToExplode.ExplosionSource.ColumnBlast ? true :
                        explosion.explosionSource == ToExplode.ExplosionSource.LineBlast ? false :
                        Random.Range(0f, 1f) < 0.5f ? true : false;

                    List<Explosion> blastOutcome = lineBlast ? boardModel.LineBlast(explosion.position.x, explosion.position.y) :
                        boardModel.ColumnBlast(explosion.position.x, explosion.position.y);

                    ExplodeEvent explodeEvent = lineBlast ?
                             new LineBlastExplodeEvent()
                             {
                                 lineBlastStartPosition = new Vector2Int(explosion.position.x, explosion.position.y),
                                 toExplode = new List<Explosion>(),
                                 toCreate = null
                             } : new ColumnBlastEvent()
                             {
                                 columnBlastStartPosition = new Vector2Int(explosion.position.x, explosion.position.y),
                                 toExplode = new List<Explosion>(),
                                 toCreate = null
                             };
                    foreach (Explosion outcome in blastOutcome)
                    {
                        if (outcome.position.x == explosion.position.x && outcome.position.y == explosion.position.y)
                        {
                            continue;
                        }
                        if (processed == null || !processed.Contains(outcome.position))
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(outcome.position.x, outcome.position.y),
                                explosionSource = lineBlast ? ToExplode.ExplosionSource.LineBlast : ToExplode.ExplosionSource.ColumnBlast,
                            });
                            explodeEvent.toExplode.Add(outcome);
                        }
                    }
                    if (explodeEvent.toExplode.Count > 0)
                    {
                        explodeEvent.toExplode.Add(new Explosion()
                        {
                            position = explosion.position,
                            id = specialId
                        });
                        swapResult.explodeEvents.Add(explodeEvent);
                    }
                    break;
                case "12":

                    Vector2Int hammerPosition = explosion.position;

                    List<Explosion> hammerBlast = boardModel.HammerBlast(hammerPosition.x, hammerPosition.y, explosion.hammerRadius, explosion.hammerRadius);

                    HammerBlastEvent hammerBlastEvent = new HammerBlastEvent();
                    hammerBlastEvent.toExplode = new List<Explosion>();
                    hammerBlastEvent.toCreate = null;

                    foreach (Explosion hammerExplosion in hammerBlast)
                    {
                        if (processed == null || !processed.Contains(hammerExplosion.position))
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(hammerExplosion.position.x, hammerExplosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.HammerBlast,
                            });
                            hammerBlastEvent.toExplode.Add(hammerExplosion);
                        }
                    }
                    if (hammerBlastEvent.toExplode.Count > 0)
                    {
                        swapResult.explodeEvents.Add(hammerBlastEvent);
                    }
                    break;
                case "13":
                    string target = levelData.gemSet[Random.Range(0, levelData.gemSet.Count)].gemId;
                    List<Vector2Int> candidates = boardModel.GetPossibleColorChanges(target);

                    candidates.RemoveAll(x => processed.Contains(x));
                    while (candidates.Count > 9)
                    {
                        candidates.RemoveAt(Random.Range(0, candidates.Count));
                    }
                    candidates.Add(explosion.position);

                    ColorChangeEvent colorChangeEvent = new ColorChangeEvent()
                    {
                        targetColor = target,
                        toChange = candidates,
                        toCreate = null,
                        toExplode = new List<Explosion>()
                    };
                    swapResult.explodeEvents.Add(colorChangeEvent);
                    break;
                case "10":
                    string targetColor = levelData.gemSet[Random.Range(0, levelData.gemSet.Count)].gemId;
                    List<Explosion> toDestroy = boardModel.GetAllById(targetColor);

                    toDestroy.Add(new Explosion() { position = explosion.position});
                    toDestroy.RemoveAll(x => processed.Contains(x.position));

                    foreach (Explosion item in toDestroy)
                    {
                        toExplode.Add(new ToExplode()
                        {
                            position = new Vector2Int(item.position.x, item.position.y),
                            explosionSource = ToExplode.ExplosionSource.ColorBlast,
                        });
                    }
                    ColorBlastEvent colorBlastEvent = new ColorBlastEvent();
                    colorBlastEvent.toExplode = toDestroy;
                    colorBlastEvent.toCreate = null;
                    colorBlastEvent.colorBlastPosition = explosion.position;
                    swapResult.explodeEvents.Add(colorBlastEvent);
                    break;
                default:
                    break;
            }

            return toExplode;
        }

        private HashSet<ToExplode> ExplodeNormalNormal(ref NewSwapResult swapResult, int startX, int startY, int releaseX, int releaseY)
        {
            HashSet<ToExplode> toExplode = new HashSet<ToExplode>();
            //test on swap position as they are guaranteed to be different
            MatchTestResult matchTestResult = boardModel.TestForMatchOnPosition(startX, startY, matchPrecedence.matches, null);

            bool enableSpecialBug = false;

            if (matchTestResult != null)
            {
                for (int i = 0; i < matchTestResult.match.Count; ++i)
                {
                    ToExplode explosion = new ToExplode();
                    explosion.position = matchTestResult.match[i];
                    explosion.explosionSource = ToExplode.ExplosionSource.Swap;
                    toExplode.Add(explosion);
                }
                ExplodeEvent explodeEvent = new ExplodeEvent();
                explodeEvent.toExplode = new List<Explosion>(/*matchTestResult.match*/);
                foreach (Vector2Int pos in matchTestResult.match)
                {
                    explodeEvent.toExplode.Add(new Explosion()
                    {
                        position = pos,
                        id = matchTestResult.id
                    });
                }

                explodeEvent.toCreate = new List<GemCreate>() { matchTestResult.outcome };
                swapResult.explodeEvents.Add(explodeEvent);

                if (matchTestResult.outcome == null && matchTestResult.match[0].y > boardModel.GetLastCreatedSpecialPosition().y)
                {
                    enableSpecialBug = true;
                    //boardModel.EnableSpecialBug();
                }
            }

            matchTestResult = boardModel.TestForMatchOnPosition(releaseX, releaseY, matchPrecedence.matches, null);
            if (matchTestResult != null)
            {
                for (int i = 0; i < matchTestResult.match.Count; ++i)
                {
                    ToExplode explosion = new ToExplode();
                    explosion.position = matchTestResult.match[i];
                    explosion.explosionSource = ToExplode.ExplosionSource.Swap;
                    toExplode.Add(explosion);
                }
                ExplodeEvent explodeEvent = new ExplodeEvent();
                explodeEvent.toExplode = new List<Explosion>(/*matchTestResult.match*/);
                foreach (Vector2Int pos in matchTestResult.match)
                {
                    explodeEvent.toExplode.Add(new Explosion()
                    {
                        position = pos,
                        id = matchTestResult.id
                    });
                }
                explodeEvent.toCreate = new List<GemCreate>() { matchTestResult.outcome };
                swapResult.explodeEvents.Add(explodeEvent);

                if (matchTestResult.outcome == null && matchTestResult.match[0].y > boardModel.GetLastCreatedSpecialPosition().y)
                {
                    enableSpecialBug = true;
                    // boardModel.EnableSpecialBug();
                }
            }

            if (enableSpecialBug)
            {
                boardModel.EnableSpecialBug();
            }
            return toExplode;
        }

        private HashSet<ToExplode> ExplodeSpecialBubble(ref NewSwapResult swapResult, int startX, int startY, int releaseX, int releaseY)
        {
            HashSet<ToExplode> toExplode = new HashSet<ToExplode>();
            if ((boardModel.cells[startX][startY].gem.IsBubble() && boardModel.cells[releaseX][releaseY].gem.IsSpecial()) ||
                (boardModel.cells[startX][startY].gem.IsSpecial() && boardModel.cells[releaseX][releaseY].gem.IsBubble()))
            {
                toExplode.Add(
                    new ToExplode()
                    {
                        position = new Vector2Int(startX, startY),
                        explosionSource = ToExplode.ExplosionSource.Swap
                    });
                toExplode.Add(
                    new ToExplode()
                    {
                        position = new Vector2Int(releaseX, releaseY),
                        explosionSource = ToExplode.ExplosionSource.Swap
                    });
                ///code to handle special-bubble swap
            }
            return toExplode;
        }
        private HashSet<ToExplode> ExplodeSpecialNormal(ref NewSwapResult swapResult, int startX, int startY, int releaseX, int releaseY)
        {
            HashSet<ToExplode> toExplode = new HashSet<ToExplode>();
            int numSpecials = (boardModel[startX][startY].gem.IsSpecial() ? 1 : 0) + (boardModel[releaseX][releaseY].gem.IsSpecial() ? 1 : 0);
            if (numSpecials == 1)
            {
                boardModel.DisableSpecialBug();

                int normalPositionX = boardModel[startX][startY].gem.IsSpecial() ? releaseX : startX;
                int normalPositionY = boardModel[startX][startY].gem.IsSpecial() ? releaseY : startY;

                MatchTestResult matchTestResult = boardModel.TestForMatchOnPosition(normalPositionX, normalPositionY, matchPrecedence.matches, null);

                if (matchTestResult != null)
                {
                    for (int i = 0; i < matchTestResult.match.Count; ++i)
                    {
                        ToExplode explosion = new ToExplode();
                        explosion.position = matchTestResult.match[i];
                        explosion.explosionSource = ToExplode.ExplosionSource.Swap;
                        toExplode.Add(explosion);
                    }
                    ExplodeEvent explodeEvent = new ExplodeEvent();
                    explodeEvent.toExplode = new List<Explosion>(/*matchTestResult.match*/);
                    foreach (Vector2Int pos in matchTestResult.match)
                    {
                        explodeEvent.toExplode.Add(new Explosion()
                        {
                            position = pos,
                            id = matchTestResult.id
                        });
                    }
                    explodeEvent.toCreate = new List<GemCreate>() { matchTestResult.outcome };
                    swapResult.explodeEvents.Add(explodeEvent);
                }

                string specialId = boardModel[startX][startY].gem.IsSpecial() ? boardModel[startX][startY].gem.GetId() : boardModel[releaseX][releaseY].gem.GetId();

                switch (specialId)
                {
                    case "9": //lightning
                        bool isLineBlast = startX != releaseX;

                        toExplode.Add(new ToExplode() { explosionSource = ToExplode.ExplosionSource.Swap, position = new Vector2Int(startX, startY) });
                        toExplode.Add(new ToExplode() { explosionSource = ToExplode.ExplosionSource.Swap, position = new Vector2Int(releaseX, releaseY) });

                        List<Explosion> blast = isLineBlast ? boardModel.LineBlast(startX, startY) : boardModel.ColumnBlast(startX, startY);

                        ExplodeEvent explodeEvent = isLineBlast ?
                             new LineBlastExplodeEvent()
                             {
                                 lineBlastStartPosition = new Vector2Int(startX, startY),
                                 toExplode = blast,
                                 toCreate = null
                             } : new ColumnBlastEvent()
                             {
                                 columnBlastStartPosition = new Vector2Int(startX, startY),
                                 toExplode = blast,
                                 toCreate = null
                             };
                        swapResult.explodeEvents.Add(explodeEvent);

                        foreach (Explosion explosion in blast)
                        {
                            if (explosion.position.x == startX && explosion.position.y == startY)
                            {
                                continue;
                            }
                            if (explosion.position.x == releaseX && explosion.position.y == releaseY)
                            {
                                continue;
                            }
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = isLineBlast ? ToExplode.ExplosionSource.LineBlast : ToExplode.ExplosionSource.ColumnBlast
                            });
                        }
                        break;
                    case "12": //hammer
                        int explosionRange = 1;
                        Vector2Int hammerPosition = boardModel[startX][startY].gem.IsSpecial() ? new Vector2Int(startX, startY) :
                            new Vector2Int(releaseX, releaseY);

                        List<Explosion> hammerBlast = boardModel.HammerBlast(hammerPosition.x, hammerPosition.y, explosionRange, explosionRange);
                        foreach (Explosion explosion in hammerBlast)
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.HammerBlast
                            });
                        }
                        HammerBlastEvent hammerBlastEvent = new HammerBlastEvent();
                        hammerBlastEvent.toExplode = hammerBlast;
                        hammerBlastEvent.toCreate = null;
                        swapResult.explodeEvents.Add(hammerBlastEvent);
                        break;
                    case "11": //bomb
                        Vector2Int bombPosition = boardModel[startX][startY].gem.IsSpecial() ? new Vector2Int(startX, startY) :
                            new Vector2Int(releaseX, releaseY);

                        List<Explosion> bombBlast = boardModel.BombBlast(bombPosition.x, bombPosition.y);

                        foreach (Explosion explosion in bombBlast)
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.BombBlast
                            });
                        }
                        BombBlastEvent bombBlastEvent = new BombBlastEvent();
                        bombBlastEvent.toExplode = bombBlast;
                        bombBlastEvent.toCreate = null;
                        swapResult.explodeEvents.Add(bombBlastEvent);
                        break;
                    case "13": // color switch
                        string target = boardModel[startX][startY].gem.IsSpecial() ? boardModel[releaseX][releaseY].gem.GetId() :
                            boardModel[startX][startY].gem.GetId();
                        List<Vector2Int> candidates = boardModel.GetPossibleColorChanges(target);
                        while (candidates.Count > 9)
                        {
                            candidates.RemoveAt(Random.Range(0, candidates.Count));
                        }
                        candidates.Add(boardModel[startX][startY].gem.IsSpecial() ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY));


                        ColorChangeEvent colorChangeEvent = new ColorChangeEvent()
                        {
                            targetColor = target,
                            toChange = candidates,
                            toCreate = null,
                            toExplode = null
                        };
                        swapResult.explodeEvents.Add(colorChangeEvent);
                        break;
                    case "10": //color bomb
                        string targetColor = boardModel[startX][startY].gem.IsSpecial() ? boardModel[releaseX][releaseY].gem.GetId() :
                            boardModel[startX][startY].gem.GetId();
                        List<Explosion> toDestroy = boardModel.GetAllById(targetColor);

                        toDestroy.Add(
                            new Explosion() {
                                position = boardModel[startX][startY].gem.IsSpecial() ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY)
                            });

                        foreach (Explosion explosion in toDestroy)
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.ColorBlast
                            });
                        }
                        ColorBlastEvent colorBlastEvent = new ColorBlastEvent();
                        colorBlastEvent.toExplode = toDestroy;
                        colorBlastEvent.toCreate = null;
                        colorBlastEvent.colorBlastPosition = boardModel[startX][startY].gem.IsSpecial() ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY);
                        swapResult.explodeEvents.Add(colorBlastEvent);
                        break;
                    default:
                        break;
                }


            }
            return toExplode;
        }

        private HashSet<ToExplode> ExplodeSpecialSpecial(ref NewSwapResult swapResult, int startX, int startY, int releaseX, int releaseY)
        {
            HashSet<ToExplode> toExplode = new HashSet<ToExplode>();

            if (boardModel[startX][startY].gem.IsSpecial() &&
                boardModel[releaseX][releaseY].gem.IsSpecial() &&
                boardModel[startX][startY].gem.GetId() == boardModel[releaseX][releaseY].gem.GetId())
            {
                boardModel.DisableSpecialBug();
                string specialId = boardModel[startX][startY].gem.GetId();

                switch (specialId)
                {
                    case "9": // lightning-lightning
                        toExplode.Add(new ToExplode() { explosionSource = ToExplode.ExplosionSource.Swap, position = new Vector2Int(startX, startY) });
                        toExplode.Add(new ToExplode() { explosionSource = ToExplode.ExplosionSource.Swap, position = new Vector2Int(releaseX, releaseY) });


                        List<Explosion> lineBlast1 = boardModel.LineBlast(startX, startY);
                        List<Explosion> lineBlast2 = boardModel.LineBlast(releaseX, releaseY);

                        LineBlastExplodeEvent lineBlastExplodeEvent = new LineBlastExplodeEvent()
                        {
                            lineBlastStartPosition = new Vector2Int(startX, startY),
                            toExplode = lineBlast1,
                            toCreate = null
                        };
                        swapResult.explodeEvents.Add(lineBlastExplodeEvent);

                        if (lineBlast2.Count > 0)
                        {
                            lineBlastExplodeEvent = new LineBlastExplodeEvent()
                            {
                                lineBlastStartPosition = new Vector2Int(releaseX, releaseY),
                                toExplode = lineBlast2,
                                toCreate = null
                            };
                            swapResult.explodeEvents.Add(lineBlastExplodeEvent);
                        }

                        HashSet<Explosion> lineBlasts = new HashSet<Explosion>(lineBlast1);
                        lineBlasts.UnionWith(lineBlast2);

                        foreach (Explosion explosion in lineBlasts)
                        {
                            if (explosion.position.x == startX && explosion.position.y == startY)
                            {
                                continue;
                            }
                            if (explosion.position.x == releaseX && explosion.position.y == releaseY)
                            {
                                continue;
                            }
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.LineBlast
                            });
                        }


                        List<Explosion> columnBlast1 = boardModel.ColumnBlast(startX, startY);
                        List<Explosion> columnBlast2 = boardModel.ColumnBlast(releaseX, releaseY);

                        ColumnBlastEvent columnBlastEvent = new ColumnBlastEvent()
                        {
                            columnBlastStartPosition = new Vector2Int(startX, startY),
                            toExplode = new List<Explosion>(columnBlast1),
                            toCreate = null
                        };
                        swapResult.explodeEvents.Add(columnBlastEvent);

                        if (columnBlast2.Count > 0)
                        {
                            columnBlastEvent = new ColumnBlastEvent()
                            {
                                columnBlastStartPosition = new Vector2Int(releaseX, releaseY),
                                toExplode = new List<Explosion>(columnBlast2),
                                toCreate = null
                            };
                            swapResult.explodeEvents.Add(columnBlastEvent);
                        }

                        HashSet<Explosion> columnBlasts = new HashSet<Explosion>(columnBlast1);
                        columnBlasts.UnionWith(columnBlast2);

                        foreach (Explosion explosion in columnBlasts)
                        {
                            if (explosion.position.x == startX && explosion.position.y == startY)
                            {
                                continue;
                            }
                            if (explosion.position.x == releaseX && explosion.position.y == releaseY)
                            {
                                continue;
                            }
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.ColumnBlast
                            });
                        }
                        break;
                    case "12": // hammer-hammer
                        int explosionRange = 2;

                        List<Explosion> hammerBlast = boardModel.HammerBlast(startX, startY, explosionRange, explosionRange);
                        foreach (Explosion explosion in hammerBlast)
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.HammerBlast
                            });
                        }
                        HammerBlastEvent hammerBlastEvent1 = new HammerBlastEvent();
                        hammerBlastEvent1.toExplode = hammerBlast;
                        hammerBlastEvent1.toCreate = null;
                        swapResult.explodeEvents.Add(hammerBlastEvent1);

                        hammerBlast = boardModel.HammerBlast(releaseX, releaseY, explosionRange, explosionRange);
                        foreach (Explosion explosion in hammerBlast)
                        {
                            toExplode.Add(new ToExplode()
                            {
                                position = new Vector2Int(explosion.position.x, explosion.position.y),
                                explosionSource = ToExplode.ExplosionSource.HammerBlast
                            });
                        }
                        HammerBlastEvent hammerBlastEvent2 = new HammerBlastEvent();
                        hammerBlastEvent2.toExplode = hammerBlast;
                        hammerBlastEvent2.toCreate = null;
                        swapResult.explodeEvents.Add(hammerBlastEvent2);
                        break;
                    case "11":
                    case "10":
                    case "13":
                        List<Explosion> boardBlast = boardModel.BoardBlast();

                        BoardBlastEvent boardBlastEvent = new BoardBlastEvent()
                        {
                            blastPosition = new Vector2Int(startX, startY),
                            toCreate = null,
                            toExplode = boardBlast
                        };
                        swapResult.explodeEvents.Clear();
                        swapResult.explodeEvents.Add(boardBlastEvent);
                        //blast board
                        break;
                    default:
                        break;
                }
            }
            else if (boardModel[startX][startY].gem.IsSpecial() &&
                boardModel[releaseX][releaseY].gem.IsSpecial())
            {
                boardModel.DisableSpecialBug();
                string specialId1 = boardModel[startX][startY].gem.GetId();
                string specialId2 = boardModel[releaseX][releaseY].gem.GetId();

                if (specialId1 == "13" || specialId2 == "13" ||
                    specialId1 == "10" || specialId2 == "10") // color switch and color bomb with anything = board blast
                {
                    List<Explosion> boardBlastColorSwitch = boardModel.BoardBlast();

                    BoardBlastEvent boardBlastEventColorSwitch = new BoardBlastEvent()
                    {
                        blastPosition = new Vector2Int(startX, startY),
                        toCreate = null,
                        toExplode = boardBlastColorSwitch
                    };
                    swapResult.explodeEvents.Clear();
                    swapResult.explodeEvents.Add(boardBlastEventColorSwitch);
                    return toExplode;
                }

                List<Vector2Int> exclusionList = new List<Vector2Int>();
                if (specialId1 == "11" || specialId2 == "11") // bomb
                {
                    toExplode.UnionWith(ExplodeSpecial(ref swapResult, new ToExplode()
                    {
                        explosionSource = startX == releaseX ? ToExplode.ExplosionSource.LineBlast : ToExplode.ExplosionSource.ColumnBlast,
                        position = specialId1 == "11" ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY),
                        bombRadius = specialId1 == "11" ? 2 : 1
                    }, exclusionList));

                    Vector2Int bombPosition = specialId1 == "11" ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY);
                    if (!exclusionList.Contains(bombPosition))
                    {
                        toExplode.Add(new ToExplode()
                        {
                            position = bombPosition,
                            explosionSource = ToExplode.ExplosionSource.Swap,
                            bombRadius = specialId1 == "11" ? 2 : 1
                        });
                    }
                    foreach (ToExplode outcome in toExplode)
                    {
                        exclusionList.Add(outcome.position);
                    }
                }
                if (specialId1 == "9" || specialId2 == "9")
                {
                    toExplode.UnionWith(ExplodeSpecial(ref swapResult, new ToExplode()
                    {
                        explosionSource = startX == releaseX ? ToExplode.ExplosionSource.LineBlast : ToExplode.ExplosionSource.ColumnBlast,
                        position = specialId1 == "9" ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY)
                    }, exclusionList));

                    toExplode.Add(new ToExplode()
                    {
                        position = specialId1 == "9" ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY),
                        explosionSource = ToExplode.ExplosionSource.Swap
                    });
                    foreach (ToExplode outcome in toExplode)
                    {
                        exclusionList.Add(outcome.position);
                    }
                }
                if (specialId1 == "12" || specialId2 == "12")
                {
                    bool lightningHammerMatch = (specialId1 == "12" && specialId2 == "9") || (specialId1 == "9" && specialId2 == "12");

                    toExplode.UnionWith(ExplodeSpecial(ref swapResult, new ToExplode()
                    {
                        explosionSource = startX == releaseX ? ToExplode.ExplosionSource.LineBlast : ToExplode.ExplosionSource.ColumnBlast,
                        position = specialId1 == "12" ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY),
                        hammerRadius = lightningHammerMatch ? 2 : 1
                    }, exclusionList));

                    Vector2Int hammerPosition = specialId1 == "12" ? new Vector2Int(startX, startY) : new Vector2Int(releaseX, releaseY);

                    if (!exclusionList.Contains(hammerPosition))
                    {
                        toExplode.Add(new ToExplode()
                        {
                            position = hammerPosition,
                            explosionSource = ToExplode.ExplosionSource.Swap,
                            hammerRadius = lightningHammerMatch ? 2 : 1
                        });
                    }
                    foreach (ToExplode outcome in toExplode)
                    {
                        exclusionList.Add(outcome.position);
                    }
                }
            }
            return toExplode;
        }

        public List<GemMove> RefillBoard(bool canSpawnBubbles)
        {
            return boardModel.RefillBoard(levelData.gemSet, levelData, canSpawnBubbles);
        }

        public NewSwapResult ExplodeAllSpecials()
        {
            NewSwapResult swapResult = new NewSwapResult()
            {
                explodeEvents = new List<ExplodeEvent>()
            };

            HashSet<ToExplode> toExplode = new HashSet<ToExplode>();
            for (int i = 0; i < boardModel.width; i++)
                for (int j = 0; j < boardModel.height; ++j)
                {
                    if (boardModel[i][j].gem.IsSpecial())
                    {
                        if (boardModel[i][j].gem.GetId() == "10")
                        {
                            ToExplode alreadyAdded = toExplode.ToList().Find(x => (boardModel[x.position.x][x.position.y].gem.GetId() == "10"));
                            if (alreadyAdded == null)
                            {
                                toExplode.Add(new ToExplode() { position = new Vector2Int(i, j), explosionSource = ToExplode.ExplosionSource.Chain });
                            }
                        }
                        else
                        {
                            toExplode.Add(new ToExplode() { position = new Vector2Int(i, j), explosionSource = ToExplode.ExplosionSource.Chain });
                        }
                    }
                }

            Queue<ToExplode> specialsToProcess = new Queue<ToExplode>();
            List<Vector2Int> processed = new List<Vector2Int>();

            foreach (ToExplode explosion in toExplode)
            {
                if (boardModel[explosion.position.x][explosion.position.y].gem.IsSpecial() &&
                    !processed.Contains(explosion.position))
                {
                    specialsToProcess.Enqueue(explosion);
                    processed.Add(new Vector2Int(explosion.position.x, explosion.position.y));
                }
            }

            while (specialsToProcess.Count > 0)
            {
                ToExplode currentSpecial = specialsToProcess.Dequeue();

                string specialId = boardModel[currentSpecial.position.x][currentSpecial.position.y].gem.GetId();
                HashSet<ToExplode> specialOutcome = ExplodeSpecial(ref swapResult, currentSpecial, processed);

                foreach (ToExplode outcome in specialOutcome)
                {
                    if (processed.Contains(outcome.position))
                    {
                        continue;
                    }
                    if (boardModel[outcome.position.x][outcome.position.y].gem.IsSpecial())
                    {
                        continue;
                    }
                    else if (!processed.Contains(outcome.position) && !toExplode.Contains(outcome))
                    {
                        processed.Add(outcome.position);
                        toExplode.Add(outcome);
                    }
                }
                if (swapResult.explodeEvents == null)
                {
                    swapResult.explodeEvents.Add(new ExplodeEvent()
                    {
                        toExplode = new List<Explosion>()
                    });
                }
                swapResult.explodeEvents[0].toExplode.Add(
                    new Explosion()
                    {
                        position = new Vector2Int(currentSpecial.position.x, currentSpecial.position.y),
                        id = specialId
                    });
            }

            for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
            {
                boardModel.RemoveGems(swapResult.explodeEvents[i].toExplode);
            }

            for (int i = 0; i < swapResult.explodeEvents.Count; ++i)
            {
                boardModel.CreateGems(swapResult.explodeEvents[i].toCreate);
                ColorChangeEvent colorChangeEvent = swapResult.explodeEvents[i] as ColorChangeEvent;
                if (colorChangeEvent != null)
                {
                    boardModel.ApplyColorChanges(colorChangeEvent.toChange, colorChangeEvent.targetColor);
                }
            }
            return swapResult;
        }
    }
}

public class ToExplode
{
    public enum ExplosionSource
    {
        Swap,
        Chain,
        HammerBlast,
        ColumnBlast,
        LineBlast,
        BombBlast,
        ColorBlast
    }
    public Vector2Int position;
    public ExplosionSource explosionSource;
    public int bombRadius = 1;
    public int hammerRadius = 1;
}
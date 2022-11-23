using UnityEngine;
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

        public void Initialize(LevelData _levelData, MatchPrecedence _matchPrecedence)
        {
            levelData = _levelData;
            matchPrecedence = _matchPrecedence;
            boardModel = new BoardModel(levelData.width, levelData.height);
        }

        public void PopulateBoardWithSeed(int seed)
        {
            //should use a custom random class
            Random.InitState(seed);

            for (int i = 0; i < boardModel.width; i++)
                for (int j = 0; j < boardModel.height; ++j)
                {
                    boardModel[i][j].SetGem(new BoardGem(levelData.gemSet[Random.Range(0, levelData.gemSet.Count)]));
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
            MatchTestResult matchTestResult = boardModel.TestForMatchOnPosition(posX, posY, matchPrecedence.matches);
            List<Vector2Int> match = (matchTestResult != null) ? matchTestResult.match : null;
            return match != null;
        }

        public bool CanSwap(int startX, int startY, int releaseX, int releaseY)
        {
            return boardModel.CanSwap(startX, startY, releaseX, releaseY, matchPrecedence.matches);
        }

        public SwapResult SwapGems(int startX, int startY, int releaseX, int releaseY)
        {
            boardModel.SwapGems(startX, startY, releaseX, releaseY);
            SwapResult swapResult = ExplodeMatches();
            bool horizontal = startY == releaseY;

            if (boardModel[startX][startY].gem.IsSpecial() &&
                boardModel[releaseX][releaseY].gem.IsSpecial())
            {
                if (boardModel[startX][startY].gem.GetId() == 9 &&
                    boardModel[releaseX][releaseY].gem.GetId() == 9)
                {
                    List<Vector2Int> newExploded = new List<Vector2Int>();

                    newExploded.AddRange(ExplodeSpecial(ref swapResult, startX, startY, SwipeDirection.Horizontal));
                    newExploded.AddRange(ExplodeSpecial(ref swapResult, startX, startY, SwipeDirection.Vertical));

                    List<Vector2Int> alreadyExploded = new List<Vector2Int>();
                    alreadyExploded.Add(new Vector2Int(startX, startY));
                    alreadyExploded.Add(new Vector2Int(releaseX, releaseY));

                    Stack<Vector2Int> specialsToExplode = new Stack<Vector2Int>();
                    for (int i = 0; i < newExploded.Count; ++i)
                    {
                        if (alreadyExploded.Contains(newExploded[i]))
                        {
                            continue;
                        }

                        if (boardModel[newExploded[i].x][newExploded[i].y].gem.IsSpecial())
                        {
                            specialsToExplode.Push(new Vector2Int(newExploded[i].x, newExploded[i].y));
                        }
                    }

                    while (specialsToExplode.Count > 0)
                    {
                        Vector2Int currentSpecial = specialsToExplode.Pop();
                        List<Vector2Int> newExploded1 = ExplodeSpecial(ref swapResult, currentSpecial.x, currentSpecial.y, horizontal ?
                            SwipeDirection.Horizontal : SwipeDirection.Vertical);

                        alreadyExploded.Add(new Vector2Int(currentSpecial.x, currentSpecial.y));

                        for (int i = 0; i < newExploded1.Count; ++i)
                        {
                            if (alreadyExploded.Contains(newExploded1[i]))
                            {
                                continue;
                            }

                            if (boardModel[newExploded1[i].x][newExploded1[i].y].gem.IsSpecial())
                            {
                                specialsToExplode.Push(new Vector2Int(newExploded1[i].x, newExploded1[i].y));
                            }
                        }
                    }
                }
            }
            else
            {
                Stack<Vector2Int> specialsToExplode = new Stack<Vector2Int>();

                if (boardModel[startX][startY].gem.IsSpecial())
                {
                    specialsToExplode.Push(new Vector2Int(startX, startY));
                }
                if (boardModel[releaseX][releaseY].gem.IsSpecial())
                {
                    specialsToExplode.Push(new Vector2Int(releaseX, releaseY));
                }

                List<Vector2Int> alreadyExploded = new List<Vector2Int>();

                while (specialsToExplode.Count > 0)
                {
                    Vector2Int currentSpecial = specialsToExplode.Pop();
                    List<Vector2Int> newExploded = ExplodeSpecial(ref swapResult, currentSpecial.x, currentSpecial.y, horizontal ?
                        SwipeDirection.Horizontal : SwipeDirection.Vertical);

                    alreadyExploded.Add(new Vector2Int(currentSpecial.x, currentSpecial.y));

                    for (int i = 0; i < newExploded.Count; ++i)
                    {
                        if (alreadyExploded.Contains(newExploded[i]))
                        {
                            continue;
                        }

                        if (boardModel[newExploded[i].x][newExploded[i].y].gem.IsSpecial())
                        {
                            specialsToExplode.Push(new Vector2Int(newExploded[i].x, newExploded[i].y));
                        }
                    }
                }
            }

            //explode specials consequences

            boardModel.RemoveGems(swapResult.toExplode);
            boardModel.CreateGems(swapResult.toCreate);
            return swapResult;
        }

        private List<Vector2Int> ExplodeSpecial(ref SwapResult swapResult, int posX, int posY, SwipeDirection direction)
        {
            List<Vector2Int> newExploded = new List<Vector2Int>();


            if (boardModel[posX][posY].gem.GetId() == 9) // line blast
            {
                bool lineBlast = direction == SwipeDirection.Horizontal ? true :
                    direction == SwipeDirection.Vertical ? false :
                    Random.Range(0f, 1f) < 0.5f ? true : false;


                //no 2 line blasts on the same line or 2 column blasts on the same column
                if (lineBlast)
                {
                    if (boardModel.IsRowEmpty(posY))
                    {
                        lineBlast = false;
                    }
                }
                else
                {
                    if (boardModel.IsColumnEmpty(posX))
                    {
                        lineBlast = true;
                    }
                }

                if (lineBlast)
                {
                    newExploded = boardModel.LineBlast(posX, posY);
                }
                else
                {
                    newExploded = boardModel.ColumnBlast(posX, posY);
                }


                if (lineBlast)
                {
                    LineBlastExplodeEvent line = new LineBlastExplodeEvent();
                    line.toExplode = newExploded;
                    line.toCreate = null;
                    line.lineBlastStartPosition = new Vector2Int(posX, posY);
                    if (swapResult.explodeEvents == null)
                    {
                        swapResult.explodeEvents = new List<ExplodeEvent>();
                    }
                    swapResult.explodeEvents.Add(line);
                }
                else
                {
                    ColumnBlastEvent column = new ColumnBlastEvent();
                    column.toExplode = newExploded;
                    column.toCreate = null;
                    column.columnBlastStartPosition = new Vector2Int(posX, posY);
                    if (swapResult.explodeEvents == null)
                    {
                        swapResult.explodeEvents = new List<ExplodeEvent>();
                    }
                    swapResult.explodeEvents.Add(column);
                }

                swapResult.toExplode.AddRange(newExploded);
            }
            return newExploded;
        }

        public SwapResult ExplodeMatches(bool updateBoard = false)
        {
            SwapResult swapResult = new SwapResult();
            swapResult.toExplode = new List<Vector2Int>();
            swapResult.toCreate = new List<GemCreate>();

            for (int i = 0; i < boardModel.width; i++)
                for (int j = 0; j < boardModel.height; ++j)
                {
                    if (swapResult.toExplode.Contains(new Vector2Int(i, j)))
                        continue;

                    MatchTestResult matchTestResult = boardModel.TestForMatchOnPosition(i, j, matchPrecedence.matches);
                    if (matchTestResult != null)
                    {
                        swapResult.toExplode.AddRange(matchTestResult.match);

                        ExplodeEvent explodeEvent = new ExplodeEvent();
                        explodeEvent.toExplode = new List<Vector2Int>();
                        explodeEvent.toExplode.AddRange(matchTestResult.match);

                        if (matchTestResult.outcome != null)
                        {
                            swapResult.toCreate.Add(matchTestResult.outcome);
                            explodeEvent.toCreate = new List<GemCreate>();
                            explodeEvent.toCreate.Add(matchTestResult.outcome);
                        }
                        if (swapResult.explodeEvents == null)
                        {
                            swapResult.explodeEvents = new List<ExplodeEvent>();
                        }

                        swapResult.explodeEvents.Add(explodeEvent);
                    }
                }

            if (updateBoard)
            {
                boardModel.RemoveGems(swapResult.toExplode);
                boardModel.CreateGems(swapResult.toCreate);
            }

            return swapResult;
        }

        public List<GemMove> RefillBoard()
        {
            return boardModel.RefillBoard(levelData.gemSet);
        }

    }
}

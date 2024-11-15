using System.Collections.Generic;
using BubbleBots.Match3.Data;
using UnityEngine;

namespace BubbleBots.Match3.Models
{
    public class BoardModel
    {
        public int width;
        public int height;

        public List<List<BoardCell>> cells;

        private Vector2Int hintPosition;

        private Hint hint;

        //special bug
        private Vector2Int lastCreatedSpecialPosittion = -Vector2Int.one;
        private string lastCreatedSpecialType;
        private bool canCreateSpecial = false;

        public BoardModel(int _width, int _height)
        {
            width = _width;
            height = _height;
            cells = new List<List<BoardCell>>();
            for (int i = 0; i < _width; i++)
            {
                cells.Add(new List<BoardCell>());
                for (int j = 0; j < _height; ++j)
                {
                    cells[i].Add(new BoardCell());
                }
            }
            hintPosition = -Vector2Int.one;
            hint = new Hint();
        }

        public List<BoardCell> this[int i]
        {
            get => cells[i];
            set => cells[i] = value;
        }

        public void RandomizeGem(int posX, int posY, List<GemData> gemSet, bool excludeCurrent = true)
        {
            List<string> possibleValues = new List<string>();
            foreach (GemData gemData in gemSet)
            {
                possibleValues.Add(gemData.gemId);
            }

            if (excludeCurrent)
            {
                possibleValues.Remove(cells[posX][posY].gem.GetId());
            }

            cells[posX][posY].gem.SetId(possibleValues[Random.Range(0, possibleValues.Count)]);
        }

        private bool BoundaryTest(int posX, int posY)
        {
            return 0 <= posX && posX < width &&
                    0 <= posY && posY < height;
        }

        public void EnableSpecialBug()
        {
            canCreateSpecial = true;
        }

        public void DisableSpecialBug()
        {
            lastCreatedSpecialPosittion = -Vector2Int.one;
            canCreateSpecial = false;
        }

        public Vector2Int GetLastCreatedSpecialPosition()
        {
            return lastCreatedSpecialPosittion;
        }


        public MatchTestResult TestForMatchOnPosition(int posX, int posY, List<MatchShape> matchPrecedenceList, List<Vector2Int> exclusionList = null)
        {
            for (int i = matchPrecedenceList.Count - 1; i >= 0; i--)
            {
                MatchTestResult matchResult = TestMatch(posX, posY, matchPrecedenceList[i], exclusionList);
                if (matchResult != null)
                {
                    return matchResult;
                }
            }
            return null;
        }

        public MatchTestResult TestMatch(int posX, int posY, MatchShape shape, List<Vector2Int> exclusionList)
        {
            if (!cells[posX][posY].gem.IsMatchable())
            {
                return null;
            }
            MatchTestResult matchTestResult = new MatchTestResult();
            matchTestResult.id = cells[posX][posY].gem.GetId();

            for (int i = 0; i < shape.offsets.Count; ++i)
            {
                bool foundMatch = false;
                List<Vector2Int> match = new List<Vector2Int>();
                match.Add(new Vector2Int(posX, posY));
                for (int j = 0; j < shape.offsets[i].offsetList.Count; j++)
                {
                    foundMatch = true;
                    int offsetX = shape.offsets[i].offsetList[j].x;
                    int offsetY = shape.offsets[i].offsetList[j].y;

                    if (!BoundaryTest(posX + offsetX, posY + offsetY))
                    {
                        foundMatch = false;
                        break;
                    }

                    if (exclusionList != null &&
                        exclusionList.Contains(new Vector2Int(posX + offsetX, posY + offsetY)))
                    {
                        foundMatch = false;
                        break;
                    }

                    match.Add(new Vector2Int(posX + offsetX, posY + offsetY));

                    if (cells[posX][posY].gem.GetId() != cells[posX + offsetX][posY + offsetY].gem.GetId())
                    {
                        foundMatch = false;
                        break;
                    }
                }

                if (foundMatch)
                {
                    matchTestResult.match = match;
                    if (shape.matchOutcome != null)
                    {
                        matchTestResult.outcome = new GemCreate(new Vector2Int(posX, posY), shape.matchOutcome);
                    }
                    return matchTestResult;
                }
            }

            return null;
        }

        public bool IsSwapAllowed(int startX, int startY, int releaseX, int releaseY)
        {
            //if ((cells[startX][startY].gem.IsBubble() && cells[releaseX][releaseY].gem.IsSpecial()) ||
            //    (cells[startX][startY].gem.IsSpecial() && cells[releaseX][releaseY].gem.IsBubble()))
            //{ 
            //    return true;
            //}

            //if (!cells[startX][startY].gem.IsSwappable() ||
            //    !cells[releaseX][releaseY].gem.IsSwappable())
            //{
            //    return false;
            //}

            return true;
        }

        public bool CanSwap(int startX, int startY, int releaseX, int releaseY, List<MatchShape> matchPrecedenceList)
        {
            if ((cells[startX][startY].gem.IsBubble() && cells[releaseX][releaseY].gem.IsSpecial()) ||
                (cells[startX][startY].gem.IsSpecial() && cells[releaseX][releaseY].gem.IsBubble()))
            {
                return true;
            }

            if (!cells[startX][startY].gem.IsSwappable() ||
                !cells[releaseX][releaseY].gem.IsSwappable())
            {
                return false;
            }
            if (cells[startX][startY].gem.IsSpecial() || cells[releaseX][releaseY].gem.IsSpecial())
            {
                return true;
            }

            if (cells[startX][startY].gem.GetId() == cells[releaseX][releaseY].gem.GetId())
            {
                return false;
            }

            SwapGems(startX, startY, releaseX, releaseY);

            bool canSwap = TestForMatchOnPosition(startX, startY, matchPrecedenceList) != null ||
                TestForMatchOnPosition(releaseX, releaseY, matchPrecedenceList) != null;

            SwapGems(startX, startY, releaseX, releaseY);
            return canSwap;
        }

        public void SwapGems(int oldPosX, int oldPosy, int newPosX, int newPosY)
        {
            string id = cells[oldPosX][oldPosy].gem.GetId();
            cells[oldPosX][oldPosy].gem.SetId(cells[newPosX][newPosY].gem.GetId());
            cells[newPosX][newPosY].gem.SetId(id);
        }

        public void RemoveGems(List<Explosion> toRemove)
        {
            if (toRemove == null)
            {
                return;
            }
            for (int i = 0; i < toRemove.Count; ++i)
            {
                cells[toRemove[i].position.x][toRemove[i].position.y].empty = true;
            }
        }

        public void CreateGems(List<GemCreate> toCreate)
        {
            if (toCreate == null)
            {
                return;
            }
            for (int i = 0; i < toCreate.Count; ++i)
            {
                if (toCreate[i] == null)
                {
                    continue;
                }
                cells[toCreate[i].At.x][toCreate[i].At.y].SetGem(new BoardGem(toCreate[i].GemData.gemId, GemType.Special));
                lastCreatedSpecialPosittion = new Vector2Int(toCreate[i].At.x, toCreate[i].At.y);
                lastCreatedSpecialType = toCreate[i].GemData.gemId;
            }
        }

        public List<GemMove> RefillBoard(List<GemData> gemSet, LevelData levelData, bool _canSpawnBubbles)
        {
            InvalidateHint();
            bool canSpawnBubbles = _canSpawnBubbles && levelData.bubbleSpawnChance > 0;
            List<GemMove> gemMoves = new List<GemMove>();

            for (int i = 0; i < width; i++)
                for (int j = height - 1; j >= 0; j--)
                {
                    if (cells[i][j].empty)
                    {
                        //search for the first one available unde it
                        int bottom = j - 1;
                        while (bottom >= 0 && cells[i][bottom].empty)
                        {
                            bottom--;
                        }

                        if (bottom == -1) // whole column is empty
                        {
                            cells[i][j].empty = false;
                            if (i == lastCreatedSpecialPosittion.x && canCreateSpecial)
                            {
                                cells[i][j].gem = new BoardGem(lastCreatedSpecialType, GemType.Special);
                                lastCreatedSpecialPosittion.y = -(j + 1);
                            }
                            else
                            {
                                if (canSpawnBubbles && Random.Range(0f, 100f) < levelData.bubbleSpawnChance)
                                {
                                    cells[i][j].SetGem(new BoardGem("14", GemType.Bubble));
                                }
                                else
                                {
                                    cells[i][j].SetGem(new BoardGem(gemSet[Random.Range(0, gemSet.Count)].gemId));
                                }
                            }

                            gemMoves.Add(new GemMove(new Vector2Int(i, j), new Vector2Int(i, -(j + 1))));
                        }
                        else
                        {
                            cells[i][j].empty = false;
                            cells[i][j].gem = cells[i][bottom].gem;
                            cells[i][bottom].empty = true;
                            gemMoves.Add(new GemMove(new Vector2Int(i, j), new Vector2Int(i, bottom)));
                        }
                    }
                }
            canCreateSpecial = false;

            for (int i = 0; i < gemMoves.Count; ++i)
            {
                if (gemMoves[i].From == lastCreatedSpecialPosittion)
                {
                    lastCreatedSpecialPosittion = gemMoves[i].To;
                    break;
                }
            }

            return gemMoves;
        }

        public List<Explosion> BombBlast(int posX, int posY, int bombRadius = 1)
        {
            List<Explosion> toExplode = new List<Explosion>();

            if (bombRadius == 1)
            {
                for (int i = -1; i <= 1; ++i)
                    for (int j = -1; j <= 1; ++j)
                    {
                        if (!BoundaryTest(posX + i, posY + j))
                        {
                            continue;
                        }
                        if (cells[posX + i][posY + j].empty)
                        {
                            continue;
                        }
                        toExplode.Add(new Explosion()
                        {
                            position = new Vector2Int(posX + i, posY + j),
                            id = cells[posX + i][posY + j].gem.GetId()
                        });
                        cells[posX + i][posY + j].empty = true;
                    }
            }
            else if (bombRadius == 2)
            {
                List<List<Vector2Int>> possible4RadiusExplodes = new List<List<Vector2Int>>();
                int maxIndex = 0;
                int maxExplosionCount = -1;

                //test all 4 possible explosion radiuses for max impact

                List<List<int>> offsets = new List<List<int>>()
                {
                    new List<int>() {-2, 1, -2, 1},
                    new List<int>() {-2, 1, -1, 2},
                    new List<int>() {-1, 2, -2, 1},
                    new List<int>() {-1, 2, -1, 2}
                };

                for (int off = 0; off < offsets.Count; ++off)
                {
                    List<Vector2Int> shouldExplode = new List<Vector2Int>();
                    for (int i = offsets[off][0]; i <= offsets[off][1]; ++i)
                        for (int j = offsets[off][2]; j <= offsets[off][3]; ++j)
                        {
                            if (!BoundaryTest(posX + i, posY + j))
                            {
                                continue;
                            }
                            if (cells[posX + i][posY + j].empty)
                            {
                                continue;
                            }
                            shouldExplode.Add(new Vector2Int(posX + i, posY + j));
                        }
                    possible4RadiusExplodes.Add(shouldExplode);
                    if (shouldExplode.Count > maxExplosionCount)
                    {
                        maxExplosionCount = shouldExplode.Count;
                        maxIndex = off;
                    }
                }

                for (int i = 0; i < possible4RadiusExplodes[maxIndex].Count; ++i)
                {
                    toExplode.Add(new Explosion()
                    {
                        position = possible4RadiusExplodes[maxIndex][i],
                        id = cells[possible4RadiusExplodes[maxIndex][i].x][possible4RadiusExplodes[maxIndex][i].y].gem.GetId()
                    });
                    cells[possible4RadiusExplodes[maxIndex][i].x][possible4RadiusExplodes[maxIndex][i].y].empty = true;
                }
            }
            return toExplode;
        }

        public List<Explosion> HammerBlast(int posX, int posY, int rangeX, int rangeY)
        {
            List<Explosion> toExplode = new List<Explosion>();
            for (int i = -rangeX; i <= rangeX; ++i)
            {
                if (!BoundaryTest(posX + i, posY) || i == 0)
                {
                    continue;
                }
                if (cells[posX + i][posY].empty)
                {
                    continue;
                }
                toExplode.Add(
                    new Explosion()
                    {
                        position = new Vector2Int(posX + i, posY),
                        id = cells[posX + i][posY].gem.GetId()
                    });
                cells[posX + i][posY].empty = true;
            }
            for (int i = -rangeY; i <= rangeY; ++i)
            {
                if (!BoundaryTest(posX, posY + i) || i == 0)
                {
                    continue;
                }
                if (cells[posX][posY + i].empty)
                {
                    continue;
                }
                toExplode.Add(
                    new Explosion()
                    {
                        position = new Vector2Int(posX, posY + i),
                        id = cells[posX][posY + i].gem.GetId()
                    });
                cells[posX][posY + i].empty = true;
            }
            toExplode.Add(
                new Explosion()
                {
                    position = new Vector2Int(posX, posY),
                    id = cells[posX][posY].gem.GetId()
                });
            cells[posX][posY].empty = true;
            return toExplode;
        }

        public List<Explosion> LineBlast(int posX, int posY)
        {
            List<Explosion> toExplode = new List<Explosion>();
            for (int i = 0; i < width; ++i)
            {
                if (cells[i][posY].empty)
                {
                    continue;
                }
                toExplode.Add(
                    new Explosion()
                    {
                        position = new Vector2Int(i, posY),
                        id = cells[i][posY].gem.GetId()
                    });
                cells[i][posY].empty = true;
            }
            return toExplode;
        }

        public List<Explosion> ColumnBlast(int posX, int posY)
        {
            List<Explosion> toExplode = new List<Explosion>();
            for (int i = 0; i < height; ++i)
            {
                if (cells[posX][i].empty)
                {
                    continue;
                }
                toExplode.Add(
                    new Explosion()
                    {
                        position = new Vector2Int(posX, i),
                        id = cells[posX][i].gem.GetId()
                    });
                cells[posX][i].empty = true;
            }
            return toExplode;
        }

        public bool IsRowEmpty(int row)
        {
            bool empty = true;
            for (int i = 0; i < width; ++i)
            {
                empty = empty && cells[i][row].empty;
            }
            return empty;
        }

        public bool IsColumnEmpty(int column)
        {
            bool empty = true;
            for (int i = 0; i < height; ++i)
            {
                empty = empty && cells[column][i].empty;
            }
            return empty;
        }


        public List<Vector2Int> GetPossibleColorChanges(string targetColor)
        {
            List<Vector2Int> possibleChanges = new List<Vector2Int>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    if (cells[i][j].empty ||
                        cells[i][j].gem.IsSpecial() ||
                        cells[i][j].gem.GetId() == targetColor ||
                        cells[i][j].gem.IsBubble())
                    {
                        continue;
                    }
                    possibleChanges.Add(new Vector2Int(i, j));
                }

            return possibleChanges;
        }

        public List<Explosion> GetAllById(string targetColor)
        {
            List<Explosion> gemsOfColor = new List<Explosion>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    if (cells[i][j].empty ||
                        cells[i][j].gem.IsSpecial() ||
                        cells[i][j].gem.GetId() != targetColor)
                    {
                        continue;
                    }
                    gemsOfColor.Add(new Explosion()
                    {
                        position = new Vector2Int(i, j),
                        id = targetColor
                    });
                }

            return gemsOfColor;

        }

        public void ApplyColorChanges(List<Vector2Int> toChange, string targetColor, List<Vector2Int> exclusionList = null)
        {
            for (int i = 0; toChange != null && i < toChange.Count; ++i)
            {
                if (cells[toChange[i].x][toChange[i].y].empty)
                {
                    continue;
                }
                if (exclusionList != null && exclusionList.Contains(toChange[i]))
                {
                    continue;
                }
                cells[toChange[i].x][toChange[i].y].gem.SetId(targetColor);
            }
        }

        public List<Explosion> BoardBlast()
        {
            DisableSpecialBug();
            List<Explosion> toExplode = new List<Explosion>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    toExplode.Add(new Explosion()
                    {
                        position = new Vector2Int(i, j),
                        id = cells[i][j].gem.GetId()
                    });
                    cells[i][j].empty = true;
                }
            return toExplode;
        }

        public bool HasPossibleMove(List<MatchShape> matchPrecedenceList)
        {
            Vector2Int hint = GetHint(matchPrecedenceList);
            return hint != -Vector2Int.one;
        }

        public void UpdateHint(List<MatchShape> matchPrecedenceList)
        {
            InvalidateHint();
            GetHint(matchPrecedenceList);
        }

        public Hint GetHint()
        {
            return hint;
        }


        public bool HasSpecials()
        {
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    if (cells[i][j].gem.IsSpecial())
                    {
                        return true;
                    }
                }
            return false;
        }

        public Vector2Int GetHint(List<MatchShape> matchPrecedenceList)
        {
            if (hintPosition != -Vector2Int.one)
            {
                return hintPosition;
            }

            List<Vector2Int> swapOffsets = new List<Vector2Int>()
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
            };

            List<Vector2Int> possiblePositions = new List<Vector2Int>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    possiblePositions.Add(new Vector2Int(i, j));
                }

            while (possiblePositions.Count > 0)
            {
                int randomIndex = Random.Range(0, possiblePositions.Count);
                int i = possiblePositions[randomIndex].x;
                int j = possiblePositions[randomIndex].y;
                possiblePositions.RemoveAt(randomIndex);

                foreach (Vector2Int offset in swapOffsets)
                {
                    if (!BoundaryTest(i + offset.x, j + offset.y))
                    {
                        continue;
                    }

                    SwapGems(i, j, i + offset.x, j + offset.y);

                    MatchTestResult match = TestForMatchOnPosition(i, j, matchPrecedenceList, null);
                    if (match != null)
                    {
                        hintPosition = new Vector2Int(i + offset.x, j + offset.y);
                        SwapGems(i, j, i + offset.x, j + offset.y);

                        hint.pos1 = new Vector2Int(i, j);
                        hint.pos2 = new Vector2Int(i + offset.x, j + offset.y);
                        List<Vector2Int> tilesToMatch = new List<Vector2Int>(match.match);
                        tilesToMatch.Remove(hint.pos1);
                        tilesToMatch.Add(hint.pos2);
                        hint.match = tilesToMatch;
                        hint.isSpecial = false;

                        return hintPosition;
                    }
                    else
                    {
                        match = TestForMatchOnPosition(i + offset.x, j + offset.y, matchPrecedenceList, null);
                        if (match != null)
                        {
                            hintPosition = new Vector2Int(i, j);
                            SwapGems(i, j, i + offset.x, j + offset.y);
                            hint.pos1 = new Vector2Int(i, j);
                            hint.pos2 = new Vector2Int(i + offset.x, j + offset.y);
                            List<Vector2Int> tilesToMatch = new List<Vector2Int>(match.match);
                            tilesToMatch.Remove(hint.pos2);
                            tilesToMatch.Add(hint.pos1);
                            hint.match = tilesToMatch;
                            hint.isSpecial = false;
                            return hintPosition;
                        }
                    }
                    SwapGems(i, j, i + offset.x, j + offset.y);
                }
            }

            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    if (cells[i][j].gem.IsSpecial())
                    {
                        hintPosition = new Vector2Int(i, j);
                        hint.pos1 = hintPosition;
                        hint.isSpecial = true;
                        return hintPosition;
                    }
                }


            return hintPosition;
        }

        private void InvalidateHint()
        {
            hintPosition = -Vector2Int.one;
        }

        public void Shuffle()
        {
            DisableSpecialBug();
            List<string> gemIds = new List<string>();

            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    gemIds.Add(cells[i][j].gem.GetId());
                }

            //fisher-yates shuffle
            int n = gemIds.Count;
            System.Random rng = new System.Random();
            while (n > 1)
            {
                n--;
                int index = rng.Next(n + 1);
                string val = gemIds[index];
                gemIds[index] = gemIds[n];
                gemIds[n] = val;
            }

            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    cells[i][j].gem.SetId(gemIds[0]);
                    gemIds.RemoveAt(0);
                }
        }
    }

    public class Hint
    {
        public Vector2Int pos1;
        public Vector2Int pos2;
        public bool isSpecial;

        public List<Vector2Int> match;
    }
}



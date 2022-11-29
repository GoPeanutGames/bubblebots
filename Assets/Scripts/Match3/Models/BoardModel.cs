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
        }

        public List<BoardCell> this[int i]
        {
            get => cells[i];
            set => cells[i] = value;
        }

        public void RandomizeGem(int posX, int posY, List<int> gemSet, bool excludeCurrent = true)
        {
            List<int> possibleValues = new List<int>(gemSet);
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
            MatchTestResult matchTestResult = new MatchTestResult();
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
                        exclusionList.Contains(new Vector2Int(posX + offsetX, posY + offsetY))) {
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
                    if (shape.matchOutcome != -1)
                    {
                        matchTestResult.outcome = new GemCreate(new Vector2Int(posX, posY), shape.matchOutcome);
                    }
                    return matchTestResult;
                }
            }

            return null;
        }

        public bool CanSwap(int startX, int startY, int releaseX, int releaseY, List<MatchShape> matchPrecedenceList)
        {
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
            int id = cells[oldPosX][oldPosy].gem.GetId();
            cells[oldPosX][oldPosy].gem.SetId(cells[newPosX][newPosY].gem.GetId());
            cells[newPosX][newPosY].gem.SetId(id);
        }

        public void RemoveGems(List<Vector2Int> toRemove)
        {
            if (toRemove == null)
            {
                return;
            }
            for (int i = 0; i < toRemove.Count; ++i)
            {
                cells[toRemove[i].x][toRemove[i].y].empty = true;
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
                cells[toCreate[i].At.x][toCreate[i].At.y].SetGem(new BoardGem(toCreate[i].Id, GemType.Special));
            }
        }

        public List<GemMove> RefillBoard(List<int> gemSet)
        {
            List<GemMove> gemMoves = new List<GemMove>();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (cells[i][j].empty)
                    {
                        //search for the first one available on top of it
                        int top = j + 1;
                        while (top < height && cells[i][top].empty)
                        {
                            top++;
                        }

                        if (top == height) // whole column is empty
                        {
                            cells[i][j].empty = false;
                            cells[i][j].gem = new BoardGem(gemSet[Random.Range(0, gemSet.Count)]);
                            gemMoves.Add(new GemMove(new Vector2Int(i, j), new Vector2Int(i, j + height)));
                        }
                        else
                        {
                            cells[i][j].empty = false;
                            cells[i][j].gem = cells[i][top].gem;
                            cells[i][top].empty = true;
                            gemMoves.Add(new GemMove(new Vector2Int(i, j), new Vector2Int(i, top)));
                        }
                    }
                }
            return gemMoves;
        }

        public List<Vector2Int> BombBlast(int posX, int posY)
        {
            List<Vector2Int> toExplode = new List<Vector2Int>();
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
                    toExplode.Add(new Vector2Int(posX + i, posY + j));
                    cells[posX + i][posY + j].empty = true;

                }
            return toExplode;
        }

        public List<Vector2Int> HammerBlast(int posX, int posY, int rangeX, int rangeY)
        {
            List<Vector2Int> toExplode = new List<Vector2Int>();
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
                toExplode.Add(new Vector2Int(posX + i, posY));
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
                toExplode.Add(new Vector2Int(posX, posY + i));
                cells[posX][posY + i].empty = true;
            }
            toExplode.Add(new Vector2Int(posX, posY));
            cells[posX][posY].empty = true;
            return toExplode;
        }

        public List<Vector2Int> LineBlast(int posX, int posY)
        {
            List<Vector2Int> toExplode = new List<Vector2Int>();
            for (int i = 0; i < width; ++i)
            {
                if (cells[i][posY].empty)
                {
                    continue;
                }
                toExplode.Add(new Vector2Int(i, posY));
                cells[i][posY].empty = true;
            }
            return toExplode;
        }

        public List<Vector2Int> ColumnBlast(int posX, int posY)
        {
            List<Vector2Int> toExplode = new List<Vector2Int>();
            for (int i = 0; i < height; ++i)
            {
                if (cells[posX][i].empty)
                {
                    continue;
                }
                toExplode.Add(new Vector2Int(posX, i));
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


        public List<Vector2Int> GetPossibleColorChanges(int targetColor)
        {
            List<Vector2Int> possibleChanges = new List<Vector2Int>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    if (cells[i][j].empty ||
                        cells[i][j].gem.IsSpecial() ||
                        cells[i][j].gem.GetId() == targetColor)
                    {
                        continue;
                    }
                    possibleChanges.Add(new Vector2Int(i, j));
                }

            return possibleChanges;
        }

        public List<Vector2Int> GetAllById(int targetColor)
        {
            List<Vector2Int> gemsOfColor = new List<Vector2Int>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    if (cells[i][j].empty ||
                        cells[i][j].gem.IsSpecial() ||
                        cells[i][j].gem.GetId() != targetColor)
                    {
                        continue;
                    }
                    gemsOfColor.Add(new Vector2Int(i, j));
                }

            return gemsOfColor;

        }

        public void ApplyColorChanges(List<Vector2Int> toChange, int targetColor, List<Vector2Int> exclusionList = null)
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


        public List<Vector2Int> BoardBlast()
        {
            List<Vector2Int> toExplode = new List<Vector2Int>();
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    toExplode.Add(new Vector2Int(i, j));
                    cells[i][j].empty = true;
                }
            return toExplode;
        }
    }
}


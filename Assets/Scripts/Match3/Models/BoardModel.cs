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

        public MatchTestResult TestForMatchOnPosition(int posX, int posY, List<MatchShape> matchPrecedenceList)
        {
            for (int i = matchPrecedenceList.Count - 1; i >= 0; i--)
            {
                MatchTestResult matchResult = TestMatch(posX, posY, matchPrecedenceList[i]);
                if (matchResult != null)
                {
                    return matchResult;
                }
            }
            return null;
        }

        public MatchTestResult TestMatch(int posX, int posY, MatchShape shape)
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
            for (int i = 0; i < toRemove.Count; ++i)
            {
                cells[toRemove[i].x][toRemove[i].y].empty = true;
            }
        }

        public void CreateGems(List<GemCreate> toCreate)
        {
            for (int i = 0; i < toCreate.Count; ++i)
            {
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
    }
}


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


            if (boardModel[startX][startY].gem.IsSpecial() &&
                boardModel[releaseX][releaseY].gem.IsSpecial())
            {
                //special - special match;
            }
            else
            {

                if (boardModel[startX][startY].gem.IsSpecial())
                {
                    swapResult.toExplode.Add(new Vector2Int(startX, startY));
                }
                if (boardModel[releaseX][releaseY].gem.IsSpecial())
                {
                    swapResult.toExplode.Add(new Vector2Int(releaseX, releaseY));
                }
            }

            //explode specials consequences

            boardModel.RemoveGems(swapResult.toExplode);
            boardModel.CreateGems(swapResult.toCreate);
            return swapResult;
        }

        public SwapResult SwapSpecial(int startX, int startY, int releaseX, int releaseY)
        {
            boardModel.SwapGems(startX, startY, releaseX, releaseY);

            if (boardModel[startX][startY].gem.IsSpecial() &&
                boardModel[releaseX][releaseY].gem.IsSpecial())
            {
                //special - special match;
            }
            SwapResult swapResult = ExplodeMatches();
            
            if (boardModel[startX][startY].gem.IsSpecial())
            {
                swapResult.toExplode.Add(new Vector2Int(startX, startY));
            }
            if (boardModel[releaseX][releaseY].gem.IsSpecial())
            {
                swapResult.toExplode.Add(new Vector2Int(releaseX, releaseY));
            }

            return swapResult;
        }

        public void ExplodeSpecial(int posX, int posY)
        {

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
                        if (matchTestResult.outcome != null)
                        {
                            swapResult.toCreate.Add(matchTestResult.outcome);
                        }
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

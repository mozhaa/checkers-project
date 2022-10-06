using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Шашки_по_городу
{
    public enum Player
    {
        white,
        black
    }
    internal class Presenter
    {

        public const int rows = 8;
        public const int columns = 8;
        private readonly Player?[,] board = new Player?[rows, columns];
        private readonly IBoardView view;
        private List<Tuple<int, int>> chainMove = new List<Tuple<int, int>>();
        private Player currentPlayer;
        private Dictionary<Player, int> checkersCount = new Dictionary<Player, int>
        {
            [Player.white] = 0,
            [Player.black] = 0
        };
        private bool isPlaying = false;

        public Presenter(IBoardView view)
        {
            this.view = view;
            this.view.SetPresenter(this);
        }

        public void Start()
        {
            currentPlayer = Player.white;
            isPlaying = true;
            view.ClearGrid();
            for(int row = 0; row < rows; row++)
            {
                for(int column = 0; column < columns; column++)
                {
                    switch(row){
                        case 0: 
                        case 1:
                        case 2:
                            if ((row + column) % 2 != 0)
                            {
                                board[row, column] = Player.white;
                                checkersCount[Player.white]++;
                            }
                            break;
                        case 5:
                        case 6: 
                        case 7:
                            if ((row + column) % 2 != 0)
                            {
                                board[row, column] = Player.black;
                                checkersCount[Player.black]++;
                            }
                            break;
                        default:
                            board[row, column] = null;
                            break;
                    }
                }
            }
            ShowBoard();
        }

        internal void MouseDown(int row, int column)
        {
            if (!isPlaying) { return; }
            if(chainMove.Count > 0)
            {
                var selectedChecker = chainMove[0];
                var possibleExpectedChain = new List<Tuple<int, int>>(chainMove);
                possibleExpectedChain.Add(new Tuple<int, int>(row, column));

                if (selectedChecker.Item1 == row && selectedChecker.Item2 == column && GetCheckersToEat(possibleExpectedChain) == null)
                {
                    for (int i = 0; i < chainMove.Count; i++)
                    {
                        view.DehighlightTile(chainMove[i].Item1, chainMove[i].Item2);
                    }
                    chainMove.Clear();
                    Trace.WriteLine($"Deselect tile, you can select new one");
                }
                else
                {
                    if (TryToMove(row, column) == 0)
                    {
                        for(int i = 0; i < chainMove.Count; i++)
                        {
                            view.DehighlightTile(chainMove[i].Item1, chainMove[i].Item2);
                        }
                        Trace.WriteLine($"chainmovecount: {chainMove.Count}");
                        chainMove.Clear();
                        currentPlayer = (currentPlayer == Player.white) ? Player.black : Player.white;
                        Trace.WriteLine($"Current Player: {currentPlayer}");
                    }
                    else
                    if (TryToMove(row, column) == 2)
                    {
                        chainMove.Add(new Tuple<int, int>(row, column));
                        view.HighlightChainedTile(row, column);
                        Trace.WriteLine("Chained new tile into chain move");
                    }
                }
            }
            else
            {
                if(board[row, column].HasValue && board[row, column].Value == currentPlayer)
                {
                    Trace.WriteLine("Selected new checker to move");
                    view.HighlightTile(row, column);
                    chainMove.Add(new Tuple<int, int>(row, column));
                }
                else
                {
                    Trace.WriteLine("Selected tile is empty or wrong color");
                }
            }
            if (checkersCount[Player.white] == 0 || checkersCount[Player.black] == 0)
            {
                GameOver();
            }
        }

        private HashSet<Tuple<int, int>> GetCheckersToEat(List<Tuple<int, int>> expectedChain)
        {
            var row = expectedChain[expectedChain.Count - 1].Item1;
            var column = expectedChain[expectedChain.Count - 1].Item2;
            if (row < 0 || row > 7 || column < 0 || column > 7)
            {
                return null;
            }
            if (board[row, column].HasValue && (row != expectedChain[0].Item1 || column != expectedChain[0].Item2))
            {
                return null;
            }

            var checkersToEat = new HashSet<Tuple<int, int>>();
            for (int i = 1; i < expectedChain.Count; i++)
            {
                if (Math.Abs(expectedChain[i - 1].Item1 - expectedChain[i].Item1) == 2 &&
                    Math.Abs(expectedChain[i - 1].Item2 - expectedChain[i].Item2) == 2)
                {
                    var tileToEat = new Tuple<int, int>((expectedChain[i - 1].Item1 + expectedChain[i].Item1) / 2,
                                                        (expectedChain[i - 1].Item2 + expectedChain[i].Item2) / 2);

                    if (board[tileToEat.Item1, tileToEat.Item2].HasValue &&
                        board[tileToEat.Item1, tileToEat.Item2].Value == ((currentPlayer == Player.black) ? Player.white : Player.black))
                    {
                        if (!checkersToEat.Contains(tileToEat))
                        {
                            checkersToEat.Add(tileToEat);
                            continue;
                        }
                        else
                        {
                            Trace.WriteLine($"[({row}, {column})]: You trying to eat one checker two times");
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"[({row}, {column})]: You trying to jump over empty tile or your checker");
                    }
                }
                else
                {
                    Trace.WriteLine($"[({row}, {column})]: You trying to jump more than 2 tiles diagonally");
                }
                return null; // Invalid move
            }

            return checkersToEat; // Valid move
        }

        private int TryToChainMove(List<Tuple<int, int>> expectedChain)
        {
            var row = expectedChain[expectedChain.Count - 1].Item1;
            var column = expectedChain[expectedChain.Count - 1].Item2;

            var checkersToEat = GetCheckersToEat(expectedChain);
            if(checkersToEat == null)
            {
                return 1; // Invalid move
            }

            var possibleMoves = new List<Tuple<int, int>> {
                new Tuple<int, int>(row - 2, column - 2),
                new Tuple<int, int>(row - 2, column + 2),
                new Tuple<int, int>(row + 2, column - 2),
                new Tuple<int, int>(row + 2, column + 2)
            };
            foreach (var moveToAdd in possibleMoves)
            {
                var possibleExpectedChain = new List<Tuple<int, int>>(expectedChain);
                possibleExpectedChain.Add(moveToAdd);
                if(GetCheckersToEat(possibleExpectedChain) != null)
                {
                    return 2; // Move is not complete
                }
            } 

            foreach(var checkerToEat in checkersToEat)
            {
                Trace.WriteLine($"Eated checker on ({checkerToEat.Item1}, {checkerToEat.Item2})");
                view.RemoveCheckerFromGrid(checkerToEat.Item1, checkerToEat.Item2);
                board[checkerToEat.Item1, checkerToEat.Item2] = null;
            }
            var tileFromMove = chainMove[0];
            var tileToMove = new Tuple<int, int>(row, column);
            Trace.WriteLine("Move contains of:");
            foreach(var chainmovetile in chainMove)
            {
                Trace.WriteLine($"({chainmovetile.Item1}, {chainmovetile.Item2})");
            }
            Trace.WriteLine($"Moving checker from ({tileFromMove.Item1}, {tileFromMove.Item2}) to ({tileToMove.Item1}, {tileToMove.Item2})");
            view.MoveChecker(tileFromMove.Item1, tileFromMove.Item2, tileToMove.Item1, tileToMove.Item2);
            board[tileFromMove.Item1, tileFromMove.Item2] = null;
            board[tileToMove.Item1, tileToMove.Item2] = currentPlayer;
            return 0; // Complete move

        }

        private int TryToMove(int row, int column)
        {
            if (board[row, column] != null && (row != chainMove[0].Item1 || column != chainMove[0].Item2))
            {
                Trace.WriteLine("Tile to move is not empty, can't move checker into it");
                return 1; // Invalid move
            }
            if (row < 0 || row > 7 || column < 0 || column > 7)
            {
                Trace.WriteLine("Wrong row or column, out of board");
                return 1; // Invalid move
            }
            if(TrySimpleMove(row, column))
            {
                Trace.WriteLine("Simple move is valid, make move");
                return 0; // Valid move
            }
            if(TryToChainMove(new List<Tuple<int, int>>(chainMove) { new Tuple<int, int>(row, column) }) == 0)
            {
                Trace.WriteLine("Chain move is complete, make move");
                checkersCount[((currentPlayer == Player.white) ? Player.black : Player.white)] -= chainMove.Count;
                Trace.WriteLine($"Black: {checkersCount[Player.black]}, White: {checkersCount[Player.white]}");
                return 0; // Valid move
            }
            if(TryToChainMove(new List<Tuple<int, int>>(chainMove) { new Tuple<int, int>(row, column) }) == 2)
            {
                Trace.WriteLine("Chain move is not complete, selected new tile");
                return 2; // Incomplete chain move
            }
            return 1;
        }

        private bool TrySimpleMove(int row, int column)
        {
            var selectedChecker = chainMove[0];
            if(chainMove.Count > 1)
            {
                return false;
            }
            var validRow = (currentPlayer == Player.white) ? selectedChecker.Item1 + 1 : selectedChecker.Item1 - 1;
            if (row == validRow && (column == selectedChecker.Item2 - 1 || column == selectedChecker.Item2 + 1))
            {
                view.MoveChecker(selectedChecker.Item1, selectedChecker.Item2, row, column);
                board[row, column] = currentPlayer;
                board[selectedChecker.Item1, selectedChecker.Item2] = null;
                return true;
            }
            Trace.WriteLine("Simple move is not valid, try another tile to move");
            return false;
        }

        private void GameOver()
        {
            if (checkersCount[Player.white] == 0)
            {
                Trace.WriteLine("Black wins!");
                isPlaying = false;
            }
            else
            {
                Trace.WriteLine("White wins!");
                isPlaying = false;
            }
        }

        private void ShowBoard()
        {
            for(int row = 0; row < rows; row++)
            {
                for(int column = 0; column < columns; column++)
                {
                    if (board[row, column].HasValue)
                    {
                        view.AddCheckerToGrid(row, column, board[row, column].Value);
                    }
                }
            }
        }
    }
}

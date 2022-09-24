using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        private Tuple<int, int> selectedChecker;
        private Player currentPlayer;

        public Presenter(IBoardView view)
        {
            this.view = view;
            this.view.SetPresenter(this);
        }

        public void Start()
        {
            currentPlayer = Player.white;
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
                            }
                            break;
                        case 5:
                        case 6: 
                        case 7:
                            if ((row + column) % 2 != 0)
                            {
                                board[row, column] = Player.black;
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
            if(selectedChecker != null)
            {
                if (selectedChecker.Item1 == row && selectedChecker.Item2 == column)
                {
                    view.DehighlightTile(selectedChecker.Item1, selectedChecker.Item2);
                    selectedChecker = null;
                    Trace.WriteLine($"Deselect tile, you can select new one");
                }
                else
                {
                    if (TryToMove(selectedChecker, row, column))
                    {
                        view.DehighlightTile(selectedChecker.Item1, selectedChecker.Item2);
                        selectedChecker = null;
                        currentPlayer = (currentPlayer == Player.white) ? Player.black : Player.white;
                        Trace.WriteLine($"Current Player: {currentPlayer}");
                    }
                }
            }
            else
            {
                if(board[row, column].HasValue && board[row, column].Value == currentPlayer)
                {
                    Trace.WriteLine("Selected new checker to move");
                    view.HighlightTile(row, column);
                    selectedChecker = new Tuple<int, int>(row, column);
                }
                else
                {
                    Trace.WriteLine("Selected tile is empty or wrong color");
                }
            }
        }

        private bool TryToMove(Tuple<int, int> selectedChecker, int row, int column)
        {
            if (board[row, column] != null)
            {
                Trace.WriteLine("Tile to move is not empty, can't move checker into it");
                return false;
            }
            if (row < 0 || row > 7 || column < 0 || column > 7)
            {
                Trace.WriteLine("Wrong row or column, out of board");
                return false;
            }
            if(TrySimpleMove(selectedChecker, row, column, currentPlayer))
            {
                Trace.WriteLine("Simple move is valid, make move");
                return true;
            }
            return false;
        }

        private bool TrySimpleMove(Tuple<int, int> selectedChecker, int row, int column, Player player)
        {
            var validRow = (currentPlayer == Player.white) ? selectedChecker.Item1 + 1 : selectedChecker.Item1 - 1;
            if (row == validRow && (column == selectedChecker.Item2 - 1 || column == selectedChecker.Item2 + 1))
            {
                view.MoveChecker(selectedChecker.Item1, selectedChecker.Item2, row, column);
                board[row, column] = player;
                board[selectedChecker.Item1, selectedChecker.Item2] = null;
                return true;
            }
            Trace.WriteLine("Simple move is not valid, try another tile to move");
            return false;
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

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
    //internal class Checker
    //{
    //    public int row { get; set; }
    //    public int column { get; set; }
    //    public Player player { get; set; } 
    //}
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
                if (TryToMove(selectedChecker, row, column))
                {
                    selectedChecker = null;
                    currentPlayer = (currentPlayer == Player.white) ? Player.black : Player.white;
                    Trace.WriteLine($"Current Player: {currentPlayer}");
                }
            }
            else
            {
                if(board[row, column].HasValue && board[row, column].Value == currentPlayer)
                {
                    selectedChecker = new Tuple<int, int>(row, column);
                }
            }
        }

        private bool TryToMove(Tuple<int, int> selectedChecker, int row, int column)
        {
            if (board[row, column] != null)
            {
                return false;
            }
            if (row < 0 || row > 7 || column < 0 || column > 7)
            {
                return false;
            }
            if(TrySimpleMove(selectedChecker, row, column, currentPlayer))
            {
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Шашки_по_городу
{
    enum Player
    {
        white,
        black,
        none
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
        
        Player[,] board = new Player[rows, columns];

        private void fillBoardOnStartUp()
        {
            for(int row = 0; row < rows; row++)
            {
                for(int column = 0; column < columns; column++)
                {
                    switch(row){
                        case 0: case 1:
                            board[row, column] = Player.black;
                            break;
                        case 6: case 7:
                            board[row, column] = Player.white;
                            break;
                        default:
                            board[row, column] = Player.none;
                            break;
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace Шашки_по_городу
{
    public enum Player
    {
        white,
        black
    }

    public static class Extensions
    {
        public static Player Opposite(this Player player)
        {
            return (player == Player.white) ? Player.black : Player.white;
        }

        public static int KingRow(this Player player)
        {
            return (player == Player.white) ? 7 : 0;
        }
    }

    public struct Checker
    {

        public Checker(Player player, bool isKing) : this()
        {
            this.Player = player;
            this.IsKing = isKing;
        }
        public static Checker Black()
        {
            return new Checker(Player.black, false);
        }
        public static Checker White()
        {
            return new Checker(Player.white, false);
        }


        public Player Player { get; set; }
        public bool IsKing { get; set; }
    }

    [DebuggerDisplay("{Row}, {Column}, {IsKing}")]
    public class ChainTile
    {
        public ChainTile(int row, int column)
        {
            Row = row;
            Column = column;
            IsKing = false;
        }

        public ChainTile(int row, int column, bool isKing)
        {
            Row = row;
            Column = column;
            IsKing = isKing;
        }

        public bool IsValid()
        {
            return !(Row < 0 || Row > 7 || Column < 0 || Column > 7);
        }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsKing { get; set; }
        public override bool Equals(object obj)
        {
            ChainTile tile = (ChainTile)obj;
            return Row == tile.Row && Column == tile.Column && IsKing == tile.IsKing;
        }
        public override int GetHashCode()
        {
            return Row.GetHashCode() + Column.GetHashCode() + IsKing.GetHashCode();
        }
    }

    /// <summary>
    /// Class, that presents object of chain move. It includes functions, that check if move is valid, if move can be extended, etc
    /// </summary>
    public struct ChainMove
    {
        public List<ChainTile> chainTiles;
        private Checker?[,] board;

        public ChainMove(List<ChainTile> chainTiles, Checker?[,] board)
        {
            this.chainTiles = chainTiles;
            this.board = board;
        }

        public ChainMove(ChainMove toCopy, ChainTile newTile)
        {
            var chainTiles = new List<ChainTile>();
            foreach(var elem in toCopy.chainTiles)
            {
                chainTiles.Add(elem);
            }
            chainTiles.Add(newTile);
            this.chainTiles = chainTiles;
            this.board = toCopy.board;
        }

        /// <summary>
        /// This function clears current chain
        /// </summary>
        public void Clear()
        {
            chainTiles.Clear();
        }

        /// <summary>
        /// This function checks if [row, column] is out of board or not
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool OutOfEdges(int row, int column)
        {
            return row < 0 || row > 7 || column < 0 || column > 7;
        }

        /// <summary>
        /// This function checks if [row, column] tile is another tile than first tile in chain, but it is not empty
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool AnotherTileButHasChecker(int row, int column)
        {
            var firstTile = chainTiles[0];
            return board[row, column].HasValue && (row != firstTile.Row || column != firstTile.Column);
        }

        /// <summary>
        /// This function checks, if current chain is a valid chain move (move with eating opponent checkers) or not
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public bool IsValidChainMove(Player currentPlayer)
        {
            return GetCheckersToEat(currentPlayer) != null;
        }

        /// <summary>
        /// This function checks, if current chain is a valid simple move (move without eating opponent checkers) or not
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public bool IsValidSimpleMove(Player currentPlayer)
        {
            if (chainTiles.Count != 2 || !chainTiles[chainTiles.Count - 1].IsValid() || board[chainTiles[1].Row, chainTiles[1].Column].HasValue)
            {
                return false;
            }
            ChainTile selectedChecker = chainTiles[0], checkerToMove = chainTiles[1];
            var moveLeg = Math.Abs(checkerToMove.Column - selectedChecker.Column);
            var validRow = (currentPlayer == Player.white) ? selectedChecker.Row + 1 : selectedChecker.Row - 1;
            if ((selectedChecker.IsKing && 
                (checkerToMove.Row == selectedChecker.Row + moveLeg || 
                checkerToMove.Row == selectedChecker.Row - moveLeg)) || 
                (!selectedChecker.IsKing && checkerToMove.Row == validRow))
            {
                for (int i = 1; i < moveLeg; i++)
                {
                    var consideredTile = board[(checkerToMove.Row > selectedChecker.Row) ? selectedChecker.Row + i : selectedChecker.Row - i,
                              (checkerToMove.Column > selectedChecker.Column) ? selectedChecker.Column + i : selectedChecker.Column - i];
                    if (consideredTile.HasValue)
                    {
                        Trace.WriteLine($"[{checkerToMove.Row}, {checkerToMove.Column}]: Simple move is not valid, try another tile to move");
                        return false;
                    }
                }
                return true;
            }
            Trace.WriteLine($"[{checkerToMove.Row}, {checkerToMove.Column}]: Simple move is not valid, try another tile to move");
            return false;
        }

        /// <summary>
        /// This function returns HashSet of checkers, that player will eat, moving by current chain. If current chain is not a valid move, function returns null
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public HashSet<ChainTile> GetCheckersToEat(Player currentPlayer)
        {
            var row = chainTiles[chainTiles.Count - 1].Row;
            var column = chainTiles[chainTiles.Count - 1].Column;
            if (OutOfEdges(row, column) || AnotherTileButHasChecker(row, column))
            {
                return null;
            }

            var checkersToEat = new HashSet<ChainTile>();
            for (int i = 1; i < chainTiles.Count; i++)
            {
                ChainTile prevChainTile = chainTiles[i - 1];
                ChainTile currentChainTile = chainTiles[i];
                if (prevChainTile.IsKing || currentChainTile.Row == currentPlayer.KingRow())
                {
                    currentChainTile.IsKing = true;
                }
                if (!prevChainTile.IsKing)
                {
                    if (!(Math.Abs(prevChainTile.Row - currentChainTile.Row) == 2 &&
                        Math.Abs(prevChainTile.Column - currentChainTile.Column) == 2))
                    {
                        Trace.WriteLine($"[({row}, {column})]: You trying to jump more than 2 tiles diagonally");
                        return null;
                    }
                    var tileToEat = new ChainTile((prevChainTile.Row + currentChainTile.Row) / 2,
                                                    (prevChainTile.Column + currentChainTile.Column) / 2);
                    if(!board[tileToEat.Row, tileToEat.Column].HasValue)
                    {
                        Trace.WriteLine($"[({row}, {column})]: Can't jump over empty tile");
                        return null;
                    }
                    if (board[tileToEat.Row, tileToEat.Column].Value.Player != currentPlayer.Opposite())
                    {
                        Trace.WriteLine($"[({row}, {column})]: You trying to jump over empty tile or your checker");
                        return null;
                    }
                    if (checkersToEat.Contains(tileToEat))
                    {
                        Trace.WriteLine($"[({row}, {column})]: You trying to eat one checker two times");
                        return null;
                    }
                    checkersToEat.Add(tileToEat);
                }
                else
                {
                    if (Math.Abs(prevChainTile.Row - currentChainTile.Row) != Math.Abs(prevChainTile.Column - currentChainTile.Column))
                    {
                        return null;
                    }
                    ChainTile tileToEat = null;
                    for(int j = 1; j < Math.Abs(prevChainTile.Row - currentChainTile.Row); j++)
                    {
                        var consideredTile = new ChainTile(prevChainTile.Row + ((currentChainTile.Row > prevChainTile.Row) ? j : (-j)),
                                                  prevChainTile.Column + ((currentChainTile.Column > prevChainTile.Column) ? j : (-j)));
                        if (!board[consideredTile.Row, consideredTile.Column].HasValue)
                        {
                            continue;
                        }
                        if (tileToEat != null)
                        {
                            Trace.WriteLine($"[({row}, {column})]: Can't eat 2 checkers at the same time");
                            return null;
                        }
                        if (board[consideredTile.Row, consideredTile.Column].Value.Player != currentPlayer.Opposite())
                        {
                            Trace.WriteLine($"[({row}, {column})]: Can't eat checker of your color");
                            return null;
                        }
                        if (checkersToEat.Contains(consideredTile)) 
                        {
                            Trace.WriteLine($"[({row}, {column})]: Can't eat 1 checker 2 times");
                            return null;
                        }
                        tileToEat = consideredTile;
                    }
                    if(tileToEat == null)
                    {
                        Trace.WriteLine($"[({row}, {column})]: Can't jump over empty tiles");
                        return null;
                    }
                    checkersToEat.Add(tileToEat);
                }
            }

            return checkersToEat; // Valid move
        }

        /// <summary>
        /// This function checks, can we extend current chain with move to [row, column] tile
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public int TryAddMove(int row, int column, Player currentPlayer)
        {
            var expectedChain = new ChainMove(this, new ChainTile(row, column));
            var checkersToEat = expectedChain.GetCheckersToEat(currentPlayer);
            if (checkersToEat == null)
            {
                return 1; // Invalid move
            }
            for(int i = 2; i < 8; i++)
            {
                var newPossibleTiles = new List<ChainTile> {
                    new ChainTile(row - i, column - i),
                    new ChainTile(row - i, column + i),
                    new ChainTile(row + i, column - i),
                    new ChainTile(row + i, column + i)
                };
                foreach(var possibleTile in newPossibleTiles)
                {
                    if (possibleTile.IsValid())
                    {
                        var possibleExpectedChain = new ChainMove(expectedChain, possibleTile);
                        if (possibleExpectedChain.GetCheckersToEat(currentPlayer) != null)
                        {
                            return 2; // Move is not complete
                        }
                    }
                }
            }

            return 0; // Move is complete
        }

        /// <summary>
        /// This function check, can current chain move be continued or not
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <returns></returns>
        public bool DoesMoveExist(Player currentPlayer)
        {
            var lastTile = chainTiles[chainTiles.Count - 1];
            for (int i = 2; i < 8; i++)
            {
                var newPossibleTiles = new List<ChainTile> {
                    new ChainTile(lastTile.Row - i, lastTile.Column - i),
                    new ChainTile(lastTile.Row - i, lastTile.Column + i),
                    new ChainTile(lastTile.Row + i, lastTile.Column - i),
                    new ChainTile(lastTile.Row + i, lastTile.Column + i)
                };
                foreach (var possibleTile in newPossibleTiles)
                {
                    if (possibleTile.IsValid())
                    {
                        var possibleExpectedChain = new ChainMove(this, possibleTile);
                        if (possibleExpectedChain.GetCheckersToEat(currentPlayer) != null || possibleExpectedChain.IsValidSimpleMove(currentPlayer))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    }

    internal class Presenter
    {

        public const int rows = 8; // Number of rows
        public const int columns = 8; // Number of columns
        private readonly Checker?[,] board = new Checker?[rows, columns]; // Array of checkers, that are currently on the board
        private readonly IBoardView view; // IBoardView object, that connects current presenter with window
        private ChainMove currentChain; // List of tiles, that current player selected on the board
        private Player currentPlayer; // Current player (black or white) 
        private Dictionary<Player, int> checkersCount = new Dictionary<Player, int> // Dictionary, that stores number of checkers of particular color
        {
            [Player.white] = 0,
            [Player.black] = 0
        };
        private bool isPlaying = false; 

        public Presenter(IBoardView view)
        {
            currentChain = new ChainMove(new List<ChainTile>(), board);
            this.view = view;
            this.view.SetPresenter(this);
        }
        /// <summary>
        /// This function is called, when the window is opening, or user clicks button "New Game"
        /// </summary>
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
                                board[row, column] = Checker.White();
                                checkersCount[Player.white]++;
                            }
                            break;
                        case 5:
                        case 6: 
                        case 7:
                            if ((row + column) % 2 != 0)
                            {
                                board[row, column] = Checker.Black();
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
        /// <summary>
        /// This function checks, if current player can make a move
        /// </summary>
        /// <returns></returns>
        internal bool CanCurrentPlayerMove()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (board[row, column].HasValue && board[row, column].Value.Player == currentPlayer)
                    {
                        if ((new ChainMove(new List<ChainTile> { new ChainTile(row, column, board[row, column].Value.IsKing) }, board)).DoesMoveExist(currentPlayer))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// This function is called, when user clicks the window's Grid. It process click and make move if it's valid
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        internal void MouseDown(int row, int column)
        {
            if (!isPlaying) { return; }
            if(currentChain.chainTiles.Count > 0)
            {
                // Make a move
                var selectedChecker = currentChain.chainTiles[0];
                var possibleExpectedChain = new ChainMove(currentChain, new ChainTile(row, column));
                if (selectedChecker.Row == row && selectedChecker.Column == column && !possibleExpectedChain.IsValidChainMove(currentPlayer))
                {
                    // Chain Move is not valid, deselect option
                    for (int i = 0; i < currentChain.chainTiles.Count; i++)
                    {
                        view.DehighlightTile(currentChain.chainTiles[i].Row, currentChain.chainTiles[i].Column);
                    }
                    currentChain.chainTiles.Clear();
                    Trace.WriteLine($"Deselect tile, you can select new one");
                }
                else
                {
                    if (possibleExpectedChain.IsValidSimpleMove(currentPlayer))
                    {
                        // Simple move is valid
                        currentChain.chainTiles.Add(new ChainTile(row, column));
                        ChainTile tileToMove = currentChain.chainTiles[1];
                        view.MoveChecker(selectedChecker.Row, selectedChecker.Column, tileToMove.Row, tileToMove.Column);
                        view.DehighlightTile(selectedChecker.Row, selectedChecker.Column);
                        if (row == currentPlayer.KingRow())
                        {
                            selectedChecker.IsKing = true;
                            view.RefreshTile(tileToMove.Row, tileToMove.Column, new Checker(currentPlayer, true));
                        }
                        board[row, column] = new Checker(currentPlayer, selectedChecker.IsKing);
                        board[selectedChecker.Row, selectedChecker.Column] = null;
                        currentChain.Clear();
                        currentPlayer = currentPlayer.Opposite();
                        Trace.WriteLine($"Current Player: {currentPlayer}");
                    }
                    else
                    {
                        var moveResult = currentChain.TryAddMove(row, column, currentPlayer);
                        if (moveResult == 0)
                        {
                            // Chain move is complete

                            Trace.WriteLine("Ate checkers on:");
                            foreach (var checkerToEat in possibleExpectedChain.GetCheckersToEat(currentPlayer))
                            {
                                Trace.WriteLine($"  ({checkerToEat.Row}, {checkerToEat.Column})");
                                view.RemoveCheckerFromGrid(checkerToEat.Row, checkerToEat.Column);
                                checkersCount[currentPlayer.Opposite()]--;
                                board[checkerToEat.Row, checkerToEat.Column] = null;
                            }
                            Trace.WriteLine("Move contains of:");
                            foreach (var chainTile in currentChain.chainTiles)
                            {
                                Trace.WriteLine($"  ({chainTile.Row}, {chainTile.Column})");
                            }

                            var tileFromMove = currentChain.chainTiles[0];
                            var tileToMove = possibleExpectedChain.chainTiles[possibleExpectedChain.chainTiles.Count - 1];
                            Trace.WriteLine($"Moving checker from ({tileFromMove.Row}, {tileFromMove.Column}) to ({tileToMove.Row}, {tileToMove.Column})");
                            view.MoveChecker(tileFromMove.Row, tileFromMove.Column, tileToMove.Row, tileToMove.Column);
                            board[tileFromMove.Row, tileFromMove.Column] = null;
                            board[tileToMove.Row, tileToMove.Column] = new Checker(currentPlayer, tileToMove.IsKing);
                            if(board[tileToMove.Row, tileToMove.Column].Value.IsKing)
                            {
                                view.RefreshTile(tileToMove.Row, tileToMove.Column, board[tileToMove.Row, tileToMove.Column].Value);
                            }
                            for (int i = 0; i < currentChain.chainTiles.Count; i++)
                            {
                                view.DehighlightTile(currentChain.chainTiles[i].Row, currentChain.chainTiles[i].Column);
                            }
                            currentChain.chainTiles.Clear();
                            currentPlayer = currentPlayer.Opposite();
                            Trace.WriteLine($"Current Player: {currentPlayer}");
                        }
                        else
                        if (moveResult == 2)
                        {
                            // Chain move is valid, but not complete
                            currentChain.chainTiles.Add(new ChainTile(row, column));
                            view.HighlightChainedTile(row, column);
                            Trace.WriteLine($"[{row}, {column}]: Chained new tile into chain move");
                        }
                    }
                }
            }
            else
            {
                // Select new checker
                if(board[row, column].HasValue && board[row, column].Value.Player == currentPlayer)
                {
                    // Clicked tile is a valid checker to select
                    Trace.WriteLine("Selected new checker to move");
                    view.HighlightTile(row, column);
                    currentChain.chainTiles.Add(new ChainTile(row, column, board[row, column].Value.IsKing));
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
        /// <summary>
        /// This function is called, if the game ends (all checkers of one color have been eaten, or one player can't move)
        /// </summary>
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

        /// <summary>
        /// This function draw the board on the empty window's grid, using board list
        /// </summary>
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

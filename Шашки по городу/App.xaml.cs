using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Шашки_по_городу
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        const string whiteColor = "#FFCE9E";
        const string blackColor = "#D18B47";
        const string checkerWhiteColor = "#FFFFFF";
        const string checkerBlackColor = "#000000";
        const int gridWidth = 560;
        const int gridHeight = 560;
        const int tileWidth = gridWidth / Presenter.columns;
        const int tileHeight = gridHeight / Presenter.rows;

        SolidColorBrush whiteBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(whiteColor);
        SolidColorBrush blackBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(blackColor);
        SolidColorBrush checkerWhiteBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(checkerWhiteColor);
        SolidColorBrush checkerBlackBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(checkerBlackColor);

        System.Windows.Controls.Grid boardGrid = new System.Windows.Controls.Grid();

        List<Ellipse> checkersList = new List<Ellipse>();
        Ellipse clickedOnCheckerToMove = null;

        private void addCheckerToGrid(int row, int column, SolidColorBrush colorBrush)
        {
            Trace.WriteLine($"create an ellipse in ({row}, {column})");
            Ellipse ellipse = new Ellipse
            {
                Height = tileHeight - 2 * 5,
                Width = tileHeight - 2 * 5
            };
            ellipse.Fill = colorBrush;
            moveChecker(ellipse, row, column);
            boardGrid.Children.Add(ellipse);
            checkersList.Add(ellipse);
        }

        private Ellipse getCheckerByRowAndColumn(int row, int column)
        {
            for (int i = 0; i < checkersList.Count; i++)
            {
                Ellipse ellipse = checkersList[i];
                if (Grid.GetRow(ellipse) == row && Grid.GetColumn(ellipse) == column)
                {
                    return ellipse;
                }
            }
            return null;
        }

        private void removeCheckerFromGrid(int row, int column)
        {
            Ellipse checker = getCheckerByRowAndColumn(row, column);
            if (checker != null)
            {
                checkersList.Remove(checker);
                boardGrid.Children.Remove(checker);
            }
        }

        private void moveChecker(Ellipse checker, int row, int column)
        {
            Grid.SetRow(checker, row);
            Grid.SetColumn(checker, column);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickedOn = e.GetPosition(boardGrid);
            double x = clickedOn.X;
            double y = clickedOn.Y;
            int row = (int)y / tileWidth;
            int column = (int)x / tileHeight;
            Trace.WriteLine($"clicked on ({row}, {column})");
            if (clickedOnCheckerToMove == null)
            {
                Ellipse clickedOnChecker = getCheckerByRowAndColumn(row, column);
                if(clickedOnChecker != null) {
                    clickedOnCheckerToMove = clickedOnChecker;
                }
                
            }
            else {
                if (getCheckerByRowAndColumn(row, column) == null)
                {
                    moveChecker(clickedOnCheckerToMove, row, column);
                    clickedOnCheckerToMove = null;
                }
            }
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Title = "Шашки";

            boardGrid.Width = gridWidth;
            boardGrid.Height = gridHeight;
            boardGrid.HorizontalAlignment = HorizontalAlignment.Left;
            boardGrid.VerticalAlignment = VerticalAlignment.Center;
            boardGrid.MouseDown += Grid_MouseDown;

            for (int i = 0; i < Presenter.columns; i++)
            {
                ColumnDefinition coldef = new ColumnDefinition
                {
                    Width = new GridLength(tileWidth)
                };
                boardGrid.ColumnDefinitions.Add(coldef);
            }

            for (int i = 0; i < Presenter.rows; i++)
            {
                RowDefinition rowdef = new RowDefinition
                {
                    Height = new GridLength(tileHeight)
                };
                boardGrid.RowDefinitions.Add(rowdef);
            }

            Rectangle[,] boardTiles = new Rectangle[Presenter.rows, Presenter.columns];
            for (int i = 0; i < Presenter.rows; i++)
            {
                for(int j = 0; j < Presenter.columns; j++)
                {
                    boardTiles[i, j] = new Rectangle();
                    var tile = boardTiles[i, j];
                    tile.Fill = ((i + j) % 2 == 0) ? whiteBrush : blackBrush;
                    Grid.SetRow(tile, i);
                    Grid.SetColumn(tile, j);
                    
                    boardGrid.Children.Add(tile);
                }
            }
            addCheckerToGrid(3, 5, checkerBlackBrush);
            addCheckerToGrid(7, 2, checkerWhiteBrush);
            moveChecker(getCheckerByRowAndColumn(3, 5), 7, 7);
            window.Content = boardGrid;

            window.Show();
        }
    }
}

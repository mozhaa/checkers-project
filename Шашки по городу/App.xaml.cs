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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Title = "Шашки";

            System.Windows.Controls.Grid boardGrid = new System.Windows.Controls.Grid();
            boardGrid.Width = 560;
            boardGrid.Height = 560;
            boardGrid.HorizontalAlignment = HorizontalAlignment.Left;
            boardGrid.VerticalAlignment = VerticalAlignment.Center;

            const int columns = 8, rows = 8;
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinition coldef = new ColumnDefinition();
                coldef.Width = new GridLength(boardGrid.Width / columns);
                boardGrid.ColumnDefinitions.Add(coldef);
            }

            for (int i = 0; i < rows; i++)
            {
                RowDefinition rowdef = new RowDefinition();
                rowdef.Height = new GridLength(boardGrid.Height / rows);
                boardGrid.RowDefinitions.Add(rowdef);
            }

            const string whiteColor = "#FFCE9E", blackColor = "#D18B47";
            SolidColorBrush whiteBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(whiteColor);
            SolidColorBrush blackBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(blackColor);
            Rectangle[,] boardTiles = new Rectangle[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for(int j = 0; j < columns; j++)
                {
                    boardTiles[i, j] = new Rectangle();
                    boardTiles[i, j].Fill = ((i + j) % 2 == 0) ? whiteBrush : blackBrush;
                    Grid.SetRow(boardTiles[i, j], i);
                    Grid.SetColumn(boardTiles[i, j], j);
                    boardGrid.Children.Add(boardTiles[i, j]);
                    Trace.WriteLine($"tile[{i}, {j}] created");
                }
            }
            window.Content = boardGrid;
            window.Show();
        }
    }
}

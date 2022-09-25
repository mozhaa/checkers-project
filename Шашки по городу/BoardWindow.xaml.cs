using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Шашки_по_городу
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class BoardWindow : Window, IBoardView
    {
        private Presenter presenter;

        const string whiteColor = "#FFCE9E";
        const string blackColor = "#D18B47";
        const string highlightColor = "#FF0000";
        const string highlightChainedColor = "#007FFF";
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
        SolidColorBrush highlightBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(highlightColor);
        SolidColorBrush highlightChainedBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(highlightChainedColor);

        System.Windows.Controls.Grid boardGrid = new System.Windows.Controls.Grid();
        Rectangle[,] boardTiles = new Rectangle[Presenter.rows, Presenter.columns];

        List<Ellipse> checkersList = new List<Ellipse>();

        public BoardWindow()
        {
            InitializeComponent();
            Title = "Шашки";
            this.Loaded += BoardWindow_Loaded;

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

            for (int i = 0; i < Presenter.rows; i++)
            {
                for (int j = 0; j < Presenter.columns; j++)
                {
                    boardTiles[i, j] = new Rectangle();
                    var tile = boardTiles[i, j];
                    tile.Fill = ((i + j) % 2 == 0) ? whiteBrush : blackBrush;
                    Grid.SetRow(tile, i);
                    Grid.SetColumn(tile, j);

                    boardGrid.Children.Add(tile);
                    var tileNum = new TextBlock();
                    tileNum.Text = $"{i}, {j}";
                    Grid.SetRow(tileNum, i);
                    Grid.SetColumn(tileNum, j);
                    boardGrid.Children.Add(tileNum);
                }
            }
            Content = boardGrid;
        }

        private void BoardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            presenter.Start();
        }

        void IBoardView.SetPresenter(Presenter presenter)
        {
            this.presenter = presenter;
        }

        private SolidColorBrush getColorByPlayer(Player player)
        {
            switch (player)
            {
                case Player.black:
                    return checkerBlackBrush;
                case Player.white:
                    return checkerWhiteBrush;
                default:
                    return null;
            }
        }

        public void AddCheckerToGrid(int row, int column, Player player)
        {
            var colorBrush = getColorByPlayer(player);
            Trace.WriteLine($"create an ellipse in ({row}, {column})");
            Ellipse ellipse = new Ellipse
            {
                Height = tileHeight - 2 * 5,
                Width = tileHeight - 2 * 5
            };
            ellipse.Fill = colorBrush;
            SetCoordinates(row, column, ellipse);
            boardGrid.Children.Add(ellipse);
            checkersList.Add(ellipse);
        }

        private static void SetCoordinates(int row, int column, Ellipse ellipse)
        {
            Grid.SetRow(ellipse, row);
            Grid.SetColumn(ellipse, column);
        }

        private Ellipse GetCheckerByRowAndColumn(int row, int column)
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

        public void RemoveCheckerFromGrid(int row, int column)
        {
            Ellipse checker = GetCheckerByRowAndColumn(row, column);
            if (checker != null)
            {
                checkersList.Remove(checker);
                boardGrid.Children.Remove(checker);
            }
        }

        public void MoveChecker(int fromRow, int fromColumn, int toRow, int toColumn)
        {
            Ellipse checker = GetCheckerByRowAndColumn(fromRow, fromColumn);
            SetCoordinates(toRow, toColumn, checker);
        }

        public void HighlightTile(int row, int column)
        {
            var rectangle = boardTiles[row, column];
            rectangle.Stroke = highlightBrush;
            rectangle.StrokeThickness = 3;
        }

        public void HighlightChainedTile(int row, int column)
        {
            var rectangle = boardTiles[row, column];
            rectangle.Stroke = highlightChainedBrush;
            rectangle.StrokeThickness = 2;
        }

        public void DehighlightTile(int row, int column)
        {
            var rectangle = boardTiles[row, column];
            rectangle.Stroke = Brushes.Transparent;
            rectangle.StrokeThickness = 0;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickedOn = e.GetPosition(boardGrid);
            double x = clickedOn.X;
            double y = clickedOn.Y;
            int row = (int)y / tileWidth;
            int column = (int)x / tileHeight;
            Trace.WriteLine($"clicked on ({row}, {column})");
            presenter.MouseDown(row, column);
        }

    }
}

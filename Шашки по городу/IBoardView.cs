using System.Windows.Media;
using System.Windows.Shapes;

namespace Шашки_по_городу
{
    internal interface IBoardView
    {
        void AddCheckerToGrid(int row, int column, Checker checker);
        void ClearGrid();
        void DehighlightTile(int row, int column);
        void HighlightChainedTile(int row, int column);
        void HighlightTile(int row, int column);
        void MoveChecker(int fromRow, int fromColumn, int toRow, int toColumn);
        void RefreshTile(int row, int column, Checker checker);
        void RemoveCheckerFromGrid(int row, int column);
        void SetPresenter(Presenter presenter);
    }
}
using System.Windows.Media;
using System.Windows.Shapes;

namespace Шашки_по_городу
{
    internal interface IBoardView
    {
        void AddCheckerToGrid(int row, int column, Player player);
        void DehighlightTile(int row, int column);
        void HighlightTile(int row, int column);
        void MoveChecker(int fromRow, int fromColumn, int toRow, int toColumn);
        void RemoveCheckerFromGrid(int row, int column);
        void SetPresenter(Presenter presenter);
    }
}
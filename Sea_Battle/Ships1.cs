using System.Windows.Forms;

namespace SeaBattleNet
{
    public partial class Form1
    {
        bool CheckAllShipsDead(CellState[,] field)
        {
            for (int y = 0; y < BoardSize; y++)
                for (int x = 0; x < BoardSize; x++)
                    if (field[x, y] == CellState.Ship)
                        return false;

            return true;
        }

        // Определяем, уничтожен ли корабль на моём поле и возвращаем его границы
        bool IsMyShipKilledAndBounds(int x, int y,
                                     out int sx, out int sy,
                                     out int ex, out int ey)
        {
            sx = ex = x;
            sy = ey = y;

            bool IsShipOrHit(CellState c) =>
                c == CellState.Ship || c == CellState.Hit;

            // определяем ориентацию
            bool horizontal = false;
            if (x > 0 && IsShipOrHit(myField[x - 1, y]))
                horizontal = true;
            else if (x < BoardSize - 1 && IsShipOrHit(myField[x + 1, y]))
                horizontal = true;

            if (horizontal)
            {
                int cx = x;
                while (cx - 1 >= 0 && IsShipOrHit(myField[cx - 1, y]))
                    cx--;
                sx = cx;

                cx = x;
                while (cx + 1 < BoardSize && IsShipOrHit(myField[cx + 1, y]))
                    cx++;
                ex = cx;

                sy = ey = y;

                // если где-то осталась целая палуба — корабль не добит
                for (int i = sx; i <= ex; i++)
                    if (myField[i, y] == CellState.Ship)
                        return false;
            }
            else
            {
                int cy = y;
                while (cy - 1 >= 0 && IsShipOrHit(myField[x, cy - 1]))
                    cy--;
                sy = cy;

                cy = y;
                while (cy + 1 < BoardSize && IsShipOrHit(myField[x, cy + 1]))
                    cy++;
                ey = cy;

                sx = ex = x;

                for (int j = sy; j <= ey; j++)
                    if (myField[x, j] == CellState.Ship)
                        return false;
            }

            return true;
        }

        // На поле противника у нас только попадания (Hit),
        // поэтому просто берём непрерывную линию X'ов.
        void GetHitShipBounds(CellState[,] field, int x, int y,
                              out int sx, out int sy,
                              out int ex, out int ey)
        {
            sx = ex = x;
            sy = ey = y;

            bool horizontal = false;
            if (x > 0 && field[x - 1, y] == CellState.Hit)
                horizontal = true;
            else if (x < BoardSize - 1 && field[x + 1, y] == CellState.Hit)
                horizontal = true;

            if (horizontal)
            {
                int cx = x;
                while (cx - 1 >= 0 && field[cx - 1, y] == CellState.Hit)
                    cx--;
                sx = cx;

                cx = x;
                while (cx + 1 < BoardSize && field[cx + 1, y] == CellState.Hit)
                    cx++;
                ex = cx;

                sy = ey = y;
            }
            else
            {
                int cy = y;
                while (cy - 1 >= 0 && field[x, cy - 1] == CellState.Hit)
                    cy--;
                sy = cy;

                cy = y;
                while (cy + 1 < BoardSize && field[x, cy + 1] == CellState.Hit)
                    cy++;
                ey = cy;

                sx = ex = x;
            }
        }

        // Ставим точки вокруг прямоугольника корабля (sx,sy)-(ex,ey)
        void MarkAroundShip(CellState[,] field, Button[,] buttons,
                            int sx, int sy, int ex, int ey)
        {
            for (int yy = sy - 1; yy <= ey + 1; yy++)
            {
                for (int xx = sx - 1; xx <= ex + 1; xx++)
                {
                    if (xx < 0 || yy < 0 || xx >= BoardSize || yy >= BoardSize)
                        continue;

                    // пропускаем клетки самого корабля
                    if (xx >= sx && xx <= ex && yy >= sy && yy <= ey)
                        continue;

                    if (field[xx, yy] == CellState.Empty)
                    {
                        field[xx, yy] = CellState.Miss;
                        buttons[xx, yy].Text = "•";
                    }
                }
            }
        }
    }
}

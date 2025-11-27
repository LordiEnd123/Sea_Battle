using System;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattleNet
{
    public partial class Form1
    {
        // Поля и корабли

        // Создание сетки 10 на 10 из кнопок для обеих сторон
        void InitBoards()
        {
            pnlMy.Controls.Clear();
            pnlEnemy.Controls.Clear();

            pnlMy.Width = pnlEnemy.Width = BoardSize * CellSize;
            pnlMy.Height = pnlEnemy.Height = BoardSize * CellSize;

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    // Мое поле
                    var bMy = new Button();
                    bMy.Width = bMy.Height = CellSize;
                    bMy.Left = x * CellSize;
                    bMy.Top = y * CellSize;
                    bMy.Tag = new Point(x, y);
                    bMy.Enabled = false;
                    pnlMy.Controls.Add(bMy);
                    myButtons[x, y] = bMy;

                    // Поле противника
                    var bEn = new Button();
                    bEn.Width = bEn.Height = CellSize;
                    bEn.Left = x * CellSize;
                    bEn.Top = y * CellSize;
                    bEn.Tag = new Point(x, y);
                    bEn.Click += EnemyCell_Click;
                    pnlEnemy.Controls.Add(bEn);
                    enemyButtons[x, y] = bEn;
                }
            }

            ClearFields();
        }

        // Сбрасывает поля
        void ClearFields()
        {
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    myField[x, y] = CellState.Empty;
                    enemyField[x, y] = CellState.Empty;

                    myButtons[x, y].BackColor = SystemColors.Control;
                    myButtons[x, y].Text = "";
                    enemyButtons[x, y].BackColor = SystemColors.Control;
                    enemyButtons[x, y].Text = "";
                }
            }

            gameOver = false;
        }

        // Ставит корабли в случайном порядке
        void PlaceShipsRandom()
        {
            ClearFields();
            int[] ships = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            foreach (int deck in ships)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed)
                {
                    attempts++;
                    bool horizontal = rnd.Next(2) == 0;
                    int x = rnd.Next(BoardSize);
                    int y = rnd.Next(BoardSize);

                    if (!CanPlaceShip(x, y, deck, horizontal))
                        continue;

                    // Ставим корабль
                    if (horizontal)
                    {
                        for (int i = 0; i < deck; i++)
                            myField[x + i, y] = CellState.Ship;
                    }
                    else
                    {
                        for (int i = 0; i < deck; i++)
                            myField[x, y + i] = CellState.Ship;
                    }

                    placed = true;
                }
            }

            // Показать свои корабли
            for (int yy = 0; yy < BoardSize; yy++)
            {
                for (int xx = 0; xx < BoardSize; xx++)
                {
                    if (myField[xx, yy] == CellState.Ship)
                        myButtons[xx, yy].BackColor = Color.LightBlue;
                    else
                        myButtons[xx, yy].BackColor = SystemColors.Control;
                }
            }
        }

        // Проверка, можно ли поставить корабль такой длины в точке (x,y)
        bool CanPlaceShip(int x, int y, int deck, bool horizontal)
        {
            if (horizontal)
            {
                if (x + deck > BoardSize) return false;
                for (int i = 0; i < deck; i++)
                {
                    int cx = x + i;
                    int cy = y;

                    // Проверяем клетку и все клетки вокруг неё
                    for (int yy = cy - 1; yy <= cy + 1; yy++)
                    {
                        for (int xx = cx - 1; xx <= cx + 1; xx++)
                        {
                            if (xx < 0 || yy < 0 || xx >= BoardSize || yy >= BoardSize)
                                continue;

                            if (myField[xx, yy] == CellState.Ship)
                                return false;
                        }
                    }
                }
            }
            else
            {
                if (y + deck > BoardSize) return false;
                for (int i = 0; i < deck; i++)
                {
                    int cx = x;
                    int cy = y + i;

                    for (int yy = cy - 1; yy <= cy + 1; yy++)
                    {
                        for (int xx = cx - 1; xx <= cx + 1; xx++)
                        {
                            if (xx < 0 || yy < 0 || xx >= BoardSize || yy >= BoardSize)
                                continue;
                            if (myField[xx, yy] == CellState.Ship)
                                return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}

using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SeaBattleNet
{
    public partial class Form1 : Form
    {
        // чтобы не конфликтовать с Form.Size
        const int BoardSize = 10;
        const int CellSize = 30;

        Button[,] myButtons = new Button[BoardSize, BoardSize];
        Button[,] enemyButtons = new Button[BoardSize, BoardSize];

        CellState[,] myField = new CellState[BoardSize, BoardSize];
        CellState[,] enemyField = new CellState[BoardSize, BoardSize];

        // поля сети делаем допускающими null
        TcpListener? listener;
        TcpClient? client;
        StreamReader? reader;
        StreamWriter? writer;

        bool isHost = false;   // я сервер или клиент
        bool myTurn = false;   // сейчас мой ход?
        bool gameOver = false; // игра закончилась?
        bool connected = false; // есть сетевое соединение?

        Random rnd = new Random();

        enum CellState
        {
            Empty,
            Ship,
            Miss,
            Hit
        }

        public Form1()
        {
            InitializeComponent();
            InitBoards();
            lblStatus.Text = "Нажмите «Создать игру» или «Подключиться». ";

        }

        // ====== ИГРОВАЯ ЛОГИКА / ПРОТОКОЛ ======

        private void EnemyCell_Click(object? sender, EventArgs e)
        {
            // защита от странных ситуаций
            if (!connected)
            {
                MessageBox.Show("Сначала создайте игру или подключитесь.", "Морской бой",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (gameOver)
            {
                MessageBox.Show("Игра уже закончена. Нажмите «Новая игра».", "Морской бой",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!myTurn)
            {
                lblStatus.Text = "Сейчас ход соперника, ждите.";
                return;
            }

            if (writer == null)
                return;

            if (sender is not Button b || b.Tag is not Point p)
                return;

            // уже стреляли сюда
            if (enemyField[p.X, p.Y] == CellState.Miss ||
                enemyField[p.X, p.Y] == CellState.Hit)
                return;

            Send($"SHOT {p.X} {p.Y}");
            myTurn = false;
            lblStatus.Text = "Выстрел отправлен. Ждём ответа...";
        }

        void ProcessMessage(string msg)
        {
            string[] parts = msg.Split(' ');

            if (parts.Length == 0) return;

            switch (parts[0])
            {
                case "SHOT":
                    if (parts.Length >= 3 &&
                        int.TryParse(parts[1], out int sx) &&
                        int.TryParse(parts[2], out int sy))
                    {
                        HandleIncomingShot(sx, sy);
                    }
                    break;

                case "RESULT":
                    if (parts.Length >= 4 &&
                        int.TryParse(parts[2], out int rx) &&
                        int.TryParse(parts[3], out int ry))
                    {
                        string res = parts[1];
                        HandleShotResult(res, rx, ry);
                    }
                    break;

                case "NEW":
                    StartNewGameLocal();
                    break;
            }
        }

        void HandleShotResult(string res, int x, int y)
        {
            if (res == "miss")
            {
                enemyField[x, y] = CellState.Miss;
                enemyButtons[x, y].Text = "•";
            }
            else
            {
                enemyField[x, y] = CellState.Hit;
                enemyButtons[x, y].BackColor = Color.Red;
                enemyButtons[x, y].Text = "X";

                // если корабль противника уничтожен (или это был последний — win),
                // ставим точки вокруг него
                if (res == "kill" || res == "win")
                {
                    GetHitShipBounds(enemyField, x, y, out int sx, out int sy, out int ex, out int ey);
                    MarkAroundShip(enemyField, enemyButtons, sx, sy, ex, ey);
                }
            }

            if (res == "win")
            {
                gameOver = true;
                lblStatus.Text = "Вы победили!";

                var r = MessageBox.Show(
                    "Победа! Переиграть?",
                    "Морской бой",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (r == DialogResult.Yes)
                    btnNewGame_Click(this, EventArgs.Empty);
                else
                    DisconnectFromLobby();
            }

            else
            {
                // если промахнулись — ход переходит сопернику,
                // если попали или уничтожили корабль — ход остаётся у нас
                myTurn = (res == "hit" || res == "kill");
                lblStatus.Text = myTurn ? "Ваш ход. Стреляйте по правому полю."
                                        : "Ход соперника. Ждите его выстрела.";
            }
        }


        void HandleIncomingShot(int x, int y)
        {
            if (gameOver) return;

            bool hit = myField[x, y] == CellState.Ship;
            bool shipKilled = false;
            int sx = x, sy = y, ex = x, ey = y;

            if (hit)
            {
                myField[x, y] = CellState.Hit;
                myButtons[x, y].BackColor = Color.Red;
                myButtons[x, y].Text = "X";

                // проверяем, добит ли весь корабль, и получаем его границы
                shipKilled = IsMyShipKilledAndBounds(x, y, out sx, out sy, out ex, out ey);
                if (shipKilled)
                {
                    // ставим точки вокруг нашего уничтоженного корабля
                    MarkAroundShip(myField, myButtons, sx, sy, ex, ey);
                }
            }
            else
            {
                if (myField[x, y] == CellState.Empty)
                    myField[x, y] = CellState.Miss;

                myButtons[x, y].Text = "•";
            }

            bool allDead = CheckAllShipsDead(myField);

            string res;
            if (!hit)
                res = "miss";
            else if (allDead)
                res = "win";
            else if (shipKilled)
                res = "kill";   // НОВЫЙ результат: корабль уничтожен
            else
                res = "hit";

            Send($"RESULT {res} {x} {y}");

            if (res == "win")
            {
                gameOver = true;
                lblStatus.Text = "Вы проиграли :(";

                var r = MessageBox.Show(
                    "Вы проиграли. Переиграть?",
                    "Морской бой",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (r == DialogResult.Yes)
                    btnNewGame_Click(this, EventArgs.Empty);
                else
                    DisconnectFromLobby();
            }

            else
            {
                // если по нам попали — соперник ходит ещё раз,
                // если промахнулся — теперь наш ход
                myTurn = !hit;
                lblStatus.Text = myTurn ? "Ваш ход. Стреляйте по правому полю."
                                        : "Ход соперника. Ждите его выстрела.";
            }
        }

        void DisconnectFromLobby()
        {
            connected = false;
            myTurn = false;
            gameOver = true;

            try
            {
                reader?.Dispose();
                writer?.Dispose();
                client?.Close();
                listener?.Stop();
            }
            catch
            {
            }

            // если у тебя есть кнопка "Отключиться" – выключаем её
            // (если такой кнопки нет, просто удали эту строку)
            // btnDisconnect.Enabled = false;

            btnHost.Enabled = true;
            btnConnect.Enabled = true;

            lblStatus.Text = "Отключено. Нажмите «Создать игру» или «Подключиться».";
        }
    }
}

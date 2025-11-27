using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SeaBattleNet
{
    public partial class Form1 : Form
    {
        const int BoardSize = 10;
        const int CellSize = 30;

        Button[,] myButtons = new Button[BoardSize, BoardSize];
        Button[,] enemyButtons = new Button[BoardSize, BoardSize];

        CellState[,] myField = new CellState[BoardSize, BoardSize];
        CellState[,] enemyField = new CellState[BoardSize, BoardSize];

        // Поля сети 
        TcpListener? listener; // Создание игры 
        TcpClient? client;     // Подключение к другому игроку
        StreamReader? reader;  // Чтение сообщений от второго игрока
        StreamWriter? writer;  // Отправка сообщений второму игроку

        bool isHost = false;    // Я являюсь сервером игры или клиентом
        bool myTurn = false;    // Мой ли ход сейчас
        bool gameOver = false;  // Игра закончилась
        bool connected = false; // Есть ли соединение
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

        // Игровая логика

        // Реакция на клик игрока по полю врага
        private void EnemyCell_Click(object? sender, EventArgs e)
        {
            if (!connected)
            {
                MessageBox.Show("Сначала создайте игру или подключитесь.", "Морской бой",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!myTurn)
            {
                lblStatus.Text = "Сейчас ход соперника, ждите.";
                return;
            }

            if (writer == null || sender is not Button b || b.Tag is not Point p)
                return;

            // Уже стреляли сюда
            if (enemyField[p.X, p.Y] == CellState.Miss ||
                enemyField[p.X, p.Y] == CellState.Hit)
                return;

            Send($"SHOT {p.X} {p.Y}");
            myTurn = false;
            lblStatus.Text = "Выстрел отправлен. Ждём ответа...";
        }

        // Обработчик входящих сетевых сообщений
        void ProcessMessage(string msg)
        {
            string[] parts = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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

        // Обработка ответа от врага на наш выстрел
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

                // Если корабль противника уничтожен, то ставим точки вокруг него
                if (res == "kill" || res == "win")
                {
                    GetHitShipBounds(enemyField, x, y, out int sx, out int sy, out int ex, out int ey);
                    MarkAroundShip(enemyField, enemyButtons, sx, sy, ex, ey);
                }
            }

            if (res == "win")
            {
                gameOver = true;
                lblStatus.Text = "Вы победили! Игра окончена.";
                MessageBox.Show("Вы победили!", "Морской бой", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisconnectFromLobby();
            }
            else
            {
                // Если промахнулись, то ход переходит сопернику, если попали или уничтожили корабль, то ход остаётся у нас
                myTurn = (res == "hit" || res == "kill");
                lblStatus.Text = myTurn ? "Ваш ход. Стреляйте по правому полю." : "Ход соперника. Ждите его выстрела.";
            }
        }

        // Обрабатываем выстрел врага по нам
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

                // Проверяем, добит ли весь корабль, получаем его границы
                shipKilled = IsMyShipKilledAndBounds(x, y, out sx, out sy, out ex, out ey);
                if (shipKilled)
                {
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
                res = "kill";
            else
                res = "hit";

            Send($"RESULT {res} {x} {y}");

            if (res == "win")
            {
                gameOver = true;
                lblStatus.Text = "Вы проиграли. Игра окончена.";
                MessageBox.Show("Вы проиграли.", "Морской бой", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisconnectFromLobby();
            }
            else
            {
                // Если по нам попали, то соперник ходит ещё раз, если промахнулся, то теперь наш ход
                myTurn = !hit;
                lblStatus.Text = myTurn ? "Ваш ход. Стреляйте по правому полю." : "Ход соперника. Ждите его выстрела.";
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
            catch { }
            btnHost.Enabled = true;
            btnConnect.Enabled = true;
            lblStatus.Text = "Отключено. Нажмите «Создать игру» или «Подключиться».";
        }
    }
}
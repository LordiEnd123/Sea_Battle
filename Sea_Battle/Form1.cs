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
        bool manualPlacement = false; // Сейчас расставляем корабли руками или нет
        Random rnd = new Random();

        readonly int[] fleet = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };    // Набор кораблей для ручной расстановки
        int currentShipIndex = 0;      // Какой по счёту корабль сейчас ставим
        bool manualHorizontal = true;  // Ориентация текущего корабля (true — горизонтальная, false — вертикальная)

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
                MessageBox.Show("Сначала создайте игру или подключитесь.", "Морской бой", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (enemyField[p.X, p.Y] == CellState.Miss || enemyField[p.X, p.Y] == CellState.Hit)
                return;

            Send($"SHOT {p.X} {p.Y}");
            myTurn = false;
            lblStatus.Text = "Выстрел отправлен. Ждём ответа...";
        }

        // Обработчик входящих сообщений
        void ProcessMessage(string msg)
        {
            string[] parts = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            switch (parts[0])
            {
                case "SHOT":
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int sx) && int.TryParse(parts[2], out int sy))
                    {
                        HandleIncomingShot(sx, sy);
                    }
                    break;

                case "RESULT":
                    if (parts.Length >= 4 && int.TryParse(parts[2], out int rx) && int.TryParse(parts[3], out int ry))
                    {
                        string res = parts[1];
                        HandleShotResult(res, rx, ry);
                    }
                    break;
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
        }

        // Ручная установка кораей
        private void MyBoardCell_MouseDown(object? sender, MouseEventArgs e)
        {
            if (!manualPlacement)
                return;

            if (sender is not Button b || b.Tag is not Point p)
                return;

            int x = p.X;
            int y = p.Y;

            if (e.Button == MouseButtons.Right)
            {
                manualHorizontal = !manualHorizontal;
                lblStatus.Text = manualHorizontal ? "Ориентация: горизонтальная" : "Ориентация: вертикальная";
                return;
            }

            // Если все корабли уже поставлены, то ничего не делаем
            if (currentShipIndex >= fleet.Length)
            {
                lblStatus.Text = "Все корабли уже расставлены.";
                manualPlacement = false;
                return;
            }
            int deck = fleet[currentShipIndex];

            // Проверяем, можно ли тут поставить корабль нужной длины
            if (!CanPlaceShip(x, y, deck, manualHorizontal))
            {
                MessageBox.Show("Сюда корабль поставить нельзя.", "Морской бой",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ставим корабль в массив
            if (manualHorizontal)
            {
                for (int i = 0; i < deck; i++)
                {
                    myField[x + i, y] = CellState.Ship;
                    myButtons[x + i, y].BackColor = Color.LightBlue;
                }
            }
            else
            {
                for (int i = 0; i < deck; i++)
                {
                    myField[x, y + i] = CellState.Ship;
                    myButtons[x, y + i].BackColor = Color.LightBlue;
                }
            }

            // Переходим к следующему кораблю
            currentShipIndex++;
            if (currentShipIndex >= fleet.Length)
            {
                manualPlacement = false;
                lblStatus.Text = "Все корабли расставлены. Можно начинать игру.";
            }
            else
            {
                int nextSize = fleet[currentShipIndex];
                string orient = manualHorizontal ? "горизонтально" : "вертикально";
                lblStatus.Text = $"Ручная расстановка.";
            }
        }

        // Автоматическая расстановка
        private void btnAutoPlace_Click(object? sender, EventArgs e)
        {
            if (connected && !gameOver)
            {
                MessageBox.Show("Авторасстановку можно делать только до начала игры.", "Морской бой", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            manualPlacement = false;
            PlaceShipsRandom();
            lblStatus.Text = "Корабли расставлены автоматически.";
        }

        // Ручная расстановка
        private void btnManualPlace_Click(object? sender, EventArgs e)
        {
            if (connected && !gameOver)
            {
                MessageBox.Show("Ручную расстановку можно делать только до начала игры.", "Морской бой", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            manualPlacement = true;
            ClearFields();
            currentShipIndex = 0;
            manualHorizontal = true;
            lblStatus.Text = "Ручная расстановка. Ставим корабль на 4 клетки (горизонтально).";
        }

        private void chkHorizontal_CheckedChanged(object? sender, EventArgs e)
        {
            manualPlacement = true; // Мы в режиме ручной расстановки
            manualHorizontal = chkHorizontal.Checked;
        }

        // Проверяем есть ли хотя бы один корабль на поле
        private bool HasAnyShips()
        {
            for (int y = 0; y < BoardSize; y++)
                for (int x = 0; x < BoardSize; x++)
                    if (myField[x, y] == CellState.Ship)
                        return true;
            return false;
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
            lblStatus.Text = "Нажмите «Создать игру» или «Подключиться».";
        }
    }
}
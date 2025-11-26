using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeaBattleNet
{
    public partial class Form1
    {
        // Сеть и кнопки

        void SetupStreams(TcpClient c)
        {
            var ns = c.GetStream();
            reader = new StreamReader(ns);
            writer = new StreamWriter(ns) { AutoFlush = true };
        }

        async void StartReceiveLoop()
        {
            await Task.Run(async () =>
            {
                try
                {
                    while (!gameOver && connected)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null) break;

                        Invoke(new Action(() => ProcessMessage(line)));
                    }
                }
                catch { }
            });
        }

        void Send(string msg)
        {
            try
            {
                writer?.WriteLine(msg);
            }
            catch { }
        }

        private async void btnHost_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                lblStatus.Text = "Ожидание подключения клиента...";
                client = await listener.AcceptTcpClientAsync();
                SetupStreams(client);

                isHost = true;
                myTurn = true;
                connected = true;

                btnHost.Enabled = false;
                btnConnect.Enabled = false;

                lblStatus.Text = "Подключен как ХОСТ. Ваш ход. Кликайте по правому полю.";
                PlaceShipsRandom();
                StartReceiveLoop();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка: " + ex.Message;
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                client = new TcpClient();
                await client.ConnectAsync(txtIp.Text, port);
                SetupStreams(client);

                isHost = false;
                myTurn = false;
                connected = true;

                btnHost.Enabled = false;
                btnConnect.Enabled = false;

                lblStatus.Text = "Подключен как КЛИЕНТ. Сейчас ход соперника.";
                PlaceShipsRandom();
                StartReceiveLoop();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка: " + ex.Message;
            }
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            // Послать команду напарнику и локально перезапустить
            if (connected && writer != null)
                Send("NEW");

            StartNewGameLocal();
        }

        void StartNewGameLocal()
        {
            ClearFields();
            PlaceShipsRandom();
            gameOver = false;

            if (connected)
            {
                myTurn = isHost;
                lblStatus.Text = myTurn
                    ? "Новая игра. Ваш ход. Стреляйте по правому полю." : "Новая игра. Ход соперника.";
            }
            else
            {
                lblStatus.Text = "Для сетевой нажмите «Создать игру» или «Подключиться».";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            connected = false;
            try
            {
                reader?.Dispose();
                writer?.Dispose();
                client?.Close();
                listener?.Stop();
            }
            catch { }
        }
    }
}

using System.Windows.Forms;

namespace SeaBattleNet
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlMy;
        private Panel pnlEnemy;
        private TextBox txtIp;
        private TextBox txtPort;
        private Button btnHost;
        private Button btnConnect;
        private Button btnNewGame;
        private Label lblStatus;
        private Label labelIp;
        private Label labelPort;
        private GroupBox groupConnection;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlMy = new Panel();
            pnlEnemy = new Panel();
            txtIp = new TextBox();
            txtPort = new TextBox();
            btnHost = new Button();
            btnConnect = new Button();
            btnNewGame = new Button();
            lblStatus = new Label();
            labelIp = new Label();
            labelPort = new Label();
            groupConnection = new GroupBox();
            groupConnection.SuspendLayout();
            SuspendLayout();

            pnlMy.BorderStyle = BorderStyle.FixedSingle;
            pnlMy.Location = new Point(3, 12);
            pnlMy.Name = "pnlMy";
            pnlMy.Size = new Size(310, 310);
            pnlMy.TabIndex = 0;

            pnlEnemy.BorderStyle = BorderStyle.FixedSingle;
            pnlEnemy.Location = new Point(320, 12);
            pnlEnemy.Name = "pnlEnemy";
            pnlEnemy.Size = new Size(310, 310);
            pnlEnemy.TabIndex = 1;

            txtIp.Location = new Point(45, 24);
            txtIp.Name = "txtIp";
            txtIp.Size = new Size(160, 27);
            txtIp.TabIndex = 0;
            txtIp.Text = "127.0.0.1";

            txtPort.Location = new Point(290, 24);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(60, 27);
            txtPort.TabIndex = 1;
            txtPort.Text = "5000";
            
            btnHost.Location = new Point(19, 58);
            btnHost.Name = "btnHost";
            btnHost.Size = new Size(120, 40);
            btnHost.TabIndex = 2;
            btnHost.Text = "Создать игру";
            btnHost.UseVisualStyleBackColor = true;
            btnHost.Click += btnHost_Click;

            btnConnect.Location = new Point(145, 58);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(120, 40);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "Подключиться";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;

            btnNewGame.Location = new Point(370, 17);
            btnNewGame.Name = "btnNewGame";
            btnNewGame.Size = new Size(120, 41);
            btnNewGame.TabIndex = 4;
            btnNewGame.Text = "Новая игра";
            btnNewGame.UseVisualStyleBackColor = true;
            btnNewGame.Click += btnNewGame_Click;

            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(271, 68);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(124, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Статус: нет ИГРЫ";

            labelIp.AutoSize = true;
            labelIp.Location = new Point(16, 27);
            labelIp.Name = "labelIp";
            labelIp.Size = new Size(24, 20);
            labelIp.TabIndex = 4;
            labelIp.Text = "IP:";

            labelPort.AutoSize = true;
            labelPort.Location = new Point(221, 27);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(47, 20);
            labelPort.TabIndex = 5;
            labelPort.Text = "Порт:";

            groupConnection.Controls.Add(btnNewGame);
            groupConnection.Controls.Add(lblStatus);
            groupConnection.Controls.Add(btnHost);
            groupConnection.Controls.Add(btnConnect);
            groupConnection.Controls.Add(labelIp);
            groupConnection.Controls.Add(labelPort);
            groupConnection.Controls.Add(txtIp);
            groupConnection.Controls.Add(txtPort);
            groupConnection.Location = new Point(12, 325);
            groupConnection.Name = "groupConnection";
            groupConnection.Size = new Size(618, 104);
            groupConnection.TabIndex = 2;
            groupConnection.TabStop = false;


            ClientSize = new Size(642, 441);
            Controls.Add(groupConnection);
            Controls.Add(pnlEnemy);
            Controls.Add(pnlMy);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Морской бой (сеть)";
            FormClosing += Form1_FormClosing;
            groupConnection.ResumeLayout(false);
            groupConnection.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
    }
}

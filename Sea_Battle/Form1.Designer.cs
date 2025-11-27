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
        private Label lblStatus;
        private Label labelIp;
        private Label labelPort;
        private GroupBox groupConnection;
        private Button btnAutoPlace;
        private Button btnManualPlace;
        private CheckBox chkHorizontal;


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
            lblStatus = new Label();
            labelIp = new Label();
            labelPort = new Label();
            groupConnection = new GroupBox();
            chkHorizontal = new CheckBox();
            btnAutoPlace = new Button();
            btnManualPlace = new Button();
            groupConnection.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMy
            // 
            pnlMy.BorderStyle = BorderStyle.FixedSingle;
            pnlMy.Location = new Point(3, 12);
            pnlMy.Name = "pnlMy";
            pnlMy.Size = new Size(310, 310);
            pnlMy.TabIndex = 0;
            // 
            // pnlEnemy
            // 
            pnlEnemy.BorderStyle = BorderStyle.FixedSingle;
            pnlEnemy.Location = new Point(320, 12);
            pnlEnemy.Name = "pnlEnemy";
            pnlEnemy.Size = new Size(310, 310);
            pnlEnemy.TabIndex = 1;
            // 
            // txtIp
            // 
            txtIp.Location = new Point(45, 24);
            txtIp.Name = "txtIp";
            txtIp.Size = new Size(160, 27);
            txtIp.TabIndex = 0;
            txtIp.Text = "10.233.54.81";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(290, 24);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(60, 27);
            txtPort.TabIndex = 1;
            txtPort.Text = "5000";
            // 
            // btnHost
            // 
            btnHost.Location = new Point(19, 58);
            btnHost.Name = "btnHost";
            btnHost.Size = new Size(120, 40);
            btnHost.TabIndex = 2;
            btnHost.Text = "Создать игру";
            btnHost.UseVisualStyleBackColor = true;
            btnHost.Click += btnHost_Click;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(145, 58);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(120, 40);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "Подключиться";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(271, 68);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(124, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Статус: нет ИГРЫ";
            // 
            // labelIp
            // 
            labelIp.AutoSize = true;
            labelIp.Location = new Point(16, 27);
            labelIp.Name = "labelIp";
            labelIp.Size = new Size(24, 20);
            labelIp.TabIndex = 4;
            labelIp.Text = "IP:";
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(221, 27);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(47, 20);
            labelPort.TabIndex = 5;
            labelPort.Text = "Порт:";
            // 
            // groupConnection
            // 
            groupConnection.Controls.Add(chkHorizontal);
            groupConnection.Controls.Add(lblStatus);
            groupConnection.Controls.Add(btnHost);
            groupConnection.Controls.Add(btnConnect);
            groupConnection.Controls.Add(labelIp);
            groupConnection.Controls.Add(labelPort);
            groupConnection.Controls.Add(txtIp);
            groupConnection.Controls.Add(txtPort);
            groupConnection.Controls.Add(btnAutoPlace);
            groupConnection.Controls.Add(btnManualPlace);
            groupConnection.Location = new Point(12, 325);
            groupConnection.Name = "groupConnection";
            groupConnection.Size = new Size(618, 104);
            groupConnection.TabIndex = 2;
            groupConnection.TabStop = false;
            // 
            // chkHorizontal
            // 
            chkHorizontal.AutoSize = true;
            chkHorizontal.Checked = true;
            chkHorizontal.CheckState = CheckState.Checked;
            chkHorizontal.Location = new Point(19, 104);
            chkHorizontal.Name = "chkHorizontal";
            chkHorizontal.Size = new Size(138, 24);
            chkHorizontal.TabIndex = 5;
            chkHorizontal.Text = "Горизонтально";
            chkHorizontal.CheckedChanged += chkHorizontal_CheckedChanged;
            // 
            // btnAutoPlace
            // 
            btnAutoPlace.Location = new Point(356, 24);
            btnAutoPlace.Name = "btnAutoPlace";
            btnAutoPlace.Size = new Size(120, 27);
            btnAutoPlace.TabIndex = 4;
            btnAutoPlace.Text = "Авто";
            btnAutoPlace.UseVisualStyleBackColor = true;
            btnAutoPlace.Click += btnAutoPlace_Click;
            // 
            // btnManualPlace
            // 
            btnManualPlace.Location = new Point(482, 24);
            btnManualPlace.Name = "btnManualPlace";
            btnManualPlace.Size = new Size(120, 27);
            btnManualPlace.TabIndex = 5;
            btnManualPlace.Text = "Ручная";
            btnManualPlace.UseVisualStyleBackColor = true;
            btnManualPlace.Click += btnManualPlace_Click;
            // 
            // Form1
            // 
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

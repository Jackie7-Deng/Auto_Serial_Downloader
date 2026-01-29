namespace Auto_Serial_Downloader
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            txtVid = new TextBox();
            txtPid = new TextBox();
            txtSn = new TextBox();
            txtCyusbPath = new TextBox();
            btnBrowse = new Button();
            btnDownload = new Button();
            openFileDialog1 = new OpenFileDialog();
            btnConnect = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 103);
            label1.Name = "label1";
            label1.Size = new Size(33, 20);
            label1.TabIndex = 0;
            label1.Text = "VID";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(142, 104);
            label2.Name = "label2";
            label2.Size = new Size(32, 20);
            label2.TabIndex = 1;
            label2.Text = "PID";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(284, 103);
            label3.Name = "label3";
            label3.Size = new Size(25, 20);
            label3.TabIndex = 2;
            label3.Text = "Sn";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(408, 103);
            label4.Name = "label4";
            label4.Size = new Size(76, 20);
            label4.TabIndex = 3;
            label4.Text = "CyusbPath";
            // 
            // txtVid
            // 
            txtVid.Location = new Point(51, 100);
            txtVid.Name = "txtVid";
            txtVid.Size = new Size(85, 27);
            txtVid.TabIndex = 4;
            // 
            // txtPid
            // 
            txtPid.Location = new Point(180, 101);
            txtPid.Name = "txtPid";
            txtPid.Size = new Size(98, 27);
            txtPid.TabIndex = 5;
            // 
            // txtSn
            // 
            txtSn.Location = new Point(315, 100);
            txtSn.Name = "txtSn";
            txtSn.Size = new Size(87, 27);
            txtSn.TabIndex = 6;
            // 
            // txtCyusbPath
            // 
            txtCyusbPath.Location = new Point(490, 100);
            txtCyusbPath.Name = "txtCyusbPath";
            txtCyusbPath.Size = new Size(178, 27);
            txtCyusbPath.TabIndex = 7;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(674, 100);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(61, 29);
            btnBrowse.TabIndex = 8;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(437, 239);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(148, 59);
            btnDownload.TabIndex = 9;
            btnDownload.Text = "Download";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(161, 239);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(148, 59);
            btnConnect.TabIndex = 10;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnConnect);
            Controls.Add(btnDownload);
            Controls.Add(btnBrowse);
            Controls.Add(txtCyusbPath);
            Controls.Add(txtSn);
            Controls.Add(txtPid);
            Controls.Add(txtVid);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "USB-Serial Downloader V1.0";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox txtVid;
        private TextBox txtPid;
        private TextBox txtSn;
        private TextBox txtCyusbPath;
        private Button btnBrowse;
        private Button btnDownload;
        private OpenFileDialog openFileDialog1;
        private Button btnConnect;
        private System.Windows.Forms.Timer timer1;
    }
}

namespace MyGoBangClient
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label_player1 = new System.Windows.Forms.Label();
            this.label_player2 = new System.Windows.Forms.Label();
            this.textBox1_INFO = new System.Windows.Forms.TextBox();
            this.textBox1_MSG = new System.Windows.Forms.TextBox();
            this.btn_send = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_player1
            // 
            this.label_player1.AutoSize = true;
            this.label_player1.Location = new System.Drawing.Point(464, 59);
            this.label_player1.Name = "label_player1";
            this.label_player1.Size = new System.Drawing.Size(47, 12);
            this.label_player1.TabIndex = 0;
            this.label_player1.Text = "player1";
            // 
            // label_player2
            // 
            this.label_player2.AutoSize = true;
            this.label_player2.Location = new System.Drawing.Point(464, 88);
            this.label_player2.Name = "label_player2";
            this.label_player2.Size = new System.Drawing.Size(47, 12);
            this.label_player2.TabIndex = 1;
            this.label_player2.Text = "player2";
            // 
            // textBox1_INFO
            // 
            this.textBox1_INFO.Location = new System.Drawing.Point(452, 164);
            this.textBox1_INFO.Multiline = true;
            this.textBox1_INFO.Name = "textBox1_INFO";
            this.textBox1_INFO.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1_INFO.Size = new System.Drawing.Size(147, 239);
            this.textBox1_INFO.TabIndex = 2;
            // 
            // textBox1_MSG
            // 
            this.textBox1_MSG.Location = new System.Drawing.Point(455, 412);
            this.textBox1_MSG.Name = "textBox1_MSG";
            this.textBox1_MSG.Size = new System.Drawing.Size(98, 21);
            this.textBox1_MSG.TabIndex = 3;
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(558, 412);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(41, 20);
            this.btn_send.TabIndex = 4;
            this.btn_send.Text = "发送";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.btn_send_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(459, 148);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "聊天信息:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(460, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "玩家列表:";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 464);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_send);
            this.Controls.Add(this.textBox1_MSG);
            this.Controls.Add(this.textBox1_INFO);
            this.Controls.Add(this.label_player2);
            this.Controls.Add(this.label_player1);
            this.Name = "Form2";
            this.Text = "游戏窗口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form2_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1_MSG;
        private System.Windows.Forms.Button btn_send;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox1_INFO;
        public System.Windows.Forms.Label label_player2;
        public System.Windows.Forms.Label label_player1;
    }
}
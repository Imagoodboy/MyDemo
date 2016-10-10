using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyGoBangClient
{
    public partial class Form2 : Form
    {
        public static CreatChessBoard Chess;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Chess = new CreatChessBoard(this.CreateGraphics(), this.BackColor);
            ControlChess C = new ControlChess();
            this.Click += new EventHandler(C.MouseClick);
        }

       // private void button1_Click(object sender, EventArgs e)
       // {
            //Chess.DrawChessBoard();
            //textBox1_INFO.AppendText("[系统]:当前房间号为" + Form1.mRoom + "号\n");
      //  }

        private void btn_send_Click(object sender, EventArgs e)
        {
            string msg = textBox1_MSG.Text;
            Form1.MyClient.SendPublicMessage("PRIVATE_MSG|" + Form1.mName + "|" +Form1.vsID+ "|"+msg);//发送公共消息
            textBox1_INFO.AppendText("[" + Form1.mName + "]:" + msg+"\n");
            textBox1_MSG.Text= "";
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e) //当一方关闭游戏界面时，游戏结束
        {
            if (Form1.flag_Exit == 0)
            {
                Form1.ifGame = false;
                Form1.MyClient.SendPublicMessage("GAME_EXIT|" + Form1.mName + "|" + Form1.mID + "|" + Form1.vsID);
            }
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
             string msg  = "";
             string msg2 = "";
             if (Form1.Player == 1) {msg = "(先手)";msg2 ="(后手)";}
             else if(Form1.Player == 2){msg = "(后手)";msg2 ="(先手)";}
             label_player1.Text = "我:" + Form1.mName + msg;
             label_player2.Text = "对手：" + Form1.vsName + msg2;
        }
    }
}

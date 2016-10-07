using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPDLL;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace MyGoBangServer
{
    public partial class Form1 : Form
    {
        private List<Socket> mClientNum = new List<Socket>();//客户端列表
        public Form1()
        {
            InitializeComponent();
        }
        private void btn_open_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("等待客户端连接\n");
            TCPServer MyServer = new TCPServer("192.168.1.99", 5566, 10);//127.0.0.1
            MyServer.ReturnMsg += new ReturnSomeMsg2(SetMsg);
            MyServer.Start();
        }
        private void SetMsg(int flag, string msg) //接受消息输出
        {
            this.SetText(msg+"\n");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TCPServer.Close();
            Thread.Sleep(100);
        }
        //跨线程调用控件
        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.AppendText(text);
                this.textBox1.Refresh();
            }
        }
    }
}
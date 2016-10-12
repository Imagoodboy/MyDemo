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
using System.Threading;
namespace MyGoBangClient
{
    public partial class Form1 : Form
    {
        public class Node
        {
            public string state;//游戏状态
            public string name;//名字
            public int id;//用户ID
        };
        public static TCPClient MyClient = new TCPClient("127.0.0.1", 5566);//127.0.0.1
        public static int CHESS_SIZE = 15;
        public static int CHESS_ST = 20;
        public static int CHESS_LENGTH = 440;
        public static int CHESS_SPACE = 30;
        public static int[,] vis= new int[15, 15];//标记棋盘落子 棋盘大小15*15
        private ListViewItem xy;//获取listview中的项
        public static bool ifGame = false;//记录是否正在游戏
        public static string vsName;//记录对手的名字
        public static string mName;//记录我的名字
        public static int mID;//记录我的ID
        public static int vsID;//记录对手的ID
        public static int Player;//标记先手后手
        public static int Player_Now;//目前下棋方
        public static int mRoom;//我的游戏房间号
        public static int flag_Exit;//记录对方是否关闭
        private bool ifConnect = false;
        Form2 f;//窗体2
        //private List<Node> mClient;//客户端列表
        public Form1()
        {
            InitializeComponent();
        }
        //输出信息状态
        private void SetMsg(int flag,string msg)
        {
            if (flag == 1)
            {
                if (msg.Contains("连接服务器成功"))
                {
                    ifConnect = true;
                }
                this.SetText(msg, 1);//基本消息输出
            }
            else if (flag == 4) //当有新加入的客户端,刷新listview,输出信息
            {
                this.SetText(msg + "\n",1);
                this.SetText("", 3);

            }
            else if (flag == 5) //获取昵称
            {
                mName = MyClient.GetName();
                mID = MyClient.GetID();
                this.SetText("我的昵称:"+mName, 2);
            }
            else if (flag == 6)//新加入的客户端获取客户端列表，并输出在listview
            {
                this.SetText("", 3);
            }
            else if (flag == 10) //选择是否接受挑战
            {
                vsID = int.Parse(msg);
                vsName = FindName(vsID); //寻找对战方昵称
                this.SetText("[系统]:" + vsName + "向你发起挑战!\n", 1);
                ShowMessage("玩家"+vsName+"向你发起挑战,是否接受?");//弹出对话选择框
            }
            else if (flag == 11)//对手接受挑战，输出信息
            {
                this.SetText("[系统]:玩家" + vsName + "接受了你的挑战!\n",1);
                Init();
                Player = 1;//挑战方先手
                Player_Now = 1; //目前下棋方
                ifGame = true;
                this.Invoke((MethodInvoker)delegate //通过委托，回到主线程，创建form2
                {
                    flag_Exit = 0;
                    f = new Form2();
                    f.Show();
                    //Form2.Chess.DrawChessBoard();
                });
            }
            else if (flag == 12)//对手拒绝挑战，输出信息
            {
                this.SetText("[系统]:玩家" + vsName + "拒绝了你的挑战!\n", 1);
            }
            else if (flag == 13)//落子坐标获取
            {
                string[] tokens = msg.Split(new Char[] { '|' });
                this.Invoke((MethodInvoker)delegate //通过委托，回到主线程,画棋子
                {
                    if (Player_Now == 1) //先手执白
                    {
                        vis[int.Parse(tokens[0]), int.Parse(tokens[1])] = 1;
                        f.Invalidate();
                       // f.invalidate();
                         //Pen p2 = new Pen(Color.White, 2);
                        // SolidBrush b1 = new SolidBrush(Color.White);
                         //CreatChessBoard.gp.FillEllipse(b1, int.Parse(tokens[0]) - 10, int.Parse(tokens[1]) - 10, 20, 20);//画棋子
                    }
                    else if (Player_Now == 2)//后手执黑
                    {
                        vis[int.Parse(tokens[0]), int.Parse(tokens[1])] = 2;
                        f.Invalidate();
                       // Pen p2 = new Pen(Color.Black, 2);
                       // SolidBrush b1 = new SolidBrush(Color.Black);
                       // CreatChessBoard.gp.FillEllipse(b1, int.Parse(tokens[0]) - 10, int.Parse(tokens[1]) - 10, 20, 20);//画棋子
                    }                  
                });
                if (Player_Now == 1) Player_Now = 2;
                else if (Player_Now == 2) Player_Now = 1;
            }
            else if (flag == 15)//私聊
            {
                this.Invoke((MethodInvoker)delegate //通过委托，回到主线程,私聊
                {
                    f.textBox1_INFO.AppendText(msg+"\n");
                });
            }
            else if (flag == 14)//游戏结束
            {
                this.Invoke((MethodInvoker)delegate
                {
                    string[] tokens = msg.Split(new Char[] { '|' });
                    string _msg = "";
                    if (int.Parse(tokens[0]) == mID)
                    {
                        f.textBox1_INFO.AppendText("[系统]:" + mName + "获得了胜利!\n");
                        msg = mName;
                    }
                    else if (int.Parse(tokens[0]) == vsID)
                    {
                        f.textBox1_INFO.AppendText("[系统]:" + vsName + "获得了胜利!\n");
                        msg = vsName;
                    }
                    Init();//清空棋盘标记信息，重新开始游戏
                    DialogResult dr;
                    dr = MessageBox.Show(f, msg + "获得了胜利!", "系统信息", MessageBoxButtons.OK,
                           MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    if (dr == DialogResult.OK)
                    {
                        Player_Now = 1;//永远挑战方先手
                    }
                    f.Invalidate();//重新绘制
                });     
            }
            else if (flag == 16)//分配房间数
            {
                string[] tokens = msg.Split(new Char[] { '|' });
                this.SetText("[系统]:玩家" + tokens[0] + "和" + tokens[1] + "开始游戏\n", 1);
                if (mID == int.Parse(tokens[2]) || mID == int.Parse(tokens[3]))
                {
                    mRoom = int.Parse(tokens[4]);
                }
            }
            else if (flag == 17)//对方关闭游戏界面，对战结束
            {
                ifGame = false;
                this.Invoke((MethodInvoker)delegate 
                {
                    flag_Exit = 1;
                    DialogResult dr;
                    dr = MessageBox.Show(f, msg + "退出游戏,双方游戏结束!", "系统信息", MessageBoxButtons.OK);
                    if (dr == DialogResult.OK)
                    {
                       f.Close();
                    }
                });
            }
        }
        //发送消息
        private void btn_msg_Click(object sender, EventArgs e)
        {
            string msg = textBox2.Text;
            MyClient.SendPublicMessage("PUBLIC_MSG|"+mName+"|"+msg);//发送公共消息
            textBox2.Text = "";
        }
        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (!ifConnect)
            {
                MyClient.ReturnMsg += new ReturnSomeMsg(SetMsg);//为事件绑定方法
                MyClient.Start();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ifConnect == true)
            {
                MyClient.SendPublicMessage("EXIT|");//发送消息,退出游戏
            }
            MyClient.Close();
            Thread.Sleep(100);
        }
        //跨线程调用控件
        delegate void SetTextCallback(string text,int flag);
        private void SetText(string text,int flag)
        {
            if (flag == 1)
            {
                if (this.textBox1.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text,flag });
                }
                else
                {
                    this.textBox1.AppendText(text);
                    this.textBox1.Refresh();
                }
            }
            else if(flag == 2)
            {
                if (this.label2.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text,flag });
                }
                else
                {
                    this.label2.Text = text;
                    this.label2.Refresh();
                }
            }
            else if (flag == 3)  //listview更新全部信息
            {
                if (this.listView1.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text, flag });
                }
                else
                {
                    this.listView1.Items.Clear(); 
                    for (int i = 0; i < TCPClient.mClient.Count; i++)
                    {
                        ListViewItem list = new ListViewItem((TCPClient.mClient[i].id).ToString());
                        list.SubItems.Add(TCPClient.mClient[i].name); list.SubItems.Add(TCPClient.mClient[i].state);
                        listView1.Items.Add(list);
                    }
                }
            }
        }

        //修改名字
        private void btn_change_Click(object sender, EventArgs e) 
        {
            MyClient.SendPublicMessage("NAME_CHANGE|" + mID + "|" + textBox3.Text);
            textBox3.Text = "";
        }

        //listview鼠标点击事件
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                   xy = listView1.GetItemAt(e.X, e.Y);
                  if (xy != null)//仅当点击项不空时弹出菜单
                  {
                      Point point = this.PointToClient(listView1.PointToScreen(new Point(e.X, e.Y)));
                      this.contextMenuStrip1.Show(this, point);
                  }
            }
        }

        private void 挑战ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //textBox2.Text = a;
            if (ifGame == true)
            {
                textBox1.AppendText("[系统]:你正在游戏中,不能挑战他人!\n");
            }
            else if (xy.SubItems[2].Text == "正在游戏")
            {
                textBox1.AppendText("[系统]:该玩家正在游戏,不能挑战!\n");
            }
            else if (int.Parse(xy.SubItems[0].Text) == mID)
            {
                textBox1.AppendText("[系统]:不能挑战自己!\n");
            }
            else
            {
                vsID = int.Parse(xy.SubItems[0].Text);
                MyClient.SendPublicMessage("CHALLENGE|" + mID + "|" + vsID);
                vsName = FindName(vsID);
                textBox1.AppendText("[系统]:你挑战了" + vsName + "!\n");
            }
        }
        private string FindName(int vsid) //寻找对手昵称
        {
            string _NAME="";
            for (int i = 0; i < TCPClient.mClient.Count; i++)
            {
                if (TCPClient.mClient[i].id == vsid)
                {
                    _NAME = TCPClient.mClient[i].name;
                    break;
                }
            }
            return _NAME;
        }
        //弹出对话框
        private void ShowMessage(string msg)
        {
            this.Invoke(new MessageBoxShow(MessageBoxShow_F), new object[] { msg });
        }
        delegate void MessageBoxShow(string msg);
        private void MessageBoxShow_F(string msg)
        {
            DialogResult dr;
            dr=MessageBox.Show(msg, "系统信息", MessageBoxButtons.YesNo,
                   MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            if (dr == DialogResult.Yes)
            {
                MyClient.SendPublicMessage("CHALLENGE_YES|" + mID + "|" + vsID);
                textBox1.AppendText("[系统]:你接受了"+vsName+"的挑战!\n");
                Init();
                ifGame = true;
                Player = 2;//被挑战方后手
                Player_Now = 1; //目前下棋方
                flag_Exit = 0;
                f = new Form2();
                f.Show();
                //Form2.Chess.DrawChessBoard();
                //textBox1.AppendText("接受挑战");
            }
            else if (dr == DialogResult.No)
            {
                MyClient.SendPublicMessage("CHALLENGE_NO|" + mID + "|" + vsID);
                textBox1.AppendText("[系统]:你拒绝了" + vsName + "的挑战!\n");
                //textBox1.AppendText("拒绝挑战");
            }
        }
        private void Init()
        {
            for (int i = 0; i < CHESS_SIZE; i++)
            {
                for (int j = 0; j < CHESS_SIZE; j++)
                {
                    vis[i, j] = 0;
                }
            }
        }
    }
}

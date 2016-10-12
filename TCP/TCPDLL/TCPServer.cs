using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace TCPDLL
{
    public delegate void ReturnSomeMsg2(int _flag, string msg);  //委托
    public class TCPServer
    {
        public class Node
        {
            public Socket mClient;//Socket
            public string state;//游戏状态
            public string name;//名字
            public int id;//用户ID
        };
        private string rMsg;//返回信息(利用委托事件传递)
        private int ID;//标记ID使用序号
        private string ip; //ip
        private int port; //端口
        private int maxClientCount; //最大客户端数量,最大监听数目
        private byte[] data = new byte[1024];//接收数据的缓存
        private IPEndPoint ipEnd;//ip终端
        private Socket mServerSocket; //服务端Socket
        private List<Node> mClientSockets;//客户端列表
        private Node T;//暂存Node信息
        private Socket mClientSocket;  //当前客户端Socket
        static bool flag = true;
        private int[,,] vis = new int[100,CHESS_SIZE, CHESS_SIZE];//标记棋盘落子 棋盘大小15*15; 100代表游戏房间总数
        private int roomNum=0;//当前房间数
        private const int CHESS_SIZE = 15;
        private const int CHESS_ST = 20;
        private const int CHESS_LENGTH = 440;
        private const int CHESS_SPACE = 30;
        private int player_now;//目前下棋方
        private bool Winer;//是否胜利
        /// <summary>
        /// //获取客户端列表
        /// </summary>
        /// <returns></returns>
        public List<Node> GetClient()
        {
            return mClientSockets;
        }
        /// <summary>
        /// 获取当前客户端数量
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            return mClientSockets.Count;
        }
        public TCPServer() { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="count">监听的最大数目</param>
        public TCPServer(string ip, int port, int count)
        {
            this.ip = ip;
            this.port = port;
            this.maxClientCount = count;
            this.mClientSockets = new List<Node>(); ;
            this.ID = 0;
            //定义侦听端口,初始化IP终端
            ipEnd = new IPEndPoint(IPAddress.Parse(ip), port);
            //初始化服务端Socket
            mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定主机
            mServerSocket.Bind(ipEnd);
            //监听数目,开始侦听
            mServerSocket.Listen(count);
        }
        /// <summary>
        /// Start 开启线程
        /// </summary>
        public void Start()
        {
            //创建服务端线程，实现客户端连接请求的循环监听 
            Thread mServerThread = new Thread(ListenClientConnect);
            mServerThread.IsBackground = true; //后台线程
            //服务端线程开启
            mServerThread.Start();
        }
        public event ReturnSomeMsg2 ReturnMsg;  //事件
        /// <summary>
        /// 监听客户端
        /// </summary>
        private void ListenClientConnect()
        {
            while (flag)
            {
                //获取连接到服务端的客户端
                mClientSocket = mServerSocket.Accept();
                T = new Node();
                //设置新加入的客户端属性
                T.mClient = mClientSocket;
                T.state = "空闲";
                int CC = ++ID;//客户端ID
                T.name = "游客" +(CC.ToString());
                T.id = ID;
                //将获取到的客户端添加到客户端列表
                mClientSockets.Add(T);
                //向连接客户端发送消息，传值获取昵称,id
                SendPrivateMessage(-1, T.id, "NAME_ID|" + T.name+"|"+T.id+"|CLIENT_INFO|" + GetClientInfo());//-1代表服务端
                //向所有客户端发送消息
                SendMessageAll("JOIN|"+T.name+"|"+T.id+"|"+T.state+"|已加入游戏");
                //SendMessageAll("JOIN|" + T.name + "|已成功连接到服务器"); //加上这条爆炸
                //SendPrivateMessage(-1, T.id, "CLIENT_INFO|" + GetClientInfo());
                //服务器输出到textbox
                rMsg = T.name + "(" + mClientSocket.RemoteEndPoint + ")" + "已成功连接到服务器";
                ReturnMsg(1,rMsg);
                //创建客户端消息线程，实现客户端消息的循环监听
                Thread mReceiveThread = new Thread(ReceiveClient);
                mReceiveThread.IsBackground = true;//后台线程
                mReceiveThread.Start(mClientSocket);
            }
        }
        /// <summary>
        /// 接收客户端信息
        /// </summary>
        /// <param name="soc"></param>
        private void ReceiveClient(object soc)
        {
            Socket _timelyClientSocket = (Socket)soc;
            while (flag)
            {
                try
                {
                    //
                    int mlength; //消息长度
                    byte[] lengthBytes = new byte[4];
                    _timelyClientSocket.Receive(lengthBytes, 0, 4, SocketFlags.None);//先接收消息头（长度）byte[]
                    mlength = BitConverter.ToInt32(lengthBytes, 0);//将byte转为int，为消息长度
                    //
                    int recv;//接收数据长度变量
                    recv = _timelyClientSocket.Receive(data,0,mlength,SocketFlags.None);
                    //获取客户端消息 
                    string _clientMsg = Encoding.UTF8.GetString(data, 0, recv);
                    string[] tokens = _clientMsg.Split(new Char[] { '|' });
                    //向所有客户端发送接收到的该消息
                    if (tokens[0]=="EXIT") //处理用户退出
                    {
                        throw new Exception("该用户已经退出");
                    }
                    if (tokens[0] == "NAME_CHANGE")//用户更改昵称
                    {
                        string _NAME;
                        for (int i = 0; i < mClientSockets.Count; i++)
                        {
                            if (mClientSockets[i].id == int.Parse(tokens[1]))
                            {
                                _NAME = mClientSockets[i].name;
                                mClientSockets[i].name = tokens[2];
                                SendMessageAll("NAME_CHANGE|" + tokens[1] + "|" + tokens[2]);
                                ReturnMsg(1, "用户:" + _NAME + "将昵称修改为 " + tokens[2]);
                                break;
                            }
                        }
                    }
                    if (tokens[0] == "PUBLIC_MSG")
                    {
                        SendMessageAll("PUBLIC_MSG|" + tokens[1] + "|" + tokens[2]);
                    }
                    if (tokens[0] == "CHALLENGE")
                    {
                        SendPrivateMessage(-1, int.Parse(tokens[2]), "CHALLENGE|" + tokens[1]);
                    }
                    if (tokens[0] == "CHALLENGE_YES")
                    {
                        Init(roomNum);//初始化棋盘
                        string _NAME1 = "";
                        string _NAME2 = "";
                        _NAME1 = FindName(int.Parse(tokens[1]));
                        _NAME2 = FindName(int.Parse(tokens[2]));
                        for (int i = 0; i < mClientSockets.Count; i++)//更改对战玩家游戏状态
                        {
                            if (mClientSockets[i].id == int.Parse(tokens[1]) || mClientSockets[i].id == int.Parse(tokens[2]))
                            {
                                mClientSockets[i].state = "正在游戏";
                            }
                        }
                        SendPrivateMessage(-1, int.Parse(tokens[2]), "CHALLENGE_YES|" + tokens[1]);
                        SendMessageAll("CHALLENGE_PLAYER|"+_NAME1+"|"+_NAME2+"|"+tokens[1]+"|"+tokens[2]+"|"+roomNum);//向所有客户端发送消息
                        roomNum++;//房间数加一
                    }
                    if (tokens[0] == "CHALLENGE_NO")
                    {
                        SendPrivateMessage(-1, int.Parse(tokens[2]), "CHALLENGE_NO|" + tokens[1]);
                    }
                    if (tokens[0] == "GAME_RUN")
                    {
                        player_now = int.Parse(tokens[3]);
                        if (vis[int.Parse(tokens[8]),int.Parse(tokens[4]), int.Parse(tokens[5])] == 0) //判断棋子是否可下
                        {
                            vis[int.Parse(tokens[8]), int.Parse(tokens[4]), int.Parse(tokens[5])] = int.Parse(tokens[3]);
                            SendPrivateMessage(-1, int.Parse(tokens[1]), "GAME_RUN|" + tokens[4] + "|" + tokens[5]);
                            SendPrivateMessage(-1, int.Parse(tokens[2]), "GAME_RUN|" + tokens[4] + "|" + tokens[5]);
                            Winer = JudgeWiner(int.Parse(tokens[8]), int.Parse(tokens[4]), int.Parse(tokens[5]));
                            if (Winer == true) //如果获胜
                            {
                                Init(int.Parse(tokens[8]));
                                //胜利方，tokens[1] 失败方，tokens[2]
                                SendPrivateMessage(-1,int.Parse(tokens[1]),"GAME_OVER|" + tokens[1] + "|" + tokens[2]);
                                SendPrivateMessage(-1, int.Parse(tokens[2]), "GAME_OVER|" + tokens[1] + "|" + tokens[2]);
                            }
                        }
                    }
                    if (tokens[0] == "PRIVATE_MSG")
                    {
                        SendPrivateMessage(-1, int.Parse(tokens[2]), "PRIVATE_MSG|" +tokens[1]+"|"+tokens[3]);
                    }
                    if (tokens[0] == "GAME_EXIT")
                    {
                        for (int i = 0; i < mClientSockets.Count; i++)
                        {
                            if (mClientSockets[i].id == int.Parse(tokens[2]) || mClientSockets[i].id == int.Parse(tokens[3]))
                            {
                                mClientSockets[i].state = "空闲";
                            }
                        }
                        SendMessageAll("GAME_EXIT|" + tokens[1] + "|" + tokens[2] + "|" + tokens[3]);//tokens[1]对手名字，token[2]对手ID，token[3]我的ID
                    }
                }
                catch (Exception e)
                {
                    string NAME="";
                    int _eID = 0;
                    //从客户端列表中移除该客户端  
                    for (int i = 0; i < mClientSockets.Count; i++)
                    {
                        if (mClientSockets[i].mClient == _timelyClientSocket)
                        {
                            rMsg = mClientSockets[i].name + "(" + _timelyClientSocket.RemoteEndPoint + ")" + "已断开服务器";
                            ReturnMsg(1,rMsg);
                            NAME =  mClientSockets[i].name;
                            _eID = mClientSockets[i].id;
                            mClientSockets.Remove(mClientSockets[i]);
                            break;
                        }
                    }
                    //向其它客户端告知该客户端下线  
                    SendMessageAll("EXIT|"+NAME+"|"+_eID+"|已经退出游戏");
                    //断开连接
                    _timelyClientSocket.Shutdown(SocketShutdown.Both);
                    _timelyClientSocket.Close();
                    break;
                }
            }
        }
        /// <summary>
        /// 向所有客户端发送信息
        /// </summary>
        /// <param name="msg">发送的内容</param>
        private void SendMessageAll(string msg)
        {

            byte[] utf8 = Encoding.UTF8.GetBytes(msg);
            //确保消息非空以及客户端列表非空
            if (msg == string.Empty || mClientSockets.Count == 0) return;
            for (int i = 0; i < mClientSockets.Count; i++)
            {
                mClientSockets[i].mClient.Send(BitConverter.GetBytes(utf8.Length));// 先发内容的长度
                mClientSockets[i].mClient.Send(Encoding.UTF8.GetBytes(msg));//发送消息
            }

        }

        /// <summary>
        /// 向指定客户端发送消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="receiver">接受者</param>
        /// <param name="msg">消息内容</param>
        private void SendPrivateMessage(int sender_id, int receiver_id, string msg)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(msg);//获取msg的byte[]
            //确保消息非空以及客户端列表非空
            if (msg == string.Empty || mClientSockets.Count == 0) return;
            for (int i = 0; i < mClientSockets.Count; i++)
            {
                if (mClientSockets[i].id == receiver_id)
                {
                    mClientSockets[i].mClient.Send(BitConverter.GetBytes(utf8.Length));// 先发内容（byte[]类型）的长度，int转byte 4个字节
                    mClientSockets[i].mClient.Send(Encoding.UTF8.GetBytes(msg));
                    break;
                }
            }
        }
        private string GetClientInfo()
        {
            string _MSG = "";
            for (int i = 0; i < mClientSockets.Count; i++)
            {
                _MSG += mClientSockets[i].name;
                _MSG += "|";
                _MSG += mClientSockets[i].id;
                _MSG += "|";
                _MSG += mClientSockets[i].state;
                _MSG += "|";
            }
            _MSG.Trim(new char[] { '|' });//去除所有前导|和尾部的|
            return _MSG;
        }
        /// <summary>
        /// 销毁线程
        /// </summary>
        public static void Close()
        {
            flag = false;
        }
        private string FindName(int vsid) //寻找对手昵称
        {
            string _NAME = "";
            for (int i = 0; i < mClientSockets.Count; i++)
            {
                if (mClientSockets[i].id == vsid)
                {
                    _NAME = mClientSockets[i].name;
                    break;
                }
            }
            return _NAME;
        }
        public void Init(int room) //清空棋盘信息
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    vis[room,i, j] = 0;
                }
            }
        }
        public bool JudgeWiner(int room,int x, int y) //评判是否获胜
        {
            int judge_x;
            int judge_y;
            int chessNum = 0;
            //检查沿y轴方向是否获胜
            judge_x = x;
            judge_y = (y - 4 >= 0) ? y - 4 : 0;
            for (int i = 0; i < 9; i++)
            {
                if (judge_y + i >= CHESS_SIZE)
                {
                    break;
                }
                if (vis[room,judge_x, judge_y + i] == player_now)
                {
                    chessNum++;
                    if (chessNum == 5)
                    {
                        return true;
                    }
                }
                else
                {
                    chessNum = 0;
                }
            }
            //检查沿x轴方向是否获胜
            chessNum = 0;
            judge_x = (x - 4 >= 0) ? x - 4 : 0;
            judge_y = y;
            for (int i = 0; i < 9; i++)
            {
                if (judge_x + i >= CHESS_SIZE)
                {
                    break;
                }
                if (vis[room,judge_x + i, judge_y] == player_now)
                {
                    chessNum++;
                    if (chessNum == 5)
                    {
                        return true;
                    }
                }
                else
                {
                    chessNum = 0;
                }
            }
            //检查沿主对角线方向是否获胜
            chessNum = 0;
            int flag1 = 0;
            int flag2 = 0;
            flag1 = (x - 4 >= 0) ? 0 : 1;
            flag2 = (y - 4 >= 0) ? 0 : 1;
            if (flag1 == 0 && flag2 == 0)
            {
                judge_x = x - 4;
                judge_y = y - 4;
            }
            else
            {
                judge_x = x - Math.Min(x, y);
                judge_y = y - Math.Min(x, y);
            }
            for (int i = 0; i < 9; i++)
            {
                if (judge_x + i >= CHESS_SIZE || judge_y + i >= CHESS_SIZE)
                {
                    break;
                }
                if (vis[room,judge_x + i, judge_y + i] == player_now)
                {
                    chessNum++;
                    if (chessNum == 5)
                    {
                        return true;
                    }
                }
                else
                {
                    chessNum = 0;
                }
            }
            //检查沿副对角线方向是否获胜
            chessNum = 0;
            flag1 = 0;
            flag2 = 0;
            flag1 = (x + 4 >= CHESS_SIZE) ? 1 : 0;
            flag2 = (y - 4 >= 0) ? 0 : 1;
            if (flag1 == 0 && flag2 == 0)
            {
                judge_x = x + 4;
                judge_y = y - 4;
            }
            else
            {
                judge_x = x + Math.Min(CHESS_SIZE - x - 1, y);
                judge_y = y - Math.Min(CHESS_SIZE - x - 1, y);
            }
            for (int i = 0; i < 9; i++)
            {
                if (judge_x - i < 0 || judge_y + i >= CHESS_SIZE)
                {
                    break;
                }
                if (vis[room,judge_x - i, judge_y + i] == player_now)
                {
                    chessNum++;
                    if (chessNum == 5)
                    {
                        return true;
                    }
                }
                else
                {
                    chessNum = 0;
                }
            }
            return false;
        }
    }
}
/*
       /// <summary>
       /// 向指定客户发送消息
       /// </summary>
       /// <param name="ip">客户ip地址</param>
       /// <param name="port">客户端口号</param>
       /// <param name="msg">要发送的信息</param>
       private void SendMessageOne(string ip, int port, string msg)
       {
           //构造IP终端地址
           IPEndPoint _IPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
           for (int i = 0; i < mClientSockets.Count; i++)
           {
               if (_IPEndPoint == (IPEndPoint)mClientSockets[i].RemoteEndPoint)
               {
                   mClientSockets[i].Send(Encoding.UTF8.GetBytes(msg));//发送消息
               }
           }
       }*/
/*
        /// <summary>
        /// 设置客户端信息
        /// </summary>
        /// <param name="k">原本名字</param>
        /// <param name="name">修改名字</param>
        public void SetClientNum(string k,string name)
        {
            for (int i = 0; i < mClientSockets.Count; i++)
            {
                if (mClientSockets[i].name == k)
                {
                    mClientSockets[i].name = name;
                    break;
                }
            }
        }*/
/*
/// <summary>
/// 发送游戏信息
/// </summary>
/// <param name="receiver">接受者名字</param>
/// <param name="msg">信息内容</param>
private void SendGameMessage(bool flag,string receiver, string msg)
{
    //确保消息非空以及客户端列表非空
    if (msg == string.Empty || mClientSockets.Count == 0) return;
    msg = msg.Insert(0, "GameMsg&FLAG=" + flag);
    for (int i = 0; i < mClientSockets.Count;i++ )
    {
        if(mClientSockets[i].name==receiver)
        {
            mClientSockets[i].mClient.Send(Encoding.UTF8.GetBytes(msg));
            break;
        }
    }
}*/
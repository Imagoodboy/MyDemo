using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;//正则匹配
namespace TCPDLL
{
    public delegate void ReturnSomeMsg(int _flag,string msg);  //委托
    public class TCPClient
    {
        public class Node
        {
            public string state;//游戏状态
            public string name;//名字
            public int id;//用户ID
        };
        private string peopleName="暂无"; //新连接用户名称
        private int peopleID=0;//新连接用户ID
        private byte[] data = new byte[1024];//接受数据的缓存
        private IPEndPoint ipEnd;//IP终端
        private static Socket mClientSocket;//客户端Socket
        public static List<Node> mClient;//客户端列表,记录所有用户
        private Node T;//暂存Node信息
        private bool isConnected = false; //是否连接服务器标志
        static bool flag = true;
        /// <summary>
        /// 获取用户名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return peopleName;
        }
        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return peopleID;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public TCPClient(string ip, int port)
        {
            //初始化IP终端
            ipEnd = new IPEndPoint(IPAddress.Parse(ip), port);
            mClient = new List<Node>();
            //初始化客户端Socket
            // mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
        }
        /// <summary>
        /// Start 开启线程
        /// </summary>
        public void Start()
        {
            //创建一个线程以不断连接服务器  
            Thread mConnectThread = new Thread(ConnectToServer);
            //开启线程  
            mConnectThread.Start();
            //创建一个线程以监听数据接收  
            Thread mReceiveThread = new Thread(ReceiveMessage);
            //开启线程  
            mReceiveThread.Start();
        }
        public event ReturnSomeMsg ReturnMsg;  //事件
        private void ConnectToServer()
        {
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //当没有连接到服务器时，开始连接
            while (!isConnected)
            {
                try
                {
                    //开始连接
                    mClientSocket.Connect(ipEnd);
                    isConnected = true;
                    break;
                }
                catch (Exception e)
                {
                    ReturnMsg(1, "[系统]:暂时无法连接到服务器\n");//"[系统]:暂时无法连接到服务器
                    //this.SetText("因为一个错误的发生，暂时无法连接到服务器，错误信息为:" + e.Message + "\n");
                    isConnected = false;
                }
                //等待5秒钟后尝试再次连接  
                Thread.Sleep(5000);
                ReturnMsg(1, "[系统]:正在尝试重新连接...\n");
                //this.SetText("正在尝试重新连接...\n");
            }
            ReturnMsg(1, "[系统]:连接服务器成功，现在可以和玩家进行会话了\n");
            //this.SetText("连接服务器成功，现在可以和服务器进行会话了\n");
        }
        /// <summary>
        /// 接受来自服务端的数据
        /// </summary>
        private void ReceiveMessage()
        {
            while (flag)
            {
                if (!isConnected)
                {
                    continue;
                }
                try
                {
                    if (!flag)//如果客户端被关闭，即终止线程
                    {
                        break;
                    }
                    //
                    int mlength; //消息长度
                    byte[] lengthBytes =new byte[4];
                    mClientSocket.Receive(lengthBytes, 0, 4, SocketFlags.None);//先接收消息头（长度）byte[]
                    mlength = BitConverter.ToInt32(lengthBytes, 0);//将byte转为int，为消息长度
                    //
                    int recv;//接收数据长度变量
                    recv = mClientSocket.Receive(data, 0,mlength, SocketFlags.None);//略更改
                    //获取服务器消息   
                    string _serverMsg = Encoding.UTF8.GetString(data, 0, recv);
                    string[] tokens = _serverMsg.Split(new Char[] { '|' });
                   
                    if (tokens[0] == "JOIN")//新用户成功连接服务器
                    {
                        T = new Node();
                        T.name = tokens[1];
                        T.id = int.Parse(tokens[2]);
                        T.state = tokens[3];
                        if (T.id != peopleID)
                        {
                            mClient.Add(T);
                            //ReturnMsg(4, tokens[1] + tokens[4]); //如果该客户端是旧客户端，那么需要加入mClient
                        }
                        ReturnMsg(4, "[系统]:"+tokens[1] + tokens[4]);//输出信息，刷新listview
                    }
                    if (tokens[0] == "NAME_ID") //获取初始化NAME，ID,获取客户端列表
                    {
                        peopleName = tokens[1];
                        peopleID = int.Parse(tokens[2]);
                        for (int i = 4; i < tokens.Length - 1; i++) //获取客户端列表
                        {
                            T = new Node();
                            T.name = tokens[i++];
                            T.id = int.Parse(tokens[i++]);
                            T.state = tokens[i];
                            mClient.Add(T);
                        }
                        ReturnMsg(5, "");//更新昵称
                        ReturnMsg(6, "");//委托，让项目获取客户端列表，跟新listview
                    }
                    if (tokens[0] == "EXIT")//当有用户退出
                    {
                        for (int i = 0; i < mClient.Count; i++)
                        {
                            if (mClient[i].id == int.Parse(tokens[2]))
                            {
                                mClient.Remove(mClient[i]);//找到该用户并在列表中删除
                            }
                        }
                        ReturnMsg(1, "[系统]:"+tokens[1]+tokens[3]+"\n");//退出游戏
                        ReturnMsg(6, "");//更新listview
                    }
                    if (tokens[0] == "NAME_CHANGE")
                    {
                        string _NAME ="";
                        for (int i = 0; i < mClient.Count; i++)
                        {
                            if (mClient[i].id == int.Parse(tokens[1]))
                            {
                                _NAME = mClient[i].name;
                                mClient[i].name = tokens[2];
                                break;
                            }
                        }
                        if (peopleID == int.Parse(tokens[1]))
                        {
                            peopleName = tokens[2];
                        }
                        ReturnMsg(5, "");//更新昵称
                        ReturnMsg(6, "");//更新listview
                        ReturnMsg(1, "[系统]:用户" + _NAME + "将名字改为" + tokens[2]+"\n");
                    }
                    if (tokens[0] == "PUBLIC_MSG")
                    {
                        ReturnMsg(1, "[" + tokens[1] + "]:" + tokens[2]+"\n");
                    }
                    if (tokens[0] == "CHALLENGE")
                    {
                        ReturnMsg(10, tokens[1]);//返回给客户端，让其选择是否参与挑战,tokens[1]挑战方
                    }
                    if (tokens[0] == "CHALLENGE_YES")
                    {
                        ReturnMsg(11, tokens[1]);//同意挑战
                    }
                    if (tokens[0] == "CHALLENGE_NO")
                    {
                        ReturnMsg(12, tokens[1]);//拒绝挑战
                    }
                    if (tokens[0] == "CHALLENGE_PLAYER") //所有客户端获取玩家挑战消息
                    {
                        for (int i = 0; i < mClient.Count; i++)//所有客户端更新对战玩家状态
                        {
                            if (mClient[i].id == int.Parse(tokens[3]) || mClient[i].id == int.Parse(tokens[4]))
                            {
                                mClient[i].state = "正在游戏";
                            }
                        }
                        ReturnMsg(6, "");//更新listview
                        ReturnMsg(16,tokens[1]+"|"+tokens[2]+"|"+tokens[3]+"|"+tokens[4]+"|"+tokens[5]);//token[5]为房间号
                    }
                    if (tokens[0] == "GAME_RUN") //游戏进行中，落子
                    {
                        ReturnMsg(13, tokens[1] + "|" + tokens[2]);//返回落子坐标
                    }
                    if (tokens[0] == "GAME_OVER")//游戏结束
                    {
                        ReturnMsg(14, tokens[1] + "|" + tokens[2]);
                    }
                    if (tokens[0] == "PRIVATE_MSG")
                    {
                        ReturnMsg(15, "[" + tokens[1] + "]:" + tokens[2]);
                    }
                    if (tokens[0] == "GAME_EXIT")
                    {
                        for (int i = 0; i < mClient.Count; i++)
                        {
                            if (mClient[i].id == int.Parse(tokens[2]) || mClient[i].id == int.Parse(tokens[3]))
                            {
                                mClient[i].state = "空闲";
                            }
                        }
                        ReturnMsg(6, "");//更新listview
                        if (peopleID == int.Parse(tokens[3]))
                        {
                            ReturnMsg(17, tokens[1]);
                        }
                    }
                }
                catch (Exception e)
                {
                    //停止消息接收  
                    flag = false;
                    //断开服务器  
                    mClientSocket.Shutdown(SocketShutdown.Both);
                    //关闭套接字
                    mClientSocket.Close();
                    //重新尝试连接服务器  
                    isConnected = false;
                    ConnectToServer();
                }
            }
        }
        /// <summary>
        /// 发送公共信息
        /// </summary>
        /// <param name="msg">消息内容</param>
        public void SendPublicMessage(string msg)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(msg);//获取msg的byte[]
            if (msg == string.Empty || mClientSocket == null) return;
            mClientSocket.Send(BitConverter.GetBytes(utf8.Length));// 先发内容（byte[]类型）的长度，int转byte 4个字节
            mClientSocket.Send(Encoding.UTF8.GetBytes(msg));
        }
        /// <summary>
        /// 销毁线程
        /// </summary>
        public void Close()
        {
            flag = false;
        }
    }
}
/*
                   if (tokens[0] == "CLIENT_INFO")//获取客户端列表
                   {
                       T = new Node();
                       for (int i = 1; i < tokens.Length-1; i++)
                       {
                           T.name = tokens[i++];
                           T.id = int.Parse(tokens[i++]);
                           T.state = tokens[i];
                           mClient.Add(T);
                       }
                       ReturnMsg(6, "");//委托，让项目获取客户端列表
                   }*/
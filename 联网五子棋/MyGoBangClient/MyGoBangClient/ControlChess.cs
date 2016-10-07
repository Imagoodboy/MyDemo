using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using TCPDLL;
namespace MyGoBangClient
{
     public class ControlChess
    {
         public void MouseClick(object sender, EventArgs e1)
         {
             MouseEventArgs e = (MouseEventArgs)e1;
             int _x = e.X;
             int _y = e.Y;
             //落子X坐标
             int xs = (int)(Math.Abs(_x - CreatChessBoard.CHESS_ST) / (1.0 * CreatChessBoard.CHESS_SPACE) + 0.5) * (CreatChessBoard.CHESS_SPACE) + CreatChessBoard.CHESS_ST;
             //落子y坐标
             int xf = (int)(Math.Abs(_y - CreatChessBoard.CHESS_ST) / (1.0 * CreatChessBoard.CHESS_SPACE) + 0.5) * (CreatChessBoard.CHESS_SPACE) + CreatChessBoard.CHESS_ST;
             if (Form1.Player_Now == Form1.Player)//玩家的回合
             {
                 if (e.Button == MouseButtons.Left)
                 {
                     if (Math.Abs(_x - xs) <= 10 && Math.Abs(_y - xf) <= 10)
                     {
                         int num_x = (xs - CreatChessBoard.CHESS_ST) / CreatChessBoard.CHESS_SPACE; //落子在棋盘中的X位置
                         int num_y = (xf - CreatChessBoard.CHESS_ST) / CreatChessBoard.CHESS_SPACE; //落子在棋盘中的Y位置
                         Form1.MyClient.SendPublicMessage("GAME_RUN|" + Form1.mID+"|"+Form1.vsID + "|" +Form1.Player_Now+"|"+ num_x + "|" + num_y+"|"+xs+"|"+xf + "|" + Form1.mRoom);
                     }
                 }
             }
         }
    }
}

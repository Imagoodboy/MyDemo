using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace MyGoBangClient
{
    public class CreatChessBoard
    {
        public static Graphics gp;
        public static Color back;
        public const int CHESS_SIZE = 15;
        public const int CHESS_ST = 20;
        public const int CHESS_LENGTH = 440;
        public const int CHESS_SPACE = 30;
        public static Bitmap bmp = new Bitmap(450, 450);//在内存中建立一块“虚拟画布”
        public static Graphics bufferGraphics = Graphics.FromImage(bmp);//获取这块内存画布的Graphics引用
        //构造方法,用来传参
        public CreatChessBoard(Graphics _gp, Color _back)
        {
            gp = _gp;
            back = _back;
        }
        public CreatChessBoard() { }
        //画一个棋盘
        public void DrawChessBoard() 
        {
            bufferGraphics.Clear(back);
            //gp.Clear(back);
            Pen p = new Pen(Color.Black, 2);
            for (int i = 0; i < CHESS_SIZE; i++)
            {
                bufferGraphics.DrawLine(p, CHESS_ST, CHESS_ST + i * CHESS_SPACE, CHESS_LENGTH, CHESS_ST + i * CHESS_SPACE);
                bufferGraphics.DrawLine(p, CHESS_ST + i * CHESS_SPACE, CHESS_ST, CHESS_ST + i * CHESS_SPACE, CHESS_LENGTH);
            }
        }

    }
}

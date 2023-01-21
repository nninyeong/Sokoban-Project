using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokoban
{
    /// <summary>
    /// 콘솔 환경에 그려주는 걸 도와줄 클래스
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    internal class Renderer
    {
        public void Render(int x, int y, string symbol)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(symbol);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetsMakeModules
{
    static class CollisionHelper
    {
        // 두 물체가 충돌했는지 판별합니다.
        public static bool IsCollided(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 && y1 == y2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void OnCollision(Action action)
        {
            action();
        }
    }
}

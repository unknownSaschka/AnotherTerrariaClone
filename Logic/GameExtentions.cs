using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Logic
{
    public static class GameExtentions
    {
        /// <summary>
        /// Gibt true zurück, falls sich der Punkt innerhalb der Welt befindet
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="worldSize"></param>
        /// <returns></returns>
        public static bool CheckIfInBound(int x, int y, System.Numerics.Vector2 worldSize)
        {
            if (x < 0 || x >= worldSize.X || y < 0 || y >= worldSize.Y) return false;
            else return true;
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}

using OpenTK;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Logic
{
    public static class GameExtentions
    {
        public struct ViewItemPositions
        {
            public OpenTK.Vector2 Position;
            public OpenTK.Vector2 Size;
            public int InventoryX;
            public int InventoryY;

            public ViewItemPositions(OpenTK.Vector2 position, OpenTK.Vector2 size, int invX, int invY)
            {
                Position = position;
                Size = size;
                InventoryX = invX;
                InventoryY = invY;
            }
        }

        public struct ViewButtonPositions
        {
            public OpenTK.Vector2 Position;
            public OpenTK.Vector2 Size;
            public string ButtonInput;
            public ButtonType ButtonType;
            public int Slot;

            public ViewButtonPositions(Vector2 postition, Vector2 size, string text, ButtonType buttonType, int saveSlot)
            {
                Position = postition;
                Size = size;
                ButtonInput = text;
                ButtonType = buttonType;
                Slot = saveSlot;
            }
        }

        public enum ButtonType { World, Player, CloseGame, Back, ToWorldList, None }

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

        public static bool CheckIfWithin(OpenTK.Vector2 point, OpenTK.Vector2 targetPosition, OpenTK.Vector2 targetSize, bool centered)
        {
            OpenTK.Vector2 min = new OpenTK.Vector2(targetPosition.X - targetSize.X / 2, targetPosition.Y - targetSize.Y / 2);
            OpenTK.Vector2 max = new OpenTK.Vector2(targetPosition.X + targetSize.X / 2, targetPosition.Y + targetSize.Y / 2);

            return CheckIfWithin(point, min, max);
        }

        public static bool CheckIfWithin(OpenTK.Vector2 point, Box2D box)
        {
            return CheckIfWithin(point, box.Min, box.Max);
        }

        public static bool CheckIfWithin(OpenTK.Vector2 point, OpenTK.Vector2 min, OpenTK.Vector2 max)
        {
            if (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static System.Numerics.Vector2 ConvertVector(OpenTK.Vector2 vector2)
        {
            return new System.Numerics.Vector2(vector2.X, vector2.Y);
        }

        public static OpenTK.Vector2 ConvertVector(System.Numerics.Vector2 vector2)
        {
            return new OpenTK.Vector2(vector2.X, vector2.Y);
        }
    }

    public class Box2D
    {
        public OpenTK.Vector2 CentrePosition;
        public OpenTK.Vector2 Size;

        public OpenTK.Vector2 Min;
        public OpenTK.Vector2 Max;

        public Box2D(OpenTK.Vector2 min, OpenTK.Vector2 max)
        {
            Min = min;
            Max = max;
            Size = new OpenTK.Vector2(max.X - min.X, max.Y - min.Y);
            CentrePosition = new OpenTK.Vector2(min.X + Size.X, min.Y + Size.Y);
        }

        public Box2D(OpenTK.Vector2 position, OpenTK.Vector2 size, bool centered)
        {
            if (centered)
            {
                Min = new OpenTK.Vector2(position.X - size.X / 2, position.Y - size.Y / 2);
                Max = new OpenTK.Vector2(position.X + size.X / 2, position.Y + size.Y / 2);
                CentrePosition = position;
                Size = size;
            }
            else
            {
                Min = new OpenTK.Vector2(position.X, position.Y);
                Max = new OpenTK.Vector2(position.X + size.X, position.Y + size.Y);
                CentrePosition = new OpenTK.Vector2(position.X + size.X / 2, position.Y + size.Y / 2);
                Size = size;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Logic
{
    public static class WorldLoader
    {
        public static ushort[,] LoadWorld( int width, int height)
        {
            using (FileStream fs = File.Open("world.dat", FileMode.Open))
            {
                ushort[,] world = new ushort[width, height];
                byte[] bytes = new byte[fs.Length];
                int i = 0;

                while (true)
                {
                    int n = fs.Read(bytes, 0, 2);
                    if (n == 0) break;

                    world[i % width, i / width] = BitConverter.ToUInt16(bytes, 0);
                    i++;
                }
                return world;
            }
        }

        public static bool SaveWorld(ushort[,] world, int width, int height)
        {
            using (FileStream fs = File.Create("world.dat"))
            {
                for(int iy = 0; iy < height; iy++)
                {
                    for(int ix = 0; ix < width; ix++)
                    {
                        byte[] bytes = BitConverter.GetBytes(world[ix, iy]);
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
                fs.Close();
            }

            return true;
        }
    }
}

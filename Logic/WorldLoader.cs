using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Logic
{
    public static class WorldLoader
    {
        public static void LoadWorld(out int width, out int height, out ushort[,] world, out ushort[,] worldBack)
        {
            world = null;
            worldBack = null;
            width = 0;
            height = 0;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("world.dat", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                WorldSave worldSave = (WorldSave)formatter.Deserialize(stream);
                stream.Close();
                world = worldSave.World;
                worldBack = worldSave.WorldBack;
                width = worldSave.Width;
                height = worldSave.Height;
            }
            catch(Exception fnfE)
            {
                Console.WriteLine(fnfE);
            }
        }

        public static bool SaveWorld(int width, int height, ushort[,] world, ushort[,] worldBack)
        {
            WorldSave worldSave = new WorldSave(world, worldBack, width, height);

            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("world.dat", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, worldSave);
                stream.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }

    [Serializable]
    public class WorldSave
    {
        public int Width;
        public int Height;
        public ushort[,] World;
        public ushort[,] WorldBack;

        public WorldSave(ushort[,] world, ushort[,] worldBack, int width, int height)
        {
            Width = width;
            Height = height;
            World = world;
            WorldBack = worldBack;
        }
    }
}

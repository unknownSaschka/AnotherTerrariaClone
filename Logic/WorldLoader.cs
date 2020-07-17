using ITProject.Model;
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
        public static void LoadWorld(out int width, out int height, out ushort[,] world, out ushort[,] worldBack, out Dictionary<System.Numerics.Vector2, Chest> worldChests, ModelManager manager)
        {
            world = null;
            worldBack = null;
            width = 0;
            height = 0;

            worldChests = new Dictionary<System.Numerics.Vector2, Chest>();

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

                foreach(ChestSave cs in worldSave.Chests)
                {
                    worldChests.Add(new System.Numerics.Vector2(cs.WorldPosX, cs.WorldPosY), new Chest(cs.Content, manager));
                }
            }
            catch(Exception fnfE)
            {
                Console.WriteLine(fnfE);
            }
        }

        public static bool SaveWorld(int width, int height, ushort[,] world, ushort[,] worldBack, Dictionary<System.Numerics.Vector2, Chest> worldChests)
        {
            //Converting Chest Dictionary to Array
            ChestSave[] savedChests = new ChestSave[worldChests.Count];
            int count = 0;
            foreach(KeyValuePair<System.Numerics.Vector2, Chest> chest in worldChests)
            {
                savedChests[count] = new ChestSave(chest.Value.Content.GetSaveInv(), (int)chest.Key.X, (int)chest.Key.Y);
            }

            WorldSave worldSave = new WorldSave(world, worldBack, savedChests, width, height);

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
        public ChestSave[] Chests;

        public WorldSave(ushort[,] world, ushort[,] worldBack, ChestSave[] chests, int width, int height)
        {
            Width = width;
            Height = height;
            World = world;
            WorldBack = worldBack;
            Chests = chests;
        }
    }

    [Serializable]
    public class ChestSave
    {
        public Item[,] Content;
        public  int WorldPosX;
        public int WorldPosY;

        public ChestSave(Item[,] content, int x, int y)
        {
            Content = content;
            WorldPosX = x;
            WorldPosY = y;
        }
    }
}

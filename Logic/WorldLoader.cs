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
        public static void LoadWorld(out int width, out int height, out ushort[,] world, out ushort[,] worldBack, out Dictionary<System.Numerics.Vector2, Chest> worldChests, ModelManager manager, int saveSlot)
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
                //WorldSave worldSave = (WorldSave)formatter.Deserialize(stream);
                WorldSaves worldSaves = (WorldSaves)formatter.Deserialize(stream);
                stream.Close();

                world = worldSaves.SavedWorlds[saveSlot].World;
                worldBack = worldSaves.SavedWorlds[saveSlot].WorldBack;
                width = worldSaves.SavedWorlds[saveSlot].Width;
                height = worldSaves.SavedWorlds[saveSlot].Height;

                foreach(ChestSave cs in worldSaves.SavedWorlds[saveSlot].Chests)
                {
                    worldChests.Add(new System.Numerics.Vector2(cs.WorldPosX, cs.WorldPosY), new Chest(cs.Content, manager));
                }
            }
            catch(Exception fnfE)
            {
                Console.WriteLine(fnfE);
            }
        }

        public static bool SaveWorld(int width, int height, ushort[,] world, ushort[,] worldBack, Dictionary<System.Numerics.Vector2, Chest> worldChests, int saveSlot)
        {
            //Converting Chest Dictionary to Array
            ChestSave[] savedChests = new ChestSave[worldChests.Count];
            int count = 0;
            foreach(KeyValuePair<System.Numerics.Vector2, Chest> chest in worldChests)
            {
                savedChests[count] = new ChestSave(chest.Value.Content.GetSaveInv(), (int)chest.Key.X, (int)chest.Key.Y);
            }

            WorldSave worldSave = new WorldSave(world, worldBack, savedChests, width, height);
            WorldSaves worldSaves = null;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("world.dat", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

                if(stream.Length == 0)
                {
                    worldSaves = new WorldSaves();
                }
                else
                {
                    worldSaves = (WorldSaves)formatter.Deserialize(stream);
                }

                stream.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            worldSaves.SavedWorlds[saveSlot] = worldSave;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("world.dat", FileMode.Create, FileAccess.Write, FileShare.None);

                formatter.Serialize(stream, worldSaves);
                stream.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public static void DeleteWorld(int saveSlot)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("world.dat", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                WorldSaves worldSaves = (WorldSaves)formatter.Deserialize(stream);
                stream.Close();

                worldSaves.SavedWorlds[saveSlot] = null;

                stream = new FileStream("world.dat", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, worldSaves);
                stream.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    [Serializable]
    public class WorldSaves
    {
        public WorldSave[] SavedWorlds;

        public WorldSaves()
        {
            SavedWorlds = new WorldSave[10];
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

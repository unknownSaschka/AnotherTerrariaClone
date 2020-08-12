using ITProject.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Model.World;

namespace ITProject.Logic
{
    public class SaveManagement
    {
        public void SaveItemJson()
        {
            float lightBlocking = 0.1f;
            int miningDuration = 100;

            Dictionary<ushort, ItemInfoWorld> worldItems = new Dictionary<ushort, ItemInfoWorld>();
            worldItems.Add(0, new ItemInfoWorld(0, "Air", 0.1f, true, true, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(1, new ItemInfoWorld(1, "Stone", lightBlocking, true, true, false, false, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Pickaxe));
            worldItems.Add(2, new ItemInfoWorld(2, "Dirt", lightBlocking, true, true, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(3, new ItemInfoWorld(3, "Grass", lightBlocking, true, true, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(4, new ItemInfoWorld(4, "Planks", lightBlocking, true, true, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(5, new ItemInfoWorld(5, "WoodStamp", lightBlocking, true, true, false, false, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(6, new ItemInfoWorld(6, "Wood", lightBlocking, true, true, false, false, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(7, new ItemInfoWorld(7, "Leaves", lightBlocking, true, true, false, false, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(8, new ItemInfoWorld(8, "Water", 0.3f, true, true, true, true, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(9, new ItemInfoWorld(9, "Flowing Water", lightBlocking, false, false, true, true, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(10, new ItemInfoWorld(10, "Lava", lightBlocking, false, true, true, true, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(11, new ItemInfoWorld(11, "Flowing Lava", lightBlocking, false, false, true, true, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(12, new ItemInfoWorld(12, "Chest", 0.05f, true, true, true, false, false, true, 1, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(13, new ItemInfoWorld(13, "Torch", 0.0f, true, true, false, true, true, false, 0, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(14, new ItemInfoWorld(14, "TorchWall", 0.0f, true, true, false, false, true, false, 0, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(15, new ItemInfoWorld(15, "Lamp Block", 0.0f, true, true, false, false, true, false, 1, miningDuration, ItemInfoTools.ItemToolType.Hand));
            worldItems.Add(20, new ItemInfoWorld(20, "Coal Ore", lightBlocking, true, true, false, false, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Pickaxe));
            worldItems.Add(21, new ItemInfoWorld(21, "Iron Ore", lightBlocking, true, true, false, false, false, false, 1, miningDuration, ItemInfoTools.ItemToolType.Pickaxe));
            worldItems.Add(22, new ItemInfoWorld(22, "Diamond Ore", lightBlocking, true, true, false, false, false, false, 2, miningDuration, ItemInfoTools.ItemToolType.Pickaxe));
            worldItems.Add(23, new ItemInfoWorld(23, "Cobalt Ore", lightBlocking, true, true, false, false, false, false, 3, miningDuration, ItemInfoTools.ItemToolType.Pickaxe));
            worldItems.Add(40, new ItemInfoWorld(40, "Coal", lightBlocking, true, false, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.None));
            worldItems.Add(41, new ItemInfoWorld(41, "Iron", lightBlocking, true, false, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.None));
            worldItems.Add(42, new ItemInfoWorld(42, "Diamond", lightBlocking, true, false, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.None));
            worldItems.Add(43, new ItemInfoWorld(43, "Cobalt", lightBlocking, true, false, false, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.None));

            //Tree
            worldItems.Add(70, new ItemInfoWorld(70, "TreeRoot", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(71, new ItemInfoWorld(71, "TreeStamp", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(72, new ItemInfoWorld(72, "TreeLowerLeft", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(73, new ItemInfoWorld(73, "TreeLowerMiddle", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(74, new ItemInfoWorld(74, "TrereLowerRight", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(75, new ItemInfoWorld(75, "TreeUpperLeft", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(76, new ItemInfoWorld(76, "TreeUpperMiddle", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));
            worldItems.Add(77, new ItemInfoWorld(77, "TreeUpperRight", 0.0f, false, false, true, false, false, false, 0, miningDuration, ItemInfoTools.ItemToolType.Axe));

            Dictionary<ushort, ItemInfoTools> toolItems = new Dictionary<ushort, ItemInfoTools>();

            //Tools
            toolItems.Add(48, new ItemInfoTools(48, "Pickaxe", ItemInfoTools.ItemToolType.Pickaxe, 1, 3.0f, false, false));
            toolItems.Add(49, new ItemInfoTools(49, "Iron Pickaxe", ItemInfoTools.ItemToolType.Pickaxe, 2, 1.5f, false, false));
            toolItems.Add(51, new ItemInfoTools(51, "Hammer", ItemInfoTools.ItemToolType.Hammer, 1, 3.0f, false, false));
            toolItems.Add(52, new ItemInfoTools(52, "Iron Hammer", ItemInfoTools.ItemToolType.Hammer, 2, 1.5f, false, false));
            toolItems.Add(54, new ItemInfoTools(54, "Axe", ItemInfoTools.ItemToolType.Axe, 1, 3.0f, false, false));
            toolItems.Add(55, new ItemInfoTools(55, "Iron Axe", ItemInfoTools.ItemToolType.Axe, 2, 1.5f, false, false));
            toolItems.Add(57, new ItemInfoTools(57, "Sword", ItemInfoTools.ItemToolType.Sword, 1, 0f, false, false));
            toolItems.Add(58, new ItemInfoTools(58, "Iron Sword", ItemInfoTools.ItemToolType.Sword, 2, 0f, false, false));
            toolItems.Add(59, new ItemInfoTools(59, "Diamond Sowrd", ItemInfoTools.ItemToolType.Sword, 3, 0f, false, false));

            ItemJSON json = new ItemJSON(worldItems, toolItems);

            using (StreamWriter file = File.CreateText(@"items.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, json);
            }
        }

        public Dictionary<ushort, ItemInfo> LoadItemInfo()
        {
            using (StreamReader r = new StreamReader("items.json"))
            {
                string itemFile = r.ReadToEnd();

                ItemJSON json = JsonConvert.DeserializeObject<ItemJSON>(itemFile);
                Dictionary<ushort, ItemInfo> itemInfo = new Dictionary<ushort, ItemInfo>();

                foreach(var item in json.WorldItems)
                {
                    itemInfo.Add(item.Key, item.Value);
                }

                foreach(var item in json.ToolItems)
                {
                    itemInfo.Add(item.Key, item.Value);
                }

                return itemInfo;
            }
        }

        public static PlayerSave LoadPlayer(int saveSlot)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("players.bin", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                PlayerSaves playerSaves = (PlayerSaves)formatter.Deserialize(stream);
                PlayerSave playerSave = playerSaves.Saves[saveSlot];
                return playerSave;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public static bool SavePlayer(int saveSlot, Player player)
        {
            //Laden aller benötigten Sachen
            PlayerSave playerSave = new PlayerSave(player.Position, player.ItemInventory.GetSaveInv());
            PlayerSaves playerSaves = null;

            //SaveFile laden
            IFormatter formatter = new BinaryFormatter();

            try
            {
                Stream readStream = new FileStream("players.bin", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);


                if(readStream.Length != 0)
                {
                    playerSaves = (PlayerSaves)formatter.Deserialize(readStream);
                }
                else
                {
                    playerSaves = new PlayerSaves();
                }
                
                readStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Fehler beim öffnen der Datei");
                return false;
            }

            //Neue Playerdatei in das Array einfügen
            playerSaves.Saves[saveSlot] = playerSave;

            //Alles neu in die Datei schreiben
            try
            {
                Stream writeStream = new FileStream("players.bin", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(writeStream, playerSaves);
                writeStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Fehler beim schreiben der Datei");
                return false;
            }

            return true;
        }

        public static bool DeletePlayer(int saveSlot)
        {
            PlayerSaves playerSaves = null;

            //SaveFile laden
            IFormatter formatter = new BinaryFormatter();

            try
            {
                Stream readStream = new FileStream("players.bin", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                playerSaves = (PlayerSaves)formatter.Deserialize(readStream);
                readStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Fehler beim öffnen der Datei");
                return false;
            }

            //Neue Playerdatei in das Array einfügen
            playerSaves.Saves[saveSlot] = null;

            //Alles neu in die Datei schreiben
            try
            {
                Stream writeStream = new FileStream("players.bin", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(writeStream, playerSaves);
                writeStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Fehler beim schreiben der Datei");
                return false;
            }

            return true;
        }

        public static PlayerSaves LoadPlayerList()
        {
            PlayerSaves playerSaves = null;

            //SaveFile laden
            IFormatter formatter = new BinaryFormatter();

            try
            {
                Stream readStream = new FileStream("players.bin", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                playerSaves = (PlayerSaves)formatter.Deserialize(readStream);
                readStream.Close();
                return playerSaves;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Fehler beim öffnen der Datei");
                return null;
            }
        }

        public static List<CraftingRecipie> LoadCraftingRecipies()
        {
            using (StreamReader r = new StreamReader("craftings.json"))
            {
                string craftingFile = r.ReadToEnd();
                List<CraftingRecipie> craftingRecipies = new List<CraftingRecipie>();
                List<CraftingRecipie> craftingJson = JsonConvert.DeserializeObject<List<CraftingRecipie>>(craftingFile);

                foreach(CraftingRecipie craft in craftingJson)
                {
                    craftingRecipies.Add(craft);
                }

                return craftingRecipies;
            }
        }

        public static void SaveCraftingRecipiesJSON()
        {
            List<CraftingRecipie> craftingRecipies = new List<CraftingRecipie>();

            craftingRecipies.Add(new CraftingRecipie(new Item(12, 1), new Item[] { new Item(4, 20) }));    //Chest
            craftingRecipies.Add(new CraftingRecipie(new Item(40, 4), new Item[] { new Item(20, 1) }));     //Kohle
            craftingRecipies.Add(new CraftingRecipie(new Item(41, 1), new Item[] { new Item(21, 1), new Item(40, 2) }));    //Eisen
            craftingRecipies.Add(new CraftingRecipie(new Item(43, 1), new Item[] { new Item(23, 2), new Item(40, 4) }));    //Cobalt
            craftingRecipies.Add(new CraftingRecipie(new Item(3, 1), new Item[] { new Item(2, 2) }));   //Grass
            craftingRecipies.Add(new CraftingRecipie(new Item(57, 1), new Item[] { new Item(1, 10), new Item(4, 5) }));     //Sword
            craftingRecipies.Add(new CraftingRecipie(new Item(58, 1), new Item[] { new Item(41, 15), new Item(40, 5) }));   //Iron Sword
            craftingRecipies.Add(new CraftingRecipie(new Item(59, 1), new Item[] { new Item(42, 15), new Item(40, 5) }));   //Diamond Sword


            //Abspeichern
            using (StreamWriter file = File.CreateText(@"craftings.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, craftingRecipies);
            }
        }

        public static void SavePlayerWorldJSON(PlayerSaveInfo[] playerSave, WorldSaveInfo[] worldSave)
        {
            PlayerWorldSaves saves = new PlayerWorldSaves();
            saves.PlayerSaves = playerSave;
            saves.WorldSaves = worldSave;

            using(StreamWriter w = File.CreateText("saves.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(w, saves);
            }
        }

        public static void LoadPlayerWorldJSON(out PlayerSaveInfo[] playerSaves, out WorldSaveInfo[] worldSaves)
        {
            if (!File.Exists(@"saves.json"))
            {
                playerSaves = new PlayerSaveInfo[10];
                worldSaves = new WorldSaveInfo[10];

                return;
            }

            using (StreamReader r = File.OpenText(@"saves.json"))
            {
                string savesJson = r.ReadToEnd();

                //JSON Laden
                PlayerWorldSaves saves = JsonConvert.DeserializeObject<PlayerWorldSaves>(savesJson);
                playerSaves = saves.PlayerSaves;
                worldSaves = saves.WorldSaves;
            }
        }

        public static void DeletePlayerJSON(int saveSlot)
        {
            if (!File.Exists(@"saves.json"))
            {
                return;
            }

            PlayerSaveInfo[] playerSaves;
            WorldSaveInfo[] worldSaves;

            LoadPlayerWorldJSON(out playerSaves, out worldSaves);
            playerSaves[saveSlot] = null;
            SavePlayerWorldJSON(playerSaves, worldSaves);
        }

        public static void DeleteWorldJSON(int saveSlot)
        {
            if (!File.Exists(@"saves.json"))
            {
                return;
            }

            PlayerSaveInfo[] playerSaves;
            WorldSaveInfo[] worldSaves;

            LoadPlayerWorldJSON(out playerSaves, out worldSaves);
            worldSaves[saveSlot] = null;
            SavePlayerWorldJSON(playerSaves, worldSaves);
        }
    }

    public class PlayerWorldSaves
    {
        public PlayerSaveInfo[] PlayerSaves;
        public WorldSaveInfo[] WorldSaves;
    }
}

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

namespace ITProject.Logic
{
    public class SaveManagement
    {
        public void SaveItemJson()
        {
            float lightBlocking = 0.1f;

            Dictionary<ushort, ItemJSON> itemInfo = new Dictionary<ushort, ItemJSON>();
            itemInfo.Add(0, new ItemJSON("Air", 0.1f, true, true, true, false, false, false));
            itemInfo.Add(1, new ItemJSON("Stone", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(2, new ItemJSON("Dirt", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(3, new ItemJSON("Grass", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(4, new ItemJSON("Planks", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(5, new ItemJSON("WoodStamp", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(6, new ItemJSON("Wood", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(7, new ItemJSON("Leaves", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(8, new ItemJSON("Water", 0.3f, true, true, true, true, false, false));
            itemInfo.Add(9, new ItemJSON("Flowing Water", lightBlocking, false, false, true, true, false, false));
            itemInfo.Add(10, new ItemJSON("Lava", lightBlocking, false, true, true, true, false, false));
            itemInfo.Add(11, new ItemJSON("Lamp", 0.0f, true, true, false, false, true, false));
            itemInfo.Add(12, new ItemJSON("Chest", 0.05f, true, true, true, false, false, true));
            itemInfo.Add(20, new ItemJSON("Coal Ore", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(21, new ItemJSON("Iron Ore", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(22, new ItemJSON("Diamond Ore", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(23, new ItemJSON("Cobalt Ore", lightBlocking, true, true, false, false, false, false));
            itemInfo.Add(24, new ItemJSON("Coal", lightBlocking, true, false, false, false, false, false));
            itemInfo.Add(25, new ItemJSON("Iron", lightBlocking, true, false, false, false, false, false));
            itemInfo.Add(26, new ItemJSON("Diamond", lightBlocking, true, false, false, false, false, false));
            itemInfo.Add(27, new ItemJSON("Cobalt", lightBlocking, true, false, false, false, false, false));

            using (StreamWriter file = File.CreateText(@"items.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, itemInfo);
            }
        }

        public Dictionary<ushort, ItemInfo> LoadItemInfo()
        {
            using (StreamReader r = new StreamReader("items.json"))
            {
                string itemFile = r.ReadToEnd();
                Dictionary<ushort, ItemJSON> itemJson = JsonConvert.DeserializeObject<Dictionary<ushort, ItemJSON>>(itemFile);
                Dictionary<ushort, ItemInfo> itemInfo = new Dictionary<ushort, ItemInfo>();
                foreach (KeyValuePair<ushort, ItemJSON> json in itemJson)
                {
                    ItemInfo item = new ItemInfo(json.Key, json.Value.LightBlocking, json.Value.Name, json.Value.Stackable, json.Value.Placable, json.Value.Walkable, json.Value.Fluid, json.Value.LightSource, json.Value.HasInventory);
                    itemInfo.Add(json.Key, item);
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

                Console.WriteLine(readStream.Length);

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
    }
}

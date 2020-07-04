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
            Dictionary<ushort, ItemJSON> itemInfo = new Dictionary<ushort, ItemJSON>();
            itemInfo.Add(0, new ItemJSON("Air", true, true, true, false));
            itemInfo.Add(1, new ItemJSON("Stone", true, true, false, false));
            itemInfo.Add(2, new ItemJSON("Dirt", true, true, false, false));
            itemInfo.Add(3, new ItemJSON("Grass", true, true, false, false));
            itemInfo.Add(4, new ItemJSON("Planks", true, true, false, false));
            itemInfo.Add(5, new ItemJSON("WoodStamp", true, true, false, false));
            itemInfo.Add(6, new ItemJSON("Wood", true, true, false, false));
            itemInfo.Add(7, new ItemJSON("Leaves", true, true, false, false));
            itemInfo.Add(8, new ItemJSON("Water", true, true, true, true));
            itemInfo.Add(9, new ItemJSON("Flowing Water", false, false, true, true));
            itemInfo.Add(10, new ItemJSON("Lava", false, true, true, true));
            itemInfo.Add(20, new ItemJSON("Coal Ore", true, true, false, false));
            itemInfo.Add(21, new ItemJSON("Iron Ore", true, true, false, false));
            itemInfo.Add(22, new ItemJSON("Diamond Ore", true, true, false, false));
            itemInfo.Add(23, new ItemJSON("Cobalt Ore", true, true, false, false));
            itemInfo.Add(24, new ItemJSON("Coal", true, false, false, false));
            itemInfo.Add(25, new ItemJSON("Iron", true, false, false, false));
            itemInfo.Add(26, new ItemJSON("Diamond", true, false, false, false));
            itemInfo.Add(27, new ItemJSON("Cobalt", true, false, false, false));

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
                    ItemInfo item = new ItemInfo(json.Key, json.Value.Name, json.Value.Stackable, json.Value.Placable, json.Value.Walkable, json.Value.Fluid);
                    itemInfo.Add(json.Key, item);
                }
                return itemInfo;
            }
        }

        public PlayerSave LoadPlayer(int saveSlot)
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

        public bool SavePlayer(int saveSlot, Player player)
        {
            //Laden aller benötigten Sachen
            PlayerSave playerSave = new PlayerSave(player.Position, player.ItemInventory.GetSaveInv());
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

        public bool DeletePlayer(int saveSlot)
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

        public PlayerSaves LoadPlayerList()
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

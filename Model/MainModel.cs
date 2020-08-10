using ITProject.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Model.Player;
using static ITProject.Model.World;

namespace ITProject.Model
{
    public class MainModel
    {
        //public static List<Block> Blocks = new List<Block>();
        public static Dictionary<ushort, ItemInfo> Item;
        public static System.Numerics.Vector2 DropItemSize = new System.Numerics.Vector2(0.8f, 0.8f);
        public static List<CraftingRecipie> CraftingRecipies;
        public static Random Random;

        public static int InventoryHeight = 4;
        public static int InventoryWidth = 10;

        private float _playerEnemieDistanceCheck = 2f;
        
        public ModelManager GetModelManager
        {
            get { return _manager; }
        }

        public bool Debug;

        private ModelManager _manager;

        public MainModel(WorldLoadType worldLoadType, PlayerLoadingType playerLoadingType, int playerSaveSlot, int worldSaveSlot, int worldSeed, List<CraftingRecipie> craftingRecipies, Dictionary<ushort, ItemInfo> itemList)
        {
            Item = itemList;
            CraftingRecipies = craftingRecipies;
            Random = new Random();
            _manager = new ModelManager(worldLoadType, playerLoadingType, playerSaveSlot, worldSaveSlot, worldSeed);
        }

        public void Update(double deltaTime)
        {
            _manager.World.Update(deltaTime, _manager.Player, _manager.CollisionHandler);
            _manager.Player.Update(deltaTime, _manager.CollisionHandler);
            _manager.EnemyManager.Update(deltaTime, _manager.CollisionHandler);
            _manager.Player.AudioUpdate(deltaTime, _manager.AudioManager, _manager.World);

            _manager.CollisionHandler.CheckPlayerWithDroppedItems(_manager.Player);
            _manager.CollisionHandler.CheckPlayerWithEnemies(_manager.Player, _manager.EnemyManager.GetNearbyEnemies(_manager.Player.Position, _playerEnemieDistanceCheck));
            _manager.CollisionHandler.CheckPlayerAttack(this);
            
            _manager.Player.UpdateInventory(_manager.Crafting);

            foreach(DamageNumber number in _manager.DamageNumbers.ToList())
            {
                if (number.Remove) _manager.DamageNumbers.Remove(number);
            }
        }

        public void CloseGame()
        {
            Console.WriteLine("Close Game");

            SaveGame(_manager.ActivePlayerSaveSlot, _manager.ActiveWorldSaveSlot);
        }

        private void SaveGame(int playerSlot, int worldSlot)
        {
            //Vielleicht noch später Delete Funktion einbauen

            PlayerSaveInfo[] playerSaves;
            WorldSaveInfo[] worldSaves;
            SaveManagement.LoadPlayerWorldJSON(out playerSaves, out worldSaves);

            playerSaves[playerSlot] = new PlayerSaveInfo(playerSlot, $"Slot{playerSlot}");
            worldSaves[worldSlot] = new WorldSaveInfo(worldSlot, $"Slot{worldSlot}");

            SaveManagement.SavePlayerWorldJSON(playerSaves, worldSaves);

            _manager.World.SaveWorld(worldSlot);
            _manager.Player.SavePlayer(playerSlot);
        }
    }
}

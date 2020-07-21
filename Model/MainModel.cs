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

        public static int InventoryHeight = 4;
        public static int InventoryWidth = 10;
        
        public ModelManager GetModelManager
        {
            get { return _manager; }
        }

        public bool Debug;

        private ModelManager _manager;

        public MainModel(WorldLoadType worldLoadType, PlayerLoadingType playerLoadingType, int playerSaveSlot, int worldSeed)
        {
            SaveManagement saveManagement = new SaveManagement();
            saveManagement.SaveItemJson();
            Item = saveManagement.LoadItemInfo();
            _manager = new ModelManager(worldLoadType, playerLoadingType, playerSaveSlot, worldSeed);
        }

        public void Update(double deltaTime)
        {
            _manager.World.Update(deltaTime, _manager.Player, _manager.CollisionHandler);
            _manager.Player.Update(deltaTime, _manager.CollisionHandler);
            _manager.CollisionHandler.CheckPlayerWithDroppedItems(_manager.Player);
        }

        public void CloseGame()
        {
            _manager.World.SaveWorld();
            _manager.Player.SavePlayer(_manager.ActiveSaveSlot);
        }
    }
}

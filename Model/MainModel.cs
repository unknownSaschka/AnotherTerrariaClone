using ITProject.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class MainModel
    {
        //public static List<Block> Blocks = new List<Block>();
        public static Dictionary<ushort, ItemInfo> Item;
        
        public ModelManager GetModelManager
        {
            get { return _manager; }
        }

        public bool Debug;

        private ModelManager _manager;
        private World _world;

        public MainModel()
        {
            SaveManagement saveManagement = new SaveManagement();
            saveManagement.SaveItemJson();
            Item = saveManagement.LoadItemInfo();
            _world = new World(2500, 1000, World.WorldLoadType.LoadWorld);
            _manager = new ModelManager(_world, saveManagement);
        }

        public void Update(double deltaTime)
        {
            _manager.Player.Update(deltaTime, _manager.CollisionHandler);
        }

        public void CloseGame()
        {
            _world.SaveWorld();
        }
    }
}

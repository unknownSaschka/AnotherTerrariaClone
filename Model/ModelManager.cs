using ITProject.Logic;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Logic.GameExtentions;
using static ITProject.Model.Player;
using static ITProject.Model.World;

namespace ITProject.Model
{
    public class ModelManager
    {
        public bool PlayerIntersection;
        public List<System.Numerics.Vector2> TestedCollisions;
        public Player Player { get; internal set; }
        public World World { get; internal set; }
        public CollisionHandler CollisionHandler{ get; internal set; }
        public SaveManagement SaveManagement { get; }
        public System.Numerics.Vector2 WorldMousePosition;
        public int SelectedInventorySlot;
        public bool InventoryOpen;

        //View Settings
        public float Zoom;
        public System.Numerics.Vector2 RenderDistance;
        public WindowState WindowState;
        public bool ShowGrid;
        public int ActiveSaveSlot;

        public List<ViewItemPositions> ViewItemPositions;

        public ModelManager(WorldLoadType worldLoadType, PlayerLoadingType playerLoadingType, int playerSaveSlot, int worldSeed)
        {
            InitGame(worldLoadType, playerLoadingType, playerSaveSlot, worldSeed);
        }

        private void InitGame(WorldLoadType worldLoadType, PlayerLoadingType playerLoadingType, int playerSaveSlot, int worldSeed)
        {
            ActiveSaveSlot = playerSaveSlot;
            if(worldLoadType == WorldLoadType.LoadWorld)
            {
                World = new World(2500, 1000, worldLoadType, worldSeed);
            }
            else
            {
                World = new World(2500, 1000, worldLoadType, worldSeed);
            }
            

            //int playerPosY = World.SearchGround(2000);
            //Player = new Player(2000, playerPosY + 2, new System.Numerics.Vector2(1.5f, 2.8f));
            Player = new Player(playerLoadingType, playerSaveSlot, new System.Numerics.Vector2(2000, World.SearchGround(2000)));

            CollisionHandler = new CollisionHandler(this);
            PlayerIntersection = false;
            TestedCollisions = new List<System.Numerics.Vector2>();
            RenderDistance = new System.Numerics.Vector2(42, 22);
            WorldMousePosition = new System.Numerics.Vector2();
            Zoom = 20f;
            ShowGrid = false;
            SelectedInventorySlot = 0;
            InventoryOpen = false;
        }
    }
}

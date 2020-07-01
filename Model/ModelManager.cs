using ITProject.Logic;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class ModelManager
    {
        public bool PlayerIntersection;
        public List<System.Numerics.Vector2> TestedCollisions;
        public Player Player { get; }
        public World World { get; }
        public CollisionHandler CollisionHandler{ get; }
        public SaveManagement SaveManagement { get; }
        public System.Numerics.Vector2 WorldMousePosition;
        public int SelectedInventorySlot;
        public bool InventoryOpen;

        //View Settings
        public float Zoom;
        public System.Numerics.Vector2 RenderDistance;
        public WindowState WindowState;
        public bool ShowGrid;

        public ModelManager(World world, SaveManagement saveManagement)
        { 
            World = world;
            SaveManagement = saveManagement;

            int playerPosY = world.SearchGround(2000);
            Player = new Player(2000, playerPosY + 2, new System.Numerics.Vector2(1.5f, 2.8f));
            CollisionHandler = new CollisionHandler(this);
            PlayerIntersection = false;
            TestedCollisions = new List<System.Numerics.Vector2>();
            RenderDistance = new System.Numerics.Vector2(42, 22);
            WorldMousePosition = new System.Numerics.Vector2();
            Zoom = 20;
            ShowGrid = false;
            SelectedInventorySlot = 0;
            InventoryOpen = false;
        }
    }
}

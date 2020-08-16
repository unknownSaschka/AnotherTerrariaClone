using ITProject.Logic;
using ITProject.Model.Enemies;
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
        public EnemyManager EnemyManager { get; internal set; }
        public SaveManagement SaveManagement { get; }
        public System.Numerics.Vector2 WorldMousePosition;
        public Crafting Crafting;

        public AudioManager AudioManager { get; internal set; }

        public int SelectedInventorySlot;
        public bool InventoryOpen;
        public bool CraftingWindowOpen;
        public Chest OpenChest;
        public Item ActiveHoldingItem;

        //View Settings
        public float Zoom;
        public System.Numerics.Vector2 RenderDistance;
        public WindowState WindowState;
        public bool ShowGrid;
        public int ActivePlayerSaveSlot;
        public int ActiveWorldSaveSlot;

        public List<ViewItemPositions> ViewItemPositions;
        public List<ViewItemPositions> ViewChestItemPositions;
        public List<ViewItemPositions> ViewCraftingItemPositions;

        public List<DamageNumber> DamageNumbers;
        public Box2D InventoryRectangle;

        public Hitbox TestSwordHitbox;
        public double SwordHitTimer;
        public double _swordHitDuration = 0.2d;

        public ModelManager(WorldLoadType worldLoadType, PlayerLoadingType playerLoadingType, int playerSaveSlot, int worldSaveSlot, int worldSeed)
        {
            InitGame(worldLoadType, playerLoadingType, playerSaveSlot, worldSaveSlot, worldSeed);
        }

        private void InitGame(WorldLoadType worldLoadType, PlayerLoadingType playerLoadingType, int playerSaveSlot, int worldSaveSlot, int worldSeed)
        {
            ActivePlayerSaveSlot = playerSaveSlot;
            ActiveWorldSaveSlot = worldSaveSlot;

            if(worldLoadType == WorldLoadType.LoadWorld)
            {
                World = new World(2500, 1000, worldLoadType, worldSeed, this, worldSaveSlot);
            }
            else
            {
                World = new World(2500, 1000, worldLoadType, worldSeed, this, worldSaveSlot);
            }


            //int playerPosY = World.SearchGround(2000);
            //Player = new Player(2000, playerPosY + 2, new System.Numerics.Vector2(1.5f, 2.8f));
            AudioManager = new AudioManager();
            Player = new Player(playerLoadingType, playerSaveSlot, new System.Numerics.Vector2(2000, World.SearchGround(2000)), this);
            Crafting = new Crafting();
            CollisionHandler = new CollisionHandler(this);
            EnemyManager = new EnemyManager(AudioManager, Player, World, this);
            PlayerIntersection = false;
            TestedCollisions = new List<System.Numerics.Vector2>();
            RenderDistance = new System.Numerics.Vector2(42, 22);
            WorldMousePosition = new System.Numerics.Vector2();
            Zoom = 20f;
            ShowGrid = false;
            SelectedInventorySlot = 0;
            InventoryOpen = false;
            OpenChest = null;
            ActiveHoldingItem = null;
            DamageNumbers = new List<DamageNumber>();

            SpawnBoss();
        }

        private void SpawnBoss()
        {
            if(World.BossPosition != null)
            {
                EnemyManager.SpawnEnemie(EnemyManager.EnemyType.Boss, World.BossPosition.GetValueOrDefault(), 0f);
            }
        }
    }
}

﻿using ITProject.Model;
using ITProject.Model.Enemies;
using ITProject.View;
using OpenTK.Input;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Text;
using static ITProject.Logic.GameExtentions;
using static ITProject.Logic.ViewButtonPositions;
using static ITProject.Model.World;

namespace ITProject.Logic
{
    public class MainLogic
    {
        public enum GameState { Menu, InGame, None }
        public GameState State;
        public GameState LastState;
        public bool GameStateChanged = true;
        public bool GameClose = false;
        public bool PlayerMining;

        private int _width = 800, _height = 800;
        private string _windowTitle = "Terradox";
        private InputManager inputManager;
        public MainModel MainModel;
        public MainMenuModel MainMenuModel;

        private List<CraftingRecipie> _craftingRecipies;
        private Dictionary<ushort, ItemInfo> _itemList;

        //Accumulator Prinzip zum fixen einer zu hohen deltaTime
        private double fixedDeltaTime = 0.01;
        private double accumulator = 0.0;

        private float? _zoom;

        private double _lastBreakingBlockSoundUpdate = 0d;
        private double _lastBreakingBlockMaxTime = 0.4d;

        private double _blockPlaceTimer = 0f;
        private double _blockPlacePeriod = 0.3d;

        private float _playerMiningDistance = 3.5f;

        public struct WindowPositions
        {
            public int Width;
            public int Height;
            public int X;
            public int Y;
            public OpenTK.WindowState WindowState;
            public bool Focused;
            public Vector2 WindowMousePosition;

            public WindowPositions(int width, int height, int x, int y, OpenTK.WindowState windowState, bool focused, Vector2 windowMousePosition)
            {
                Width = width;
                Height = height;
                X = x;
                Y = y;
                WindowState = windowState;
                Focused = focused;
                WindowMousePosition = windowMousePosition;
            }
        }

        private World.WorldLoadType _worldLoadType;
        private Player.PlayerLoadingType _playerLoadType;
        private int _playerSaveSlot;
        private int _worldSaveSlot;
        private int _worldSeed = 0;

        public MainLogic()
        {
            //Console.WriteLine(Convert.ToUInt32("a"[0]));
            State = GameState.Menu;   //Später wird zuerst View in Menu State gestartet und das Game später geladen
            LastState = GameState.None;

            //World.WorldLoadType worldLoadType = World.WorldLoadType.LoadWorld;
            //Player.PlayerLoadingType playerLoadingType = Player.PlayerLoadingType.LoadPlayer;
            //int saveSlot = 0;

            /*
            World.WorldLoadType worldLoadType;
            Player.PlayerLoadingType playerLoadingType;
            int playerSaveSlot;
            int worldSaveSlot;
            int worldSeed;
            */

            //ConsoleStart(out worldLoadType, out playerLoadingType, out playerSaveSlot, out worldSaveSlot, out worldSeed);

            inputManager = new InputManager();

            //LadeDaten
            SaveManagement saveManagement = new SaveManagement();
            saveManagement.SaveItemJson();
            _itemList = saveManagement.LoadItemInfo();
            SaveManagement.SaveCraftingRecipiesJSON();
            _craftingRecipies = SaveManagement.LoadCraftingRecipies();

            MainMenuModel = new MainMenuModel();
            //_mainModel = new MainModel(worldLoadType, playerLoadingType, playerSaveSlot, worldSaveSlot, worldSeed, craftingRecipies, itemList);

            MainView view = new MainView(_width, _height, OpenTK.Graphics.GraphicsMode.Default, _windowTitle, this, MainModel, MainMenuModel);
            view.VSync = OpenTK.VSyncMode.Off;
            view.Run();
        }

        private void ConsoleStart(out World.WorldLoadType worldLoadType, out Player.PlayerLoadingType playerLoadingType, out int saveSlot, out int worldSaveSlot, out int worldSeed)
        {
            Console.WriteLine("Wie soll die Welt geladen werden?");
            Console.WriteLine("1: Neue Welt, 2: Lade Welt");
            string worldLoad = Console.ReadLine();
            worldLoadType = (World.WorldLoadType) int.Parse(worldLoad);
            worldSeed = 1337;
            worldSaveSlot = -1;

            if((int)worldLoadType == 1)
            {
                Console.WriteLine("WorldSeed? (Standard: 1337)");
                string seed = Console.ReadLine();

                if(!int.TryParse(seed, out worldSeed))
                {
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(seed);
                    worldSeed = BitConverter.ToInt32(bytes, 0);
                }

                Console.WriteLine("Welcher Save Slot? (0 - 9)");
                string worldSlot = Console.ReadLine();
                worldSaveSlot = int.Parse(worldSlot);
            }
            else if((int)worldLoadType == 2)
            {
                Console.WriteLine("Von welchem Save Slot? (0 - 9)");
                string worldSlot = Console.ReadLine();
                worldSaveSlot = int.Parse(worldSlot);
            }

            Console.WriteLine("Wie soll der Spieler geladen werden?");
            Console.WriteLine("1: Neuer Spieler, 2: Lade Spieler");
            string playerLoad = Console.ReadLine();
            playerLoadingType = (Player.PlayerLoadingType)int.Parse(playerLoad);

            Console.WriteLine("Welcher PlayerSlot soll geladen werden? (0 - 9)");
            string slot = Console.ReadLine();
            saveSlot = int.Parse(slot);

            if ((int)worldLoadType == 1)
            {
                Console.WriteLine("Generiere Welt...");
            }
        }

        public void Update(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double deltaTime, float? zoom)
        {
            _zoom = zoom;
            
            accumulator += deltaTime;

            while(accumulator >= fixedDeltaTime)        //Physik wird zu einer festen zeit berechnet. Falls deltaTime (Spiel laggt, etc.) werden die Physikberechnungen dennoch weiterhin zu jedem festen Schritt berechnet und ggf. nachberechnet
            {
                FixedUpdate(keyboardState, cursorState, windowPositions, fixedDeltaTime);
                accumulator -= fixedDeltaTime;
            }
        }

        public void ChangeGameState(GameState gameState)
        {
            LastState = State;
            State = gameState;
            GameStateChanged = true;
        }

        public void FixedUpdate(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double fixedDeltaTime)
        {
            inputManager.Update(keyboardState, cursorState);

            switch (State)
            {
                case GameState.InGame:
                    PerformViewChanges(windowPositions, keyboardState);
                    MainModel.GetModelManager.WorldMousePosition = CalculateViewToWorldPosition(new Vector2(windowPositions.WindowMousePosition.X, windowPositions.WindowMousePosition.Y), MainModel.GetModelManager.Player.Position, windowPositions);

                    int mouse = inputManager.GetMouseWheelDifference();
                    if (mouse != 0)
                    {
                        MainModel.GetModelManager.SelectedInventorySlot -= mouse;

                        if (MainModel.GetModelManager.SelectedInventorySlot < 0) MainModel.GetModelManager.SelectedInventorySlot = 9;
                        if (MainModel.GetModelManager.SelectedInventorySlot > 9) MainModel.GetModelManager.SelectedInventorySlot = 0;
                    }
                    InventoryBarNumberKeys(keyboardState);

                    PerformPlayerMovement(keyboardState, cursorState, windowPositions, fixedDeltaTime);
                    PerformPlayerActions(keyboardState, cursorState, windowPositions, fixedDeltaTime);
                    MainModel.Update(fixedDeltaTime);
                    break;
                case GameState.Menu:
                    if (inputManager.GetMouseButtonPressed(MouseButton.Left) && windowPositions.Focused)
                    {
                        Vector2 mouseMiddle = new Vector2(windowPositions.WindowMousePosition.X - (windowPositions.Width / 2), -(windowPositions.WindowMousePosition.Y - (windowPositions.Height / 2)));
                        MenuButtonPresses(mouseMiddle, MainMenuModel.ButtonPositions, keyboardState);
                    }

                    break;
            }
        }

        public void CloseGame()
        {
            MainModel.CloseGame();

            //Console.ReadLine();
        }

        //In-Game Functions
        public void PerformViewChanges(WindowPositions windowPositions, KeyboardState keyboardState)
        {
            if (windowPositions.Focused)
            {
                if (keyboardState.IsKeyDown(Key.ControlLeft))
                {
                    MainModel.GetModelManager.Zoom -= inputManager.GetMouseWheelDifference() * 0.5f;
                }

                if (inputManager.GetKeyPressed(Key.G))
                {
                    MainModel.GetModelManager.ShowGrid = !MainModel.GetModelManager.ShowGrid;
                }
            }

            if (MainModel.GetModelManager.Zoom > 300f) MainModel.GetModelManager.Zoom = 300f;
            else if (MainModel.GetModelManager.Zoom < 1f) MainModel.GetModelManager.Zoom = 1f;
        }

        public void PerformPlayerMovement(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double fixedDeltaTime)
        {
            if (windowPositions.Focused)
            {
                MainModel.GetModelManager.Player.WalkSpeed = 0f;
                if (keyboardState.IsKeyDown(Key.A))
                {
                    MainModel.GetModelManager.Player.Walk(false);

                }
                if (keyboardState.IsKeyDown(Key.D))
                {
                    MainModel.GetModelManager.Player.Walk(true);
                }

                if (keyboardState.IsKeyDown(Key.Space))
                {
                    MainModel.GetModelManager.Player.Jump(fixedDeltaTime);
                }

                if (keyboardState.IsKeyUp(Key.Space))
                {
                    MainModel.GetModelManager.Player.NoJump();
                }
            }
        }

        public void PerformPlayerActions(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double fixedDeltaTime)
        {
            PlayerMining = false;
            MainModel.GetModelManager.CollisionHandler.SetPlayerAttack(null, Vector2.Zero);
            _blockPlaceTimer += fixedDeltaTime;

            if (windowPositions.Focused && cursorState.IsButtonDown(MouseButton.Left) && 
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                Vector2 mouseMiddle = new Vector2(windowPositions.WindowMousePosition.X - (windowPositions.Width / 2), -(windowPositions.WindowMousePosition.Y - (windowPositions.Height / 2)));
                PlayerLeftClick(fixedDeltaTime, mouseMiddle);
            }
            else
            {
                _lastBreakingBlockSoundUpdate = _lastBreakingBlockMaxTime;
            }

            if (windowPositions.Focused && cursorState.IsButtonDown(MouseButton.Right) &&
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                Vector2 mouseMiddle = new Vector2(windowPositions.WindowMousePosition.X - (windowPositions.Width / 2), -(windowPositions.WindowMousePosition.Y - (windowPositions.Height / 2)));
                PlayerRightClick(mouseMiddle);
            }

            /*
            if (windowPositions.Focused && inputManager.GetKeyPressed(Key.Q) &&
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                MainModel.GetModelManager.World.PlaceBlock(MainModel.GetModelManager.WorldMousePosition, 8, MainModel.GetModelManager);
            }
            */

            if(windowPositions.Focused && inputManager.GetKeyPressed(Key.E))
            {
                MainModel.GetModelManager.InventoryOpen = !MainModel.GetModelManager.InventoryOpen;
                MainModel.GetModelManager.OpenChest = null;
                MainModel.GetModelManager.CraftingWindowOpen = false;
            }

            if (MainModel.GetModelManager.InventoryOpen && inputManager.GetKeyPressed(Key.Escape))
            {
                MainModel.GetModelManager.InventoryOpen = !MainModel.GetModelManager.InventoryOpen;
                MainModel.GetModelManager.CraftingWindowOpen = false;
                MainModel.GetModelManager.OpenChest = null;
            }

            if (windowPositions.Focused && inputManager.GetKeyPressed(Key.I) &&
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                MainModel.GetModelManager.World.AddDroppedItem(new WorldItem(MainModel.GetModelManager.WorldMousePosition, new Vector2(10f, 3f), new Vector2(0.8f, 0.8f), new Item(1, 1)));
            }

            if(windowPositions.Focused && inputManager.GetKeyPressed(Key.C))
            {
                MainModel.GetModelManager.OpenChest = null;

                if (MainModel.GetModelManager.CraftingWindowOpen)
                {
                    MainModel.GetModelManager.CraftingWindowOpen = false;
                    MainModel.GetModelManager.InventoryOpen = false;
                }
                else
                {
                    MainModel.GetModelManager.CraftingWindowOpen = true;
                    MainModel.GetModelManager.InventoryOpen = true;
                }

                
            }

            //Debug
            if (windowPositions.Focused && inputManager.GetKeyPressed(Key.K) &&
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                MainModel.GetModelManager.World.PlaceBlock(MainModel.GetModelManager.WorldMousePosition, 12, MainModel.GetModelManager);
            }

            if(windowPositions.Focused && inputManager.GetKeyPressed(Key.G))
            {
                MainModel.GetModelManager.EnemyManager.SpawnEnemie(Model.Enemies.EnemyManager.EnemyType.Boss, MainModel.GetModelManager.Player.Position, 10f);
            }
        }

        public Vector2 CalculateViewToWorldPosition(Vector2 viewPosition, Vector2 playerPosition, WindowPositions windowPositions)
        {
            Vector2 worldPosition = new Vector2(playerPosition.X, playerPosition.Y);
            float ratio = (float)windowPositions.Width / (float)windowPositions.Height;

            Vector2 centeredPos = new Vector2(viewPosition.X - windowPositions.Width / 2, windowPositions.Height / 2 - viewPosition.Y);
            worldPosition.X = playerPosition.X + ((centeredPos.X *  (float)_zoom) / (500));
            worldPosition.Y = playerPosition.Y + ((centeredPos.Y * (float)_zoom) / (500));

            return worldPosition;
        }

        private void PlayerLeftClick(double deltaTime, Vector2 mousePositionMiddle)
        {
            if (MainModel.GetModelManager.InventoryOpen)
            {
                //Prüfen, welche Itemposition angeklickt wurde
                if (inputManager.GetMouseButtonPressed(MouseButton.Left))
                {
                    int itemX, itemY;

                    //Player Inventory
                    if (CheckInventoryClickedPosition(mousePositionMiddle, MainModel.GetModelManager.ViewItemPositions, out itemX, out itemY))
                    {
                        MainModel.GetModelManager.Player.ItemInventory.LeftClick(itemX, itemY);
                    }
                    else if (CheckInventoryClickedPosition(mousePositionMiddle, MainModel.GetModelManager.ViewChestItemPositions, out itemX, out itemY))
                    {
                        MainModel.GetModelManager.OpenChest.Content.LeftClick(itemX, itemY);
                    }
                    else if (CheckInventoryClickedPosition(mousePositionMiddle, MainModel.GetModelManager.ViewCraftingItemPositions, out itemX, out itemY))
                    {
                        CraftingRecipie clickedRecipie = MainModel.GetModelManager.Player.CraftableRecipies[itemX];

                        if (MainModel.GetModelManager.ActiveHoldingItem != null)
                        {
                            if (MainModel.GetModelManager.ActiveHoldingItem.ID == clickedRecipie.ResultItem.ID)
                            {
                                if (!MainModel.Item[MainModel.GetModelManager.ActiveHoldingItem.ID].Stackable || !MainModel.Item[clickedRecipie.ResultItem.ID].Stackable) { return; }
                                else if ((MainModel.GetModelManager.ActiveHoldingItem.Amount + clickedRecipie.ResultItem.Amount) > 99) { return; }
                                else { MainModel.GetModelManager.ActiveHoldingItem.Amount = (clickedRecipie.ResultItem.Amount + MainModel.GetModelManager.ActiveHoldingItem.Amount); }
                            }
                            else { return; }
                        }
                        else { MainModel.GetModelManager.ActiveHoldingItem = new Item(clickedRecipie.ResultItem.ID, clickedRecipie.ResultItem.Amount); }

                        foreach (Item item in clickedRecipie.NeededItems)
                        {
                            MainModel.GetModelManager.Player.ItemInventory.RemoveItemAmount(item);
                        }
                    }
                    else if (!CheckIfWithin(ConvertVector(mousePositionMiddle), MainModel.GetModelManager.InventoryRectangle))
                    {
                        Item activeItem = MainModel.GetModelManager.ActiveHoldingItem;

                        if (activeItem != null)
                        {
                            MainModel.GetModelManager.World.AddDroppedItem(new WorldItem(MainModel.GetModelManager.Player.Position, new Vector2(10f, 3f), new Vector2(0.8f, 0.8f), activeItem));
                            MainModel.GetModelManager.ActiveHoldingItem = null;
                        }
                    }
                }
            }

            if (!MainModel.GetModelManager.InventoryOpen && State == GameState.InGame)
            {
                PlayerMining = true;
                Inventory playerInventory = MainModel.GetModelManager.Player.ItemInventory;
                ItemInfo itemInfo = MainModel.Item[playerInventory.GetItemID(MainModel.GetModelManager.SelectedInventorySlot, 0)];

                float miningSpeed = 1;
                int toolLevel = 0;
                ItemInfoTools.ItemToolType toolType = ItemInfoTools.ItemToolType.Hand;

                if (itemInfo.GetType().Name == "ItemInfoTools")
                {
                    if(((ItemInfoTools)itemInfo).ToolType == ItemInfoTools.ItemToolType.Sword)
                    {
                        MainModel.GetModelManager.CollisionHandler.SetPlayerAttack((ItemInfoTools)itemInfo, mousePositionMiddle);
                        return;
                    }
                    else
                    {
                        MainModel.GetModelManager.CollisionHandler.SetPlayerAttack((ItemInfoTools)itemInfo, mousePositionMiddle);
                    }

                    miningSpeed = ((ItemInfoTools)itemInfo).MiningDuration;
                    toolLevel = ((ItemInfoTools)itemInfo).ToolLevel;
                    toolType = ((ItemInfoTools)itemInfo).ToolType;
                }
                else
                {
                    MainModel.GetModelManager.CollisionHandler.SetPlayerAttack(new ItemInfoTools(0, "Hand", ItemInfoTools.ItemToolType.Hand, 0, 1, false, false), mousePositionMiddle);
                }

                if (!CheckPlacingDistance(MainModel.GetModelManager.Player.Position, MainModel.GetModelManager.WorldMousePosition)) return;

                //Block Breaking Sound Update
                ushort block;
                if(toolType == ItemInfoTools.ItemToolType.Hammer)
                {
                    block = MainModel.GetModelManager.World.GetBlockType(MainModel.GetModelManager.WorldMousePosition, WorldLayer.Background);
                }
                else
                {
                    block = MainModel.GetModelManager.World.GetBlockType(MainModel.GetModelManager.WorldMousePosition, WorldLayer.Foreground);
                }

                if (block != 0)
                {
                    _lastBreakingBlockSoundUpdate += deltaTime;
                    if(_lastBreakingBlockSoundUpdate > _lastBreakingBlockMaxTime)
                    {
                        _lastBreakingBlockSoundUpdate = 0d;
                        MainModel.GetModelManager.AudioManager.PlaySound(block);
                    }
                }

                ushort removedItem = MainModel.GetModelManager.World.RemoveBlock(MainModel.GetModelManager.WorldMousePosition, miningSpeed, toolLevel, toolType);

                if (removedItem != 0)
                {
                    bool test = playerInventory.AddItemUnsorted(new Item(removedItem, 1));
                }
            }
        }

        private void PlayerRightClick(Vector2 mousePositionMiddle)
        {
            //Rechtsklick, wenn Inventar offen ist
            if (MainModel.GetModelManager.InventoryOpen)
            {
                //Prüfen, welche Itemposition angeklickt wurde
                if (inputManager.GetMouseButtonPressed(MouseButton.Right))
                {
                    int itemX, itemY;
                    if (CheckInventoryClickedPosition(mousePositionMiddle, MainModel.GetModelManager.ViewItemPositions, out itemX, out itemY))
                    {
                        MainModel.GetModelManager.Player.ItemInventory.RightClick(itemX, itemY);
                    }
                    else if (CheckInventoryClickedPosition(mousePositionMiddle, MainModel.GetModelManager.ViewChestItemPositions, out itemX, out itemY))
                    {
                        MainModel.GetModelManager.OpenChest.Content.RightClick(itemX, itemY);
                    }
                }
            }
            else
            {
                Vector2 mouseOverBlockPos = new Vector2((int)MainModel.GetModelManager.WorldMousePosition.X, (int)MainModel.GetModelManager.WorldMousePosition.Y);

                if (!CheckPlacingDistance(MainModel.GetModelManager.Player.Position, MainModel.GetModelManager.WorldMousePosition)) return;

                //Zuerst prüfen, ob Chest geöffnet werden möchte
                if (MainModel.GetModelManager.World.HasInventory(mouseOverBlockPos))
                {
                    MainModel.GetModelManager.OpenChest = MainModel.GetModelManager.World.GetChest(mouseOverBlockPos);
                    MainModel.GetModelManager.InventoryOpen = true;
                }
                //Wenn keine Chest an dem Ort, dann versuchen, Block zu platzieren
                else
                {
                    if(_blockPlaceTimer < _blockPlacePeriod)
                    {
                        return;
                    }
                    _blockPlaceTimer = 0d;

                    Player player = MainModel.GetModelManager.Player;
                    Hitbox playerHitbox = new Hitbox(player.Position, player.Size, Hitbox.HitboxType.Player);
                    Hitbox blockHitbox = new Hitbox(mouseOverBlockPos, new Vector2(1f, 1f), Hitbox.HitboxType.Block);

                    if (!MainModel.GetModelManager.CollisionHandler.Intersects(playerHitbox, blockHitbox))
                    {
                        var enemyHitboxes = from enemie in MainModel.GetModelManager.EnemyManager.Enemies
                                            where Vector2.Distance(blockHitbox.Position, enemie.Position) < 5
                                            select enemie.GetCollisionHitbox();

                        bool canPlace = true;
                        foreach (Hitbox enemyHitbox in enemyHitboxes)
                        {
                            if (MainModel.GetModelManager.CollisionHandler.Intersects(blockHitbox, enemyHitbox))
                            {
                                //Console.WriteLine($"CanPlace False");
                                canPlace = false;
                                break;
                            }
                        }

                        if (canPlace)
                        {
                            Inventory playerInventory = MainModel.GetModelManager.Player.ItemInventory;
                            ushort selectedItem = playerInventory.GetItemID(MainModel.GetModelManager.SelectedInventorySlot, 0);

                            if (MainModel.Item[selectedItem].Placable)
                            {
                                if (MainModel.GetModelManager.World.PlaceBlock(MainModel.GetModelManager.WorldMousePosition, selectedItem, MainModel.GetModelManager))
                                {
                                    playerInventory.RemoveItemAmount(new Item(selectedItem, 1), MainModel.GetModelManager.SelectedInventorySlot, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CheckPlacingDistance(Vector2 playerPosition, Vector2 blockPosition)
        {
            if(Vector2.Distance(playerPosition, blockPosition) < _playerMiningDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckInventoryClickedPosition(Vector2 mousePosition, List<ViewItemPositions> viewItemPositions, out int x, out int y)
        {
            x = 0;
            y = 0;

            if (viewItemPositions == null) return false;

            foreach (ViewItemPositions pos in viewItemPositions)
            {
                if (CheckIfWithin(new OpenTK.Vector2(mousePosition.X, mousePosition.Y), pos.Position, pos.Size, true))
                {
                    x = pos.InventoryX;
                    y = pos.InventoryY;
                    return true;
                }
            }

            return false;
        }

        private void InventoryBarNumberKeys(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Key.Number1)) MainModel.GetModelManager.SelectedInventorySlot = 0;
            else if (keyboardState.IsKeyDown(Key.Number2)) MainModel.GetModelManager.SelectedInventorySlot = 1;
            else if (keyboardState.IsKeyDown(Key.Number3)) MainModel.GetModelManager.SelectedInventorySlot = 2;
            else if (keyboardState.IsKeyDown(Key.Number4)) MainModel.GetModelManager.SelectedInventorySlot = 3;
            else if (keyboardState.IsKeyDown(Key.Number5)) MainModel.GetModelManager.SelectedInventorySlot = 4;
            else if (keyboardState.IsKeyDown(Key.Number6)) MainModel.GetModelManager.SelectedInventorySlot = 5;
            else if (keyboardState.IsKeyDown(Key.Number7)) MainModel.GetModelManager.SelectedInventorySlot = 6;
            else if (keyboardState.IsKeyDown(Key.Number8)) MainModel.GetModelManager.SelectedInventorySlot = 7;
            else if (keyboardState.IsKeyDown(Key.Number9)) MainModel.GetModelManager.SelectedInventorySlot = 8;
            else if (keyboardState.IsKeyDown(Key.Number0)) MainModel.GetModelManager.SelectedInventorySlot = 9;
        }

        //Menu Functions
        private void MenuButtonPresses(Vector2 mousePositionMiddle, List<ViewButtonPositions> buttonPositions, KeyboardState keyboardState)
        {
            foreach(ViewButtonPositions button in buttonPositions)
            {
                if (CheckIfWithin(new OpenTK.Vector2(mousePositionMiddle.X, mousePositionMiddle.Y), button.Position, button.Size, true))
                {
                    switch (button.ButtonType)
                    {
                        case ButtonTypes.ToWorldList:
                            MainMenuModel.ScreenState = MainMenuModel.Screen.WorldSelect;
                            break;
                        case ButtonTypes.World:
                            if (button.ButtonInput.Equals("Empty"))
                            {
                                _worldLoadType = World.WorldLoadType.NewWorld;

                                Console.WriteLine("Welt Seed eingeben");
                                string seed = Console.ReadLine();
                                if (!int.TryParse(seed, out _worldSeed))
                                {
                                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(seed);
                                    _worldSeed = BitConverter.ToInt32(bytes, 0);
                                }
                            }
                            else
                            {
                                if (keyboardState.IsKeyDown(Key.Delete))
                                {
                                    WorldLoader.DeleteWorld(button.Slot);
                                    SaveManagement.DeleteWorldJSON(button.Slot);
                                    MainMenuModel.AvailableWorldSaves[button.Slot] = null;
                                    break;
                                }
                                else
                                {
                                    _worldLoadType = World.WorldLoadType.LoadWorld;
                                }
                            }
                            _worldSaveSlot = button.Slot;
                            MainMenuModel.ScreenState = MainMenuModel.Screen.PlayerSelect;
                            break;
                        case ButtonTypes.Player:
                            if (button.ButtonInput.Equals("Empty"))
                            {
                                _playerLoadType = Player.PlayerLoadingType.NewPlayer;
                            }
                            else
                            {
                                if (keyboardState.IsKeyDown(Key.Delete))
                                {
                                    SaveManagement.DeletePlayer(button.Slot);
                                    SaveManagement.DeletePlayerJSON(button.Slot);
                                    MainMenuModel.AvailablePlayerSaves[button.Slot] = null;
                                    break;
                                }
                                else
                                {
                                    _playerLoadType = Player.PlayerLoadingType.LoadPlayer;
                                }
                            }
                            _playerSaveSlot = button.Slot;
                            StartGame();
                            break;
                        case ButtonTypes.Back:
                            MainMenuBackButton();
                            break;
                        case ButtonTypes.CloseGame:
                            GameClose = true;
                            break;
                    }
                }
            }
        }

        private void StartGame()
        {
            MainModel = new MainModel(_worldLoadType, _playerLoadType, _playerSaveSlot, _worldSaveSlot, _worldSeed, _craftingRecipies, _itemList);
            ChangeGameState(GameState.InGame);
        }

        private void MainMenuBackButton()
        {
            switch (MainMenuModel.ScreenState)
            {
                case MainMenuModel.Screen.WorldSelect:
                    MainMenuModel.ScreenState = MainMenuModel.Screen.MainMenuStart;
                    break;
                case MainMenuModel.Screen.PlayerSelect:
                    MainMenuModel.ScreenState = MainMenuModel.Screen.WorldSelect;
                    break;
            }
        }

        private bool MouseInsideWindow(Vector2 mousePosition, Vector2 WindowSize)
        {
            if(mousePosition.X >= 0 && mousePosition.X <= WindowSize.X &&
               mousePosition.Y >= 0 && mousePosition.Y <= WindowSize.Y)
            {
                return true;
            }
            return false;
        }
    }
}

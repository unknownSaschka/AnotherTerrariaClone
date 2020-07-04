using ITProject.Model;
using ITProject.View;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace ITProject.Logic
{
    public class MainLogic
    {
        private int _width = 800, _height = 800;
        private string _windowTitle = "Game";
        private InputManager inputManager;
        private MainModel _mainModel;

        //Accumulator Prinzip zum fixen einer zu hohen deltaTime
        private double fixedDeltaTime = 0.01;
        private double accumulator = 0.0;

        public struct WindowPositions
        {
            public int Width;
            public int Height;
            public int X;
            public int Y;
            public float Zoom;
            public bool Fullscreen;
            public OpenTK.WindowState WindowState;
            public bool Focused;
            public Vector2 WindowMousePosition;
        }

        public MainLogic()
        {
            //Console.WriteLine(Convert.ToUInt32("a"[0]));


            inputManager = new InputManager();
            _mainModel = new MainModel();
            
            MainView view = new MainView(_width, _height, OpenTK.Graphics.GraphicsMode.Default, _windowTitle, this, _mainModel);
            view.VSync = OpenTK.VSyncMode.Off;
            view.Run();
        }

        public void Update(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double deltaTime)
        {
            _mainModel.GetModelManager.WorldMousePosition = CalculateViewToWorldPosition(new Vector2(windowPositions.WindowMousePosition.X, windowPositions.WindowMousePosition.Y), _mainModel.GetModelManager.Player.Position, windowPositions);
            
            accumulator += deltaTime;

            while(accumulator >= fixedDeltaTime)        //Physik wird zu einer festen zeit berechnet. Falls deltaTime (Spiel laggt, etc.) werden die Physikberechnungen dennoch weiterhin zu jedem festen Schritt berechnet und ggf. nachberechnet
            {
                FixedUpdate(keyboardState, cursorState, windowPositions, fixedDeltaTime);
                accumulator -= fixedDeltaTime;
            }
        }

        public void FixedUpdate(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double fixedDeltaTime)
        {
            inputManager.Update(keyboardState, cursorState);
            PerformViewChanges(windowPositions, keyboardState);

            int mouse = inputManager.GetMouseWheelDifference();
            if(mouse != 0)
            {
                _mainModel.GetModelManager.SelectedInventorySlot -= mouse;

                if (_mainModel.GetModelManager.SelectedInventorySlot < 0) _mainModel.GetModelManager.SelectedInventorySlot = 9;
                if (_mainModel.GetModelManager.SelectedInventorySlot > 9) _mainModel.GetModelManager.SelectedInventorySlot = 0;
            }


            PerformPlayerMovement(keyboardState, cursorState, windowPositions, fixedDeltaTime);
            _mainModel.Update(fixedDeltaTime);
            PerformPlayerActions(keyboardState, cursorState, windowPositions, fixedDeltaTime);
        }

        public void CloseGame()
        {
            _mainModel.CloseGame();
        }

        public void PerformViewChanges(WindowPositions windowPositions, KeyboardState keyboardState)
        {
            if (windowPositions.Focused)
            {
                if (keyboardState.IsKeyDown(Key.ControlLeft))
                {
                    _mainModel.GetModelManager.Zoom -= inputManager.GetMouseWheelDifference() * 0.5f;
                }

                if (inputManager.GetKeyPressed(Key.KeypadPlus))
                {
                    _mainModel.GetModelManager.RenderDistance = new Vector2(_mainModel.GetModelManager.RenderDistance.X + 1, _mainModel.GetModelManager.RenderDistance.Y + 1);
                }
                if (inputManager.GetKeyPressed(Key.KeypadMinus))
                {
                    _mainModel.GetModelManager.RenderDistance = new Vector2(_mainModel.GetModelManager.RenderDistance.X - 1, _mainModel.GetModelManager.RenderDistance.Y - 1);
                }

                if (inputManager.GetKeyPressed(Key.F))
                {
                    if (windowPositions.WindowState == OpenTK.WindowState.Normal) _mainModel.GetModelManager.WindowState = OpenTK.WindowState.Fullscreen;
                    else _mainModel.GetModelManager.WindowState = OpenTK.WindowState.Normal;
                }

                if (inputManager.GetKeyPressed(Key.G))
                {
                    _mainModel.GetModelManager.ShowGrid = !_mainModel.GetModelManager.ShowGrid;
                }
            }

            if (_mainModel.GetModelManager.Zoom > 300f) _mainModel.GetModelManager.Zoom = 300f;
            else if (_mainModel.GetModelManager.Zoom < 1f) _mainModel.GetModelManager.Zoom = 1f;
        }

        public void PerformPlayerMovement(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double fixedDeltaTime)
        {
            if (windowPositions.Focused)
            {
                _mainModel.GetModelManager.Player.WalkSpeed = 0f;
                if (keyboardState.IsKeyDown(Key.A))
                {
                    _mainModel.GetModelManager.Player.Walk(false);

                }
                if (keyboardState.IsKeyDown(Key.D))
                {
                    _mainModel.GetModelManager.Player.Walk(true);
                }

                if (keyboardState.IsKeyDown(Key.Space))
                {
                    _mainModel.GetModelManager.Player.Jump(fixedDeltaTime);
                }

                if (keyboardState.IsKeyUp(Key.Space))
                {
                    _mainModel.GetModelManager.Player.NoJump();
                }
            }
        }

        public void PerformPlayerActions(KeyboardState keyboardState, MouseState cursorState, WindowPositions windowPositions, double fixedDeltaTime)
        {
            if (windowPositions.Focused && cursorState.IsButtonDown(MouseButton.Left) && 
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                PlayerLeftClick();
            }

            if (windowPositions.Focused && cursorState.IsButtonDown(MouseButton.Right) &&
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                Player player = _mainModel.GetModelManager.Player;
                Hitbox playerHitbox = new Hitbox(player.Position, player.Size, Hitbox.HitboxType.Player);
                Vector2 roundedMousePos = new Vector2((int)_mainModel.GetModelManager.WorldMousePosition.X, (int)_mainModel.GetModelManager.WorldMousePosition.Y);
                Hitbox blockHitbox = new Hitbox(roundedMousePos, new Vector2(1f, 1f), Hitbox.HitboxType.Block);

                if (!_mainModel.GetModelManager.CollisionHandler.Intersects(playerHitbox, blockHitbox))
                {
                    PlayerRightClick();
                }
            }

            if (windowPositions.Focused && inputManager.GetKeyPressed(Key.Q) &&
                MouseInsideWindow(windowPositions.WindowMousePosition, new Vector2(windowPositions.Width, windowPositions.Height)))
            {
                _mainModel.GetModelManager.World.PlaceBlock(_mainModel.GetModelManager.WorldMousePosition, 8);
            }

            if(windowPositions.Focused && inputManager.GetKeyPressed(Key.E))
            {
                _mainModel.GetModelManager.InventoryOpen = !_mainModel.GetModelManager.InventoryOpen;
            }
        }

        public Vector2 CalculateViewToWorldPosition(Vector2 viewPosition, Vector2 playerPosition, WindowPositions windowPositions)
        {
            Vector2 worldPosition = new Vector2(playerPosition.X, playerPosition.Y);
            float ratio = (float)windowPositions.Width / windowPositions.Height;

            Vector2 centeredPos = new Vector2(viewPosition.X - windowPositions.Width / 2, windowPositions.Height / 2 - viewPosition.Y);

            worldPosition.X = playerPosition.X + ((centeredPos.X * ratio * 800 / windowPositions.Width) / 20) * (windowPositions.Zoom / 20);
            worldPosition.Y = playerPosition.Y + ((centeredPos.Y * ratio * 800 / windowPositions.Width) / 20) * (windowPositions.Zoom / 20);

            return worldPosition;
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

        private void PlayerLeftClick()
        {
            Inventory playerInventory = _mainModel.GetModelManager.Player.ItemInventory;
            ushort removedItem = _mainModel.GetModelManager.World.RemoveBlock(_mainModel.GetModelManager.WorldMousePosition);
            bool test = playerInventory.AddItemUnsorted(new Item(removedItem, 1));
            Console.WriteLine(test);
        }

        private void PlayerRightClick()
        {
            Inventory playerInventory = _mainModel.GetModelManager.Player.ItemInventory;
            ushort selectedItem = playerInventory.GetItemID(_mainModel.GetModelManager.SelectedInventorySlot, 0);

            if (MainModel.Item[selectedItem].Placable)
            {
                if (_mainModel.GetModelManager.World.PlaceBlock(_mainModel.GetModelManager.WorldMousePosition, selectedItem))
                {
                    playerInventory.RemoveItemAmount(new Item(selectedItem, 1), _mainModel.GetModelManager.SelectedInventorySlot, 0);
                }
            }
        }
    }
}

using ITProject.Logic;
using ITProject.Model;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using SharpFont.PostScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ITProject.Logic.GameExtentions;
using static ITProject.Logic.MainLogic;

namespace ITProject.View
{
    class MainView : GameWindow
    {

        private InGameView _inGameView;
        private MainMenuView _mainMenuView;
        private MainLogic _logic;

        private MainModel _mainModel;
        private MainMenuModel _menuModel;

        public WindowPositions WindowPositions;

        public MainView(int width, int height, GraphicsMode graphicsMode, string title, MainLogic logic, MainModel mainModel, MainMenuModel mainMenuModel) : base(width, height, graphicsMode, title)
        {
            _logic = logic;
            _mainModel = mainModel;
            _menuModel = mainMenuModel;

            _inGameView = new InGameView(logic, mainModel, this);
            _mainMenuView = new MainMenuView(logic, this, mainMenuModel);
        }

        protected override void OnLoad(EventArgs e)
        {
            //_inGameView.OnLoad();

            /*
            switch (_logic.State)
            {
                case GameState.InGame:
                    _inGameView.OnLoad();
                    break;
                case GameState.Menu:
                    _mainMenuView.OnLoad();
                    break;
            }
            */

            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            WindowPositions = UpdateWindowPositions();

            if (_logic.GameStateChanged)
            {
                _logic.GameStateChanged = false;

                //Unload current State
                switch (_logic.LastState)
                {
                    case GameState.InGame:
                        _inGameView.OnUnload();
                        break;
                    case GameState.Menu:
                        _mainMenuView.OnUnload();
                        break;
                    default:

                        break;
                }

                //Load next State
                switch (_logic.State)
                {
                    case GameState.InGame:
                        _inGameView.Init();
                        _inGameView.OnLoad();
                        break;
                    case GameState.Menu:
                        _mainMenuView.OnLoad();
                        break;
                }
            }

            switch (_logic.State)
            {
                case GameState.InGame:
                    _inGameView.OnUpdateFrame(e);
                    break;
                case GameState.Menu:
                    _mainMenuView.OnUpdateFrame(e);
                    break;
            }
            
            Title = $"{Math.Round(UpdateFrequency, 2)} fps";
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            switch (_logic.State)
            {
                case GameState.InGame:
                    _inGameView.OnRenderFrame(e);
                    break;
                case GameState.Menu:
                    _mainMenuView.OnRenderFrame(e);
                    break;
            }

            base.OnRenderFrame(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            switch (_logic.State)
            {
                case GameState.InGame:
                    _inGameView.OnClosed();
                    break;
                case GameState.Menu:
                    _mainMenuView.OnClose();
                    break;
            }

            base.OnClosed(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            switch (_logic.State)
            {
                case GameState.InGame:
                    _inGameView.OnUnload();
                    break;
                case GameState.Menu:
                    _mainMenuView.OnUnload();
                    break;
            }

            
            base.OnUnload(e);
        }
        protected override void OnResize(EventArgs e)
        {
            switch (_logic.State)
            {
                case GameState.InGame:
                    _inGameView.OnResize();
                    break;
                case GameState.Menu:
                    _mainMenuView.OnResize();
                    break;
            }
            
            base.OnResize(e);
        }

        private WindowPositions UpdateWindowPositions()
        {
            float cursorPosX = (float)PointToClient(Control.MousePosition).X;
            float cursorPosY = (float)PointToClient(Control.MousePosition).Y;

            WindowPositions windowPositions = new WindowPositions(Width, Height, X, Y, WindowState, Focused, new System.Numerics.Vector2(cursorPosX, cursorPosY));
            return windowPositions;
        }
    }
}

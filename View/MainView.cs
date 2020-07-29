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

        public MainView(int width, int height, GraphicsMode graphicsMode, string title, MainLogic logic, MainModel mainModel) : base(width, height, graphicsMode, title)
        {
            _logic = logic;
            _inGameView = new InGameView(width, height, logic, mainModel, this);
        }

        protected override void OnLoad(EventArgs e)
        {
            _inGameView.OnLoad();
            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _inGameView.OnUpdateFrame(e);
            Title = $"{Math.Round(UpdateFrequency, 2)} fps";
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _inGameView.OnRenderFrame(e);
            base.OnRenderFrame(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _logic.CloseGame();
            base.OnClosed(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            _inGameView.OnUnload();
            base.OnUnload(e);
        }
        protected override void OnResize(EventArgs e)
        {
            _inGameView.OnResize();
            base.OnResize(e);
        }
    }
}

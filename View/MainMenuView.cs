using ITProject.Logic;
using ITProject.Model;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ITProject.Logic.GameExtentions;

namespace ITProject.View
{
    class MainMenuView
    {
        private MainLogic _logic;
        private MainMenuModel _menuModel;
        private MainView _mainView;

        private QFont _font;
        private QFontDrawing _drawing;

        private MenuTextures _menuTextures;

        private Shader _shader;

        private uint _buttonsVAO, _buttonsVBO;

        private Vector2 _buttonTexCoordMin = new Vector2(0f, 0f);
        private Vector2 _buttonTexCoordMax = new Vector2(1f, 1f);

        public MainMenuView(MainLogic mainLogic, MainView mainView, MainMenuModel menuModel)
        {
            _mainView = mainView;
            _logic = mainLogic;
            _menuModel = menuModel;
        }

        public void OnLoad()
        {
            GL.GenVertexArrays(1, out _buttonsVAO);
            GL.GenBuffers(1, out _buttonsVBO);

            _menuTextures = new MenuTextures();

            while (_font == null)
            {
                try
                {
                    _font = new QFont("fonts/Depredationpixie.ttf", 15, new QuickFont.Configuration.QFontBuilderConfiguration(true));
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Fehler beim QFont Laden");
                }
            }

            _drawing = new QFontDrawing();
            _shader = new Shader("Shader/shader.vert", "Shader/shader.frag");
        }

        public void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color4.Cyan);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(_mainView.ClientRectangle.X, _mainView.ClientRectangle.Y, _mainView.ClientRectangle.Width, _mainView.ClientRectangle.Height);

            Draw();

            _mainView.SwapBuffers();
        }

        public void OnUpdateFrame(FrameEventArgs e)
        {
            _logic.Update(Keyboard.GetState(), Mouse.GetState(), _mainView.WindowPositions, e.Time, null);
        }

        public void OnUnload()
        {
            _menuTextures.UnloadTextures();

            GL.DeleteVertexArray(_buttonsVAO);
            GL.DeleteBuffer(_buttonsVBO);

            _drawing.Dispose();
            _font.Dispose();
        }

        public void OnClose()
        {
            
        }

        public void OnResize()
        {

        }

        private void Draw()
        {
            Matrix4 projection = Matrix4.CreateOrthographic(_mainView.ClientRectangle.Width, _mainView.ClientRectangle.Height, -1.0f, 1.0f);
            Matrix4 transformation = Matrix4.Identity * projection;
            SetMatrix(_shader, transformation, Vector4.Zero);
            _drawing.ProjectionMatrix = projection;

            switch (_menuModel.ScreenState)
            {
                case MainMenuModel.Screen.MainMenuStart:
                    DrawStartScreen();
                    break;
                case MainMenuModel.Screen.WorldSelect:

                    break;
                case MainMenuModel.Screen.PlayerSelect:

                    break;
            }
        }

        private void DrawStartScreen()
        {
            Vector2 selectWorldPos = new Vector2(0f, 200f);
            Vector2 endGamePos = new Vector2(0f, -200f);
            Vector2 buttonSize = new Vector2(400, 150);
            int count = 0;

            float[,] vertices = new float[2 * 4, 4];

            float[,] vert = GetVertices4x4(selectWorldPos, buttonSize, _buttonTexCoordMin, _buttonTexCoordMax, true);

            for (int ic = 0; ic < 4; ic++)
            {
                for (int ia = 0; ia < 4; ia++)
                {
                    vertices[count, ia] = vert[ic, ia];
                }
                count++;
            }

            vert = GetVertices4x4(endGamePos, buttonSize, _buttonTexCoordMin, _buttonTexCoordMax, true);

            for (int ic = 0; ic < 4; ic++)
            {
                for (int ia = 0; ia < 4; ia++)
                {
                    vertices[count, ia] = vert[ic, ia];
                }
                count++;
            }

            DrawElements(_buttonsVAO, _buttonsVBO, sizeof(float) * vertices.Length, vertices, 2 * 4, _menuTextures.Button);

            List<ViewButtonPositions> buttons = new List<ViewButtonPositions>();
            buttons.Add(new ViewButtonPositions(selectWorldPos, buttonSize, "Select World"));
            buttons.Add(new ViewButtonPositions(endGamePos, buttonSize, "Close Game"));
            _menuModel.ButtonPositions = buttons;
            DrawText(buttons);
        }

        private void DrawWorldSelectorScreen()
        {

        }

        private void DrawCreateWorldScreen()
        {

        }

        private void DrawPlayerSelectorScreen()
        {

        }

        private void DrawCreatePlayerScreen()
        {

        }

        private void DrawText(List<ViewButtonPositions> buttons)
        {
            _drawing.DrawingPimitiveses.Clear();

            var textOpts = new QFontRenderOptions()
            {
                Colour = Color.FromArgb(new Color4(0.0f, 1.0f, 1.0f, 1.0f).ToArgb()),
                DropShadowActive = true
            };

            foreach(ViewButtonPositions but in buttons)
            {
                _drawing.Print(_font, but.ButtonInput, new Vector3(but.Position.X, but.Position.Y, 0), QFontAlignment.Centre, textOpts);
            }

            _drawing.RefreshBuffers();
            _drawing.Draw();
        }

        private void SetMatrix(Shader shader, Matrix4 transformation, Vector4 translation)
        {
            GL.UseProgram(0);
            shader.Use();
            shader.SetMatrix4("transform", transformation);
            shader.SetVector4("translation", translation);
            shader.SetVector4("blockColor", new Vector4(1f, 1f, 1f, 1f));
        }

        private float[,] GetVertices4x4(Vector2 position, Vector2 size, Vector2 texCoordMin, Vector2 texCoordMax, bool centered)
        {
            if (centered)
            {
                return new float[4, 4]
                {
                    { position.X - size.X / 2, position.Y - size.Y / 2,   texCoordMin.X, texCoordMax.Y },
                    { position.X + size.X / 2, position.Y - size.Y / 2,   texCoordMax.X, texCoordMax.Y },
                    { position.X + size.X / 2, position.Y + size.Y / 2,   texCoordMax.X, texCoordMin.Y },
                    { position.X - size.X / 2, position.Y + size.Y / 2,   texCoordMin.X, texCoordMin.Y }
                };
            }
            else
            {
                return new float[4, 4]
                {
                    { position.X,     position.Y,                 texCoordMin.X, texCoordMax.Y },
                    { position.X + size.X, position.Y,            texCoordMax.X, texCoordMax.Y },
                    { position.X + size.X, position.Y + size.Y,   texCoordMax.X, texCoordMin.Y },
                    { position.X,     position.Y + size.Y,        texCoordMin.X, texCoordMin.Y }
                };
            }

        }

        private void DrawElements(uint vao, uint vbo, int bufferSize, float[,] vertices, int verticesToDraw, uint gameTexture)
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, bufferSize * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gameTexture);

            GL.DrawArrays(PrimitiveType.QuadsExt, 0, verticesToDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}

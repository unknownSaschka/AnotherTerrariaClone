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
using static ITProject.Model.World;

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
        private uint _backgroundVAO, _backgroundVBO;

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

            //Background
            GL.GenVertexArrays(1, out _backgroundVAO);
            GL.GenBuffers(1, out _backgroundVBO);

            GL.BindVertexArray(_backgroundVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _backgroundVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

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

            GL.DeleteVertexArray(_backgroundVAO);
            GL.DeleteBuffer(_backgroundVBO);

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

            DrawBackground();

            switch (_menuModel.ScreenState)
            {
                case MainMenuModel.Screen.MainMenuStart:
                    DrawStartScreen();
                    break;
                case MainMenuModel.Screen.WorldSelect:
                    DrawWorldSelectorScreen();
                    break;
                case MainMenuModel.Screen.PlayerSelect:
                    DrawPlayerSelectorScreen();
                    break;
            }
        }

        private void DrawStartScreen()
        {
            Vector2 selectWorldPos = new Vector2(0f, 0f);
            Vector2 endGamePos = new Vector2(0f, -200f);
            Vector2 buttonSize = new Vector2(500, 100);
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
            buttons.Add(new ViewButtonPositions(selectWorldPos, buttonSize, "Select World", ButtonType.ToWorldList, -1));
            buttons.Add(new ViewButtonPositions(endGamePos, buttonSize, "Close Game", ButtonType.CloseGame, -1));
            _menuModel.ButtonPositions = buttons;
            DrawText(buttons);
        }

        private void DrawWorldSelectorScreen()
        {
            Vector2 firstWorldButtonPos = new Vector2(0f, (_mainView.Height / 2) - 100f);
            Vector2 backButtonPos = new Vector2(-(_mainView.Width / 2) + 100f, -(_mainView.Height/2) + 50f);
            Vector2 buttonSize = new Vector2(400, 50);
            Vector2 backButtonSize = new Vector2(200, 100);

            float[,] vertices = new float[11 * 4, 4];

            WorldSaveInfo[] worldSaves = _menuModel.AvailableWorldSaves;
            List<ViewButtonPositions> buttons = new List<ViewButtonPositions>();
            buttons.Add(new ViewButtonPositions(new Vector2(firstWorldButtonPos.X, firstWorldButtonPos.Y + 50f), buttonSize, "Select World", ButtonType.None, -1));

            float steps = 60f;
            int count = 0;
            float[,] vert;

            for (int i = 0; i < 10; i++)
            {
                vert = GetVertices4x4(firstWorldButtonPos, buttonSize, _buttonTexCoordMin, _buttonTexCoordMax, true);
                for (int ic = 0; ic < 4; ic++)
                {
                    for (int ia = 0; ia < 4; ia++)
                    {
                        vertices[count, ia] = vert[ic, ia];
                    }
                    count++;
                }

                if (worldSaves[i] != null)
                {
                    buttons.Add(new ViewButtonPositions(new Vector2(firstWorldButtonPos.X, firstWorldButtonPos.Y), buttonSize, $"Slot {worldSaves[i].SaveSlot}", ButtonType.World, worldSaves[i].SaveSlot));
                }
                else
                {
                    buttons.Add(new ViewButtonPositions(new Vector2(firstWorldButtonPos.X, firstWorldButtonPos.Y), buttonSize, "Empty", ButtonType.World, i));
                }

                firstWorldButtonPos.Y -= steps;
            }

            vert = GetVertices4x4(backButtonPos, backButtonSize, _buttonTexCoordMin, _buttonTexCoordMax, true);
            for (int ic = 0; ic < 4; ic++)
            {
                for (int ia = 0; ia < 4; ia++)
                {
                    vertices[count, ia] = vert[ic, ia];
                }
                count++;
            }

            buttons.Add(new ViewButtonPositions(backButtonPos, backButtonSize, "Zurück", ButtonType.Back, -1));
            _menuModel.ButtonPositions = buttons;

            DrawElements(_buttonsVAO, _buttonsVBO, sizeof(float) * vertices.Length, vertices, 11 * 4, _menuTextures.Button);
            DrawText(buttons);
        }

        private void DrawPlayerSelectorScreen()
        {
            Vector2 firstPlayerButtonPos = new Vector2(0f, (_mainView.Height / 2) - 100f);
            Vector2 backButtonPos = new Vector2(-(_mainView.Width / 2) + 100f, -(_mainView.Height / 2) + 50f);
            Vector2 buttonSize = new Vector2(400, 50);
            Vector2 backButtonSize = new Vector2(200, 100);

            float[,] vertices = new float[11 * 4, 4];

            PlayerSaveInfo[] playerSaves = _menuModel.AvailablePlayerSaves;
            List<ViewButtonPositions> buttons = new List<ViewButtonPositions>();
            buttons.Add(new ViewButtonPositions(new Vector2(firstPlayerButtonPos.X, firstPlayerButtonPos.Y + 50f), buttonSize, "Select Player", ButtonType.None, -1));

            float steps = 60f;
            int count = 0;
            float[,] vert;

            for (int i = 0; i < 10; i++)
            {
                vert = GetVertices4x4(firstPlayerButtonPos, buttonSize, _buttonTexCoordMin, _buttonTexCoordMax, true);
                for (int ic = 0; ic < 4; ic++)
                {
                    for (int ia = 0; ia < 4; ia++)
                    {
                        vertices[count, ia] = vert[ic, ia];
                    }
                    count++;
                }

                if (playerSaves[i] != null)
                {
                    buttons.Add(new ViewButtonPositions(new Vector2(firstPlayerButtonPos.X, firstPlayerButtonPos.Y), buttonSize, $"Slot {playerSaves[i].SaveSlot}", ButtonType.Player, playerSaves[i].SaveSlot));
                }
                else
                {
                    buttons.Add(new ViewButtonPositions(new Vector2(firstPlayerButtonPos.X, firstPlayerButtonPos.Y), buttonSize, "Empty", ButtonType.Player, i));
                }

                firstPlayerButtonPos.Y -= steps;
            }

            vert = GetVertices4x4(backButtonPos, backButtonSize, _buttonTexCoordMin, _buttonTexCoordMax, true);
            for (int ic = 0; ic < 4; ic++)
            {
                for (int ia = 0; ia < 4; ia++)
                {
                    vertices[count, ia] = vert[ic, ia];
                }
                count++;
            }

            buttons.Add(new ViewButtonPositions(backButtonPos, backButtonSize, "Zurück", ButtonType.Back, -1));
            _menuModel.ButtonPositions = buttons;

            DrawElements(_buttonsVAO, _buttonsVBO, sizeof(float) * vertices.Length, vertices, 11 * 4, _menuTextures.Button);
            DrawText(buttons);
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

        private void DrawBackground()
        {
            float backgroundZoom = 1f;
            Matrix4 transform = Matrix4.CreateOrthographic(_mainView.Width * backgroundZoom, _mainView.Height * backgroundZoom, -1, 1);
            _shader.SetMatrix4("transform", transform);

            float ratio = _mainView.Width / (float)_mainView.Height;
            Vector2 min = new Vector2(-_mainView.Width / 2, -_mainView.Height / 2);
            Vector2 max = new Vector2(_mainView.Width / 2, _mainView.Height / 2);

            float[,] vertices = GetVerices4x4MinMax(min, max, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _menuTextures.MenuBackground);

            GL.BindVertexArray(_backgroundVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _backgroundVBO);

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);
        }

        private float[,] GetVerices4x4MinMax(Vector2 min, Vector2 max, Vector2 texCoordMin, Vector2 texCoordMax)
        {
            return GetVertices4x4(min, max - min, texCoordMin, texCoordMax, false);
        }
    }
}

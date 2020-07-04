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
using static ITProject.Logic.MainLogic;

namespace ITProject.View
{
    class MainView : GameWindow
    {
        private GameTextures _gameTextures;
        private MainLogic _logic;
        private MainModel _mainModel;
        private ModelManager _modelManager;

        private Vector2 _oldPlayerPosition;
        private float _zoom;

        private QFont _font;
        private QFontDrawing _drawing;

        private uint _blockVAO, _blockVBO;
        private uint _mouseVAO, _mouseVBO;
        private uint _playerVAO, _playerVBO;

        private Shader _shader;
        private float _passedTime;

        private int _blocksToProcess = 0;

        public MainView(int width, int height, GraphicsMode graphicsMode, string title, MainLogic logic, MainModel mainModel) : base(width, height, graphicsMode, title)
        {
            _gameTextures = new GameTextures();
            _logic = logic;
            _mainModel = mainModel;
            _modelManager = mainModel.GetModelManager;
            _oldPlayerPosition = new Vector2(-_modelManager.Player.Position.X, -_modelManager.Player.Position.Y);
            _zoom = _modelManager.Zoom;
        }

        protected override void OnLoad(EventArgs e)
        {
            InitQFont();
            InitVertexBuffer();
            InitShaders();
            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _zoom = GameExtentions.Lerp(_zoom, _modelManager.Zoom, 3.0f * (float)e.Time);
            //_renderDistance = new Vector2(_modelManager.RenderDistance.X, _modelManager.RenderDistance.Y);

            _logic.Update(Keyboard.GetState(), Mouse.GetCursorState(), UpdateWindowPositions(), e.Time);
            Title = $"{Math.Round(UpdateFrequency, 2)} fps";

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _passedTime += (float)e.Time;
            Draw();
            base.OnRenderFrame(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _logic.CloseGame();
            base.OnClosed(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_blockVBO);
            GL.DeleteVertexArray(_blockVAO);

            base.OnUnload(e);
        }

        private void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //DrawWorld();
            DrawWorldV2();

            DrawGUI();

            DrawMousePointer();

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                DrawFont();
            }
            

            SwapBuffers();
        }

        private void DrawWorld()
        {
            Player player = _modelManager.Player;
            World world = _modelManager.World;
            System.Numerics.Vector2 worldSize = _modelManager.World.WorldSize;
            float zoomFactor = 0.002f;

            Vector2 smoothCameraPos = Vector2.Lerp(_oldPlayerPosition, new Vector2(-player.Position.X, -player.Position.Y), 0.3f);
            _oldPlayerPosition = smoothCameraPos;

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            Matrix4 projection = Matrix4.CreateOrthographic(ClientRectangle.Width * _zoom * zoomFactor, ClientRectangle.Height * _zoom * zoomFactor, -1.0f, 1.0f);
            Vector4 translation = new Vector4(smoothCameraPos.X, smoothCameraPos.Y, 0f, 0f);

            GL.UseProgram(0);
            _shader.Use();

            
            Matrix4 transformation = Matrix4.Identity * projection;
            _shader.SetMatrix4("transform", transformation);
            _shader.SetVector4("translation", translation);

            //Draw Player
            DrawPlayer(player);

            //DrawWorld

            int renderDistance = 30;
            //Vector2 minBoundary = new Vector2(player.Position.X - renderDistance, player.Position.Y - renderDistance);
            //Vector2 maxBoundary = new Vector2(player.Position.X + renderDistance, player.Position.Y + renderDistance);
            Vector2 minBoundary = new Vector2();
            Vector2 maxBoundary = new Vector2();
            CalculateViewBorders(player.Position, ref minBoundary, ref maxBoundary);
            float w = 1f, h = 1f;   //Block höhe und breite

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Items);

            GL.BindVertexArray(_blockVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _blockVBO);

            for (int iy = (int)minBoundary.Y; iy < maxBoundary.Y + 1; iy++)
            {
                for (int ix = (int)minBoundary.X; ix < maxBoundary.X + 1; ix++)
                {
                    //Alles, was innerhalb der RenderDistance abläuft
                    if (!GameExtentions.CheckIfInBound(ix, iy, worldSize)) continue;
                    ushort blockID = world.GetWorld[ix, iy];
                    if (blockID == 0) continue;

                    Vector2 min = new Vector2();
                    Vector2 max = new Vector2();

                    GetTextureCoord(blockID, new Vector2(8, 8), out min, out max);
                    
                    float[,] vertices = new float[4, 4]
                    {
                        { ix,     iy,       min.X, max.Y },
                        { ix + w, iy,       max.X, max.Y },
                        { ix + w, iy + h,   max.X, min.Y },
                        { ix,     iy + h,   min.X, min.Y }
                        
                    };
                    
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
                    
                    GL.DrawArrays(PrimitiveType.Quads, 0, 4);
                }
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
        }

        private void DrawWorldV2()
        {
            Player player = _modelManager.Player;
            World world = _modelManager.World;
            System.Numerics.Vector2 worldSize = _modelManager.World.WorldSize;
            float zoomFactor = 0.002f;
            float w = 1f, h = 1f;   //Block höhe und breite

            Vector2 smoothCameraPos = Vector2.Lerp(_oldPlayerPosition, new Vector2(-player.Position.X, -player.Position.Y), 0.3f);
            _oldPlayerPosition = smoothCameraPos;

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            Matrix4 projection = Matrix4.CreateOrthographic(ClientRectangle.Width * _zoom * zoomFactor, ClientRectangle.Height * _zoom * zoomFactor, -1.0f, 1.0f);
            Vector4 translation = new Vector4(smoothCameraPos.X, smoothCameraPos.Y, 0f, 0f);

            GL.UseProgram(0);
            _shader.Use();


            Matrix4 transformation = Matrix4.Identity * projection;
            _shader.SetMatrix4("transform", transformation);
            _shader.SetVector4("translation", translation);

            //Draw Player
            DrawPlayer(player);

            //DrawWorld
            Vector2 minBoundary = new Vector2();
            Vector2 maxBoundary = new Vector2();

            CalculateViewBorders(player.Position, ref minBoundary, ref maxBoundary);
            ProcessBlockVertices(minBoundary, maxBoundary);

            float[,] vertices = new float[4 * _blocksToProcess, 4];
            int count = 0;

            int blocks = 0;

            for (int iy = (int)minBoundary.Y; iy < maxBoundary.Y + 1; iy++)
            {
                for (int ix = (int)minBoundary.X; ix < maxBoundary.X + 1; ix++)
                {
                    //Alles, was innerhalb der RenderDistance abläuft
                    if (!GameExtentions.CheckIfInBound(ix, iy, worldSize)) continue;
                    ushort blockID = world.GetWorld[ix, iy];
                    if (blockID == 0) continue;

                    Vector2 min = new Vector2();
                    Vector2 max = new Vector2();

                    GetTextureCoord(blockID, new Vector2(8, 8), out min, out max);

                    
                    float[,] vert = new float[4, 4]
                    {
                        { ix,     iy,       min.X, max.Y },
                        { ix + w, iy,       max.X, max.Y },
                        { ix + w, iy + h,   max.X, min.Y },
                        { ix,     iy + h,   min.X, min.Y }

                    };
                    
                    for(int ic = 0; ic < 4; ic++)
                    {
                        for(int ia = 0; ia < 4; ia++)
                        {
                            vertices[count, ia] = vert[ic, ia];
                        }
                        count++;
                    }

                    blocks++;
                }
            }

            AlterVertexBufferBlocks(vertices);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Items);
            GL.BindVertexArray(_blockVAO);

            GL.DrawArrays(PrimitiveType.QuadsExt, 0, blocks * 4);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);
        }

        private void DrawPlayer(Player player)
        {
            Vector2 min = new Vector2(player.Position.X - (player.Size.X / 2), player.Position.Y - (player.Size.Y / 2));
            Vector2 max = new Vector2(player.Position.X + (player.Size.X / 2), player.Position.Y + (player.Size.Y / 2));

            float[,] playerVertices = new float[4, 4]
            {
                        { min.X, min.Y,   0.0f, 1.0f },
                        { max.X, min.Y,   1.0f, 1.0f },
                        { max.X, max.Y,   1.0f, 0.0f },
                        { min.X, max.Y,   0.0f, 0.0f }
            };

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Debug2);

            GL.BindVertexArray(_blockVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _blockVBO);

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * playerVertices.Length, playerVertices);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);
        }

        private void DrawGUI()
        {

        }

        private void DrawMousePointer()
        {
            float mouseSize = 0.2f;
            Vector2 min = new Vector2(_modelManager.WorldMousePosition.X - mouseSize, _modelManager.WorldMousePosition.Y - mouseSize);
            Vector2 max = new Vector2(_modelManager.WorldMousePosition.X + mouseSize, _modelManager.WorldMousePosition.Y + mouseSize);

            float[,] playerVertices = new float[4, 4]
            {
                        { min.X, min.Y,   0.0f, 1.0f },
                        { max.X, min.Y,   1.0f, 1.0f },
                        { max.X, max.Y,   1.0f, 0.0f },
                        { min.X, max.Y,   0.0f, 0.0f }
                        
            };

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Debug2);

            GL.BindVertexArray(_blockVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _blockVBO);

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * playerVertices.Length, playerVertices);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);
        }

        private void DrawFont()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            Matrix4 projectionMatrix = Matrix4.CreateOrthographic(ClientRectangle.Width, ClientRectangle.Height, -1.0f, 1.0f);
            _drawing.ProjectionMatrix = projectionMatrix;

            _drawing.DrawingPimitiveses.Clear();

            var textOpts = new QFontRenderOptions()
            {
                Colour = Color.FromArgb(new Color4(0.0f, 1.0f, 1.0f, 1.0f).ToArgb()),
                DropShadowActive = true
            };
            _drawing.Print(_font, "Test", new Vector3(0.0f, 0.0f, 0.0f), QFontAlignment.Left, textOpts);

            _drawing.RefreshBuffers();
            _drawing.Draw();
        }

        private void InitQFont()
        {
            _font = new QFont("fonts/Depredationpixie.ttf", 72, new QuickFont.Configuration.QFontBuilderConfiguration(true));
            _drawing = new QFontDrawing();
        }

        private void InitVertexBuffer()
        {
            InitVertexBufferMouse();
            InitVertexBufferPlayer();
            InitVertexBufferBlocks();
        }

        private void ProcessBlockVertices(Vector2 min, Vector2 max)
        {
            int blocks =  (((int)max.X + 1) - (int)min.X) * (((int)max.Y + 1) - (int)min.Y);

            _blocksToProcess = blocks;
        }

        private void AlterVertexBufferBlocks(float [,] vertices)
        {
            GL.BindVertexArray(_blockVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _blockVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void InitVertexBufferBlocks()
        {
            //World
            GL.GenVertexArrays(1, out _blockVAO);
            GL.GenBuffers(1, out _blockVBO);
        }

        private void InitVertexBufferPlayer()
        {
            //Player
            GL.GenVertexArrays(1, out _playerVAO);
            GL.GenBuffers(1, out _playerVBO);

            GL.BindVertexArray(_playerVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _playerVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void InitVertexBufferMouse()
        {
            //Mouse Cursor
            GL.GenVertexArrays(1, out _mouseVAO);
            GL.GenBuffers(1, out _mouseVBO);

            GL.BindVertexArray(_mouseVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _mouseVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void InitShaders()
        {
            _shader = new Shader("Shader/shader.vert", "Shader/shader.frag");
        }

        private WindowPositions UpdateWindowPositions()
        {
            float cursorPosX = (float)PointToClient(Control.MousePosition).X;
            float cursorPosY = (float)PointToClient(Control.MousePosition).Y;

            WindowPositions windowPositions = new WindowPositions();
            windowPositions.Height = Height;
            windowPositions.Width = Width;
            windowPositions.X = X;
            windowPositions.Y = Y;
            windowPositions.WindowState = WindowState;
            windowPositions.Focused = Focused;
            windowPositions.WindowMousePosition = new System.Numerics.Vector2(cursorPosX, cursorPosY);
            windowPositions.Zoom = _zoom;

            return windowPositions;
        }

        private void CalculateViewBorders(System.Numerics.Vector2 playerPosition, ref Vector2 mins, ref Vector2 maxs)
        {
            WindowPositions winPos = UpdateWindowPositions();

            System.Numerics.Vector2 upperLeft = _logic.CalculateViewToWorldPosition(new System.Numerics.Vector2(0f, 0f), playerPosition, winPos, false);
            System.Numerics.Vector2 lowerRight = _logic.CalculateViewToWorldPosition(new System.Numerics.Vector2(winPos.Width, winPos.Height), playerPosition, winPos, false);

            mins.X = upperLeft.X;
            mins.Y = lowerRight.Y;
            maxs.X = lowerRight.X;
            maxs.Y = upperLeft.Y;
        }

        private void GetTextureCoord(int position, Vector2 gridSize, out Vector2 minTexCoord, out Vector2 maxTexCoord)
        {
            float offset = 0.0042f;
            Vector2 tileSize = new Vector2();
            tileSize.X = 1 / gridSize.X;
            tileSize.Y = 1 / gridSize.Y;
            int x = (int) (position % gridSize.X);
            int y = (int) (position / gridSize.X);

            minTexCoord.X = (x * tileSize.X) + offset;
            maxTexCoord.X = ((x + 1) * tileSize.X) - offset;
            minTexCoord.Y = (y * tileSize.Y) + offset;
            maxTexCoord.Y = ((y + 1) * tileSize.Y) - offset;
        }
    }
}

using ITProject.Logic;
using ITProject.Model;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static ITProject.Logic.MainLogic;
using static ITProject.View.Fonts;

namespace ITProject.View
{
    struct Vertex
    {
        public Vector2 Position;
        public Vector2 TextureCoordinate;
        public Vector4 color;

        public Color Color
        {
            get
            {
                return Color.FromArgb((int)(255 * color.W), (int)(255 * color.X), (int)(255 * color.Y), (int)(255 * color.Z));
            }
            set
            {
                this.color = new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
            }
        }
        public static int SizeInBytes
        {
            get { return Vector2.SizeInBytes * 2 + Vector4.SizeInBytes; }
        }
        public Vertex(Vector2 position, Vector2 texCoords)
        {
            Position = position;
            TextureCoordinate = texCoords;
            color = new Vector4(1f, 1f, 1f, 1f);
        }
    }

    class MainViewOld : GameWindow
    {
        private int _width, _height;
        private MainLogic _logic;
        private MainModel _mainModel;
        private ModelManager _modelManager;
        private float zoom;
        private float _blockSize = 1f;
        private Vector2 _renderDistance;
        private Vector2 oldPlayerPostion;
        private GameTextures _textures;

        private QFont _font;
        private QFontDrawing _drawing;

        //private Shader _shader;
        //private Shader _fontShader;

        public MainViewOld(int width, int height, GraphicsMode graphicsMode, string title, MainLogic logic, MainModel mainModel) : base(width, height, graphicsMode, title)
        {
            _textures = new GameTextures();
            _width = width;
            _height = height;
            _logic = logic;
            _mainModel = mainModel;
            _modelManager = mainModel.GetModelManager;
            oldPlayerPostion = new Vector2(-_modelManager.Player.Position.X, -_modelManager.Player.Position.Y);
            zoom = _modelManager.Zoom;

            //InitVertexBuffer();
        }

        protected override void OnLoad(EventArgs e)
        {
            //InitVertexBuffer();
            //InitFont();

            _font = new QFont("fonts/Depredationpixie.ttf", 72, new QuickFont.Configuration.QFontBuilderConfiguration(true));
            _drawing = new QFontDrawing();
            //InitQFont();

            InitShader();
            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            zoom = GameExtentions.Lerp(zoom, _modelManager.Zoom, 3.0f * (float)e.Time);
            _renderDistance = new Vector2(_modelManager.RenderDistance.X, _modelManager.RenderDistance.Y);
            
            _logic.Update(Keyboard.GetState(), Mouse.GetCursorState(), UpdateWindowPositions(), e.Time);
            Title = $"{Math.Round(UpdateFrequency, 2)} fps";
            
            //Draw();

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Draw();

            base.OnRenderFrame(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
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

            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            _width = Width;
            _height = Height;
            base.OnResize(e);
        }

        private void InitQFont()
        {
            _drawing.DrawingPimitiveses.Clear();
            _drawing.Print(_font, "Test", new Vector3(0f, 0f, 0f), QFontAlignment.Left);

            _drawing.RefreshBuffers();
        }

        private void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);                              //Cleared den vorherigen Screen
            GL.Viewport(0, 0, Width, Height);
            
            ApplyCamera();

            DrawWorld();

            //DrawGUI();

            SwapBuffers();           //BufferSwapping, sonst wird aktuelles Bild nicht gerendert
        }

        private void ApplyCamera()
        {
            GL.Viewport(0, 0, _width, _height);                                     //Viewport setzen, sodass wir eine "Camera" haben. Einfacher zu Handhaben für Scaling, Aspect Ratio, etc.
            GL.MatrixMode(MatrixMode.Projection);                                   //Umstellen auf Projektionsmodus für Viewport
            GL.LoadIdentity();                                                      //Matrix laden
            double ratio = (double)_width / _height;                                //Aspect Ratio ausrechnen
            GL.Ortho(-ratio * zoom, ratio * zoom, -1 * zoom, 1 * zoom, -1, 2);      //Ratio setzen
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void InitShader()
        {
            //_shader = new Shader("Shader/shader.vert", "Shader/shader.frag");
            //_fontShader = new Shader("Shader/fontShader.vert", "Shader/fontShader.frag");
        }

        private void DrawGUI()
        {
            
            GL.UseProgram(0);

            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            double ratio = (double)_width / _height;
            GL.Ortho(-ratio, ratio, -1, 1, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            

            GL.Color3(1.0f, 1.0f, 1.0f);

            //DrawItemBar();

            if (_modelManager.InventoryOpen)
            {
                DrawInventory();
            }

            //DrawTexturedRect(new Box2D(new OpenTK.Vector2(0f, 0.9f), new OpenTK.Vector2(1.2f, 0.18f), Box2D.BoxType.MiddlePos), _textures.InventoryBar);

            GL.Disable(EnableCap.Blend);

            //_drawing.ProjectionMatrix = Matrix4.Identity;
            //_drawing.Draw();
        }

        private void DrawMousePointer()
        {
            GL.Color4(0.5f, 0.5f, 0.0f, 0.8f);
            DrawRectangle(_modelManager.WorldMousePosition.X - 0.2f, _modelManager.WorldMousePosition.X + 0.2f, _modelManager.WorldMousePosition.Y - 0.2f, _modelManager.WorldMousePosition.Y + 0.2f);
        }

        private void DrawWorld()
        {
            World world = _modelManager.World;
            Player player = _modelManager.Player;

            GL.LoadIdentity();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            //Bewegt die darunterliegende Welt nach der Spielerposition. Wegen Welt bewegung, negativ transformieren
            Vector2 smoothCameraPos = Vector2.Lerp(oldPlayerPostion, new Vector2(-player.Position.X, -player.Position.Y), 0.3f);
            GL.Translate(new Vector3(smoothCameraPos.X, smoothCameraPos.Y, 0f));
            oldPlayerPostion = smoothCameraPos;

            Vector2 minBoundary = new Vector2();
            Vector2 maxBoundary = new Vector2();
            CalculateViewBorders(player.Position, ref minBoundary, ref maxBoundary);

            System.Numerics.Vector2 worldSize = _modelManager.World.WorldSize;

            DrawPlayer(player.Position.X, player.Position.Y, player.Size.X, player.Size.Y);

            for (int iy = (int)minBoundary.Y; iy < maxBoundary.Y + 1; iy++)
            {
                for (int ix = (int)minBoundary.X; ix < maxBoundary.X + 1; ix++)
                {
                    //Alles, was innerhalb der RenderDistance abläuft
                    if (!GameExtentions.CheckIfInBound(ix, iy, worldSize)) continue;
                    DrawBlock(ix, iy, world.GetWorld[ix, iy]);
                }
            }

            DrawMousePointer();

            GL.Disable(EnableCap.Blend);
        }

        private Vector4 GetBlockColor(int blockType)
        {
            Vector4 color;
            
            if(blockType == 0)
            {
                color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }
            else if (blockType == 1)
            {
                color = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
            }
            else if (blockType == 2)
            {
                color = new Vector4(0.5f, 0.3f, 0.0f, 1.0f);
            }
            else if (blockType == 3)
            {
                color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            }
            else if (blockType == 8)
            {
                color = new Vector4(0.0f, 0.0f, 0.8f, 0.5f);
            }
            else
            {
                color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }

            return color;
        }

        private void DrawBlock(int posX, int posY, int blockType)
        {

            if (blockType == 1)
            {
                GL.Color3(0.4f, 0.4f, 0.4f);
            }
            else if (blockType == 2)
            {
                GL.Color3(0.5f, 0.3f, 0.0f);
            }
            else if(blockType == 3)
            {
                GL.Color3(0.0f, 1.0f, 0.0f);
            }
            else if(blockType == 8)
            {
                GL.Color4(0.0f, 0.0f, 0.8f, 0.5f);
            }
            else
            {
                GL.Color4(1.0f, 1.0f, 1.0f, 0.0f);
            }

            DrawRectangle(posX, posX + _blockSize, posY, posY + _blockSize);

            if (_mainModel.GetModelManager.ShowGrid)
            {
                DrawGrid(posX, posX + _blockSize, posY, posY + _blockSize);
            }

        }

        private void DrawItemBar()
        {

            //Draw Itembar
            //DrawTexturedRect(new Box2d(-0.65, 1.0, 0.65, 0.8), _textures.InventoryBar);
            DrawTexturedRect(new Box2D(new OpenTK.Vector2(0f, 0.9f), new OpenTK.Vector2(1.2f, 0.18f), Box2D.BoxType.MiddlePos), _textures.InventoryBar);

            

            //DrawItem
            OpenTK.Vector2 itemSize = new OpenTK.Vector2(0.09f, 0.09f);
            Inventory inv = _modelManager.Player.ItemInventory;
            DrawItem(new Box2D(new OpenTK.Vector2(-0.51f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(0, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(-0.40f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(1, 0)); 
            DrawItem(new Box2D(new OpenTK.Vector2(-0.285f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(2, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(-0.17f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(3, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(-0.055f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(4, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(0.055f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(5, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(0.17f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(6, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(0.285f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(7, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(0.40f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(8, 0));
            DrawItem(new Box2D(new OpenTK.Vector2(0.51f, 0.9f), itemSize, Box2D.BoxType.MiddlePos), inv.GetItemID(9, 0));
            

            //Draw Itembar Selector
            //GL.Color3(1.0f, 1.0f, 1.0f);

            
            switch (_modelManager.SelectedInventorySlot)
            {
                case 0:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(-0.51f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 1:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(-0.40f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 2:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(-0.285f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 3:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(-0.17f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 4:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(-0.055f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 5:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(0.055f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 6:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(0.17f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 7:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(0.285f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 8:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(0.40f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
                case 9:
                    DrawTexturedRect(new Box2D(new OpenTK.Vector2(0.51f, 0.9f), new OpenTK.Vector2(0.18f, 0.18f), Box2D.BoxType.MiddlePos), _textures.Itembar_Selector);
                    break;
            }
        }

        private void DrawInventory()
        {
            DrawTexturedRect(new Box2D(new OpenTK.Vector2(0f, 0f), new OpenTK.Vector2(1.4f, 1.4f), Box2D.BoxType.MiddlePos), _textures.Inventory);
        }

        private void DrawTexturedRect(Box2D box, uint textureID)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(box.MinX, box.MinY);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(box.MaxX, box.MinY);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(box.MaxX, box.MaxY);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(box.MinX, box.MaxY);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        private void DrawItem(Box2D box, ushort itemID)
        {
            if(itemID == 1)
            {
                GL.Color3(0.4f, 0.4f, 0.4f);
            }
            else if(itemID == 3)
            {
                GL.Color3(0.0f, 1.0f, 0.0f);
            }
            else
            {
                return;
            }

            DrawRectangle(box.MinX, box.MaxX, box.MinY, box.MaxY);
        }

        private void DrawRectangle(float minX, float maxX, float minY, float maxY)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(minX, minY);
            GL.Vertex2(minX, maxY);
            GL.Vertex2(maxX, maxY);
            GL.Vertex2(maxX, minY);
            GL.End();
        }

        private void DrawPlayer(float playerPosX, float playerPosY, float playerSizeX, float playerSizeY)
        {
            GL.Color3(0.0f, 1.0f, 0.0f);
            if (_modelManager.Player.Grounded) GL.Color3(0.0f, 0.0f, 1.0f);
            DrawRectangle(playerPosX - (playerSizeX / 2), playerPosX + (playerSizeX / 2), playerPosY - (playerSizeY / 2), playerPosY + (playerSizeY / 2));
            DrawPoint(playerPosX, playerPosY);
        }

        private void DrawGrid(float minX, float maxX, float minY, float maxY)
        {
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(minX, minY);
            GL.Vertex2(maxX, minY);
            GL.Vertex2(maxX, maxY);
            GL.Vertex2(minX, maxY);
            GL.End();
        }

        private void DrawPoint(float posX, float posY)
        {
            GL.Color3(1.0f, 0.0f, 1.0f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(posX, posY);
            GL.End();
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
            windowPositions.Zoom = zoom;

            return windowPositions;
        }

        private void CalculateViewBorders(System.Numerics.Vector2 playerPosition,  ref Vector2 mins,  ref Vector2 maxs)
        {
            /*
            WindowPositions winPos = UpdateWindowPositions();

            System.Numerics.Vector2 upperLeft= _logic.CalculateViewToWorldPosition(new System.Numerics.Vector2(0f, 0f), playerPosition, winPos);
            System.Numerics.Vector2 lowerRight = _logic.CalculateViewToWorldPosition(new System.Numerics.Vector2(winPos.Width, winPos.Height), playerPosition, winPos);

            mins.X = upperLeft.X;
            mins.Y = lowerRight.Y;
            maxs.X = lowerRight.X;
            maxs.Y = upperLeft.Y;
            */
        }
    }

    public class Box2D
    {
        public enum BoxType { MiddlePos, LowerLeftPos }

        public OpenTK.Vector2 Position;
        public OpenTK.Vector2 Size;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
        public BoxType Type;

        public Box2D(OpenTK.Vector2 position, OpenTK.Vector2 size, BoxType type)
        {
            if (type == BoxType.MiddlePos)
            {
                Position = position;
                Size = size;
                MinX = position.X - size.X / 2;
                MaxX = position.X + size.X / 2;
                MinY = position.Y - size.Y / 2;
                MaxY = position.Y + size.Y / 2;
                Type = type;
            }
            else if (type == BoxType.LowerLeftPos)
            {
                Position = position;
                Size = size;
                MinX = position.X;
                MaxX = position.X + size.X;
                MinY = position.Y;
                MaxY = position.Y + size.Y;
                Type = type;
            }

        }
    }
}

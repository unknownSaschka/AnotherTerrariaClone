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
        private GameTextures _gameTextures;
        private MainLogic _logic;
        private MainModel _mainModel;
        //private ModelManager _modelManager;

        private Vector2 _oldPlayerPosition;
        private float _zoom;

        private QFont _font;
        private QFontDrawing _drawing;

        private uint _blockVAO, _blockVBO;
        private uint _mouseVAO, _mouseVBO;
        private uint _playerVAO, _playerVBO;
        private uint _invBarVAO, _invBarVBO;
        private uint _inventoryVAO, _inventoryVBO;
        private uint _itemInvBarVAO, _itemInvBarVBO;
        private uint _invBarSelectorVAO, _invBarSelectorVBO;
        private uint _waterBlocksVAO, _waterBlocksVBO;
        private uint _invItemsPosVAO, _invItemsPosVBO;
        private uint _invHoldItemVAO, _invHoldItemVBO;
        private uint _background1VAO, _background1VBO;

        private Shader _shader;
        private float _passedTime;

        private int _blocksToProcess = 0;

        private struct ItemPositionAmount
        {

            public ItemPositionAmount(Vector2 position, int amount)
            {
                Position = position;
                Amount = amount;
            }

            public Vector2 Position { get; }
            public int Amount { get; }
        }

        public MainView(int width, int height, GraphicsMode graphicsMode, string title, MainLogic logic, MainModel mainModel) : base(width, height, graphicsMode, title)
        {
            _gameTextures = new GameTextures();
            _logic = logic;
            _mainModel = mainModel;
            _oldPlayerPosition = new Vector2(-_mainModel.GetModelManager.Player.Position.X, -_mainModel.GetModelManager.Player.Position.Y);
            _zoom = _mainModel.GetModelManager.Zoom;
        }

        protected override void OnLoad(EventArgs e)
        {
            InitQFont();
            InitVertexBuffer();
            InitVertexBufferWaterBlocks();
            InitVertexBufferInvBar();
            InitVertexBufferInventory();
            InitInventoryItemsPos();
            InitVertexBufferInvBarItems();
            InitVertexBufferInvBarSelector();
            InitVertexBufferInvHoldItem();
            InitVertexBufferBackground();
            InitShaders();
            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _zoom = GameExtentions.Lerp(_zoom, _mainModel.GetModelManager.Zoom, 3.0f * (float)e.Time);
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
        protected override void OnResize(EventArgs e)
        {
            AlterVertexBufferInvBar();

            base.OnResize(e);
        }

        private void SetIdentityMatrix()
        {
            Matrix4 projection = Matrix4.CreateOrthographic(ClientRectangle.Width, ClientRectangle.Height, -1.0f, 1.0f);
            Matrix4 transformation = Matrix4.Identity * projection;
            Vector4 translation = Vector4.Zero;

            GL.UseProgram(0);
            _shader.Use();
            _shader.SetMatrix4("transform", transformation);
            _shader.SetVector4("translation", translation);
            _shader.SetVector4("blockColor", new Vector4(1f, 1f, 1f, 1f));
        }

        private void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawBackground();

            //DrawWorld();
            DrawWorldV2();

            DrawMousePointer();

            DrawGUI();

            if (Keyboard.GetState().IsKeyDown(Key.T))
            {
                DrawFont();
            }

            SwapBuffers();
        }

        private void DrawWorldV2()
        {
            Player player = _mainModel.GetModelManager.Player;
            World world = _mainModel.GetModelManager.World;
            System.Numerics.Vector2 worldSize = _mainModel.GetModelManager.World.WorldSize;
            float zoomFactor = 0.002f;
            float w = 1.005f, h = 1.005f;   //Block höhe und breite

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
            _shader.SetVector4("blockColor", new Vector4(1f, 1f, 1f, 1f));

            //Draw Player
            DrawPlayer(player);

            //DrawWorld
            Vector2 minBoundary = new Vector2();
            Vector2 maxBoundary = new Vector2();

            List<Vector2> waterBlockList = new List<Vector2>();

            CalculateViewBorders(player.Position, ref minBoundary, ref maxBoundary);
            ProcessBlockVertices(minBoundary, maxBoundary);

            float[,] vertices = new float[4 * _blocksToProcess, 4];
            int count = 0;

            int blocks = 0;

            for (int iy = (int)minBoundary.Y; iy < (int)maxBoundary.Y + 1; iy++)
            {
                for (int ix = (int)minBoundary.X; ix < (int)maxBoundary.X + 1; ix++)
                {
                    //Alles, was innerhalb der RenderDistance abläuft
                    if (!GameExtentions.CheckIfInBound(ix, iy, worldSize)) continue;
                    ushort blockID = world.GetWorld[ix, iy];
                    if (blockID == 0) continue;

                    if(blockID == 8)
                    {
                        waterBlockList.Add(new Vector2(ix, iy));
                        continue;
                    }

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

            AlterVertexBufferBlocks(_blockVAO, _blockVBO, vertices);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Items);
            GL.BindVertexArray(_blockVAO);

            GL.DrawArrays(PrimitiveType.QuadsExt, 0, blocks * 4);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);


            float[,] waterBlocks = new float[waterBlockList.Count * 4, 4];
            Vector2 minWater;
            Vector2 maxWater;

            GetTextureCoord(8, new Vector2(8, 8), out minWater, out maxWater);

            count = 0;
            foreach(Vector2 pos in waterBlockList)
            {
                float[,] waterVerts = new float[4, 4]
                {
                    { pos.X,     pos.Y,       minWater.X, maxWater.Y },
                    { pos.X + w, pos.Y,       maxWater.X, maxWater.Y },
                    { pos.X + w, pos.Y + h,   maxWater.X, minWater.Y },
                    { pos.X,     pos.Y + h,   minWater.X, minWater.Y }
                };

                for (int ic = 0; ic < 4; ic++)
                {
                    for (int ia = 0; ia < 4; ia++)
                    {
                        waterBlocks[count, ia] = waterVerts[ic, ia];
                    }
                    count++;
                }
            }


            _shader.SetVector4("blockColor", new Vector4(1f, 1f, 1f, 0.5f));
            DrawElements(_waterBlocksVAO, _waterBlocksVBO, sizeof(float) * waterBlocks.Length, waterBlocks, waterBlocks.Length * 4, _gameTextures.Items);
        }

        private void DrawBackground()
        {
            SetIdentityMatrix();
            float backgroundZoom = 1f;
            //GL.Viewport(0, 0, Width, Height);
            Matrix4 transform = Matrix4.CreateOrthographic(Width * backgroundZoom, Height * backgroundZoom, -1, 1);
            _shader.SetMatrix4("transform", transform);
            //_shader.SetVector4("translation", new Vector4(-Width/2, -Height/2, 1f, 1f));

            float ratio = Width / (float)Height;
            Vector2 min = new Vector2(-Width / 2, -Height / 2);
            Vector2 max = new Vector2(Width / 2, Height / 2);

            float[,] vertices = new float[4, 4]
            {
                        { min.X, min.Y,   0.0f, 1.0f },
                        { max.X, min.Y,   1.0f, 1.0f },
                        { max.X, max.Y,   1.0f, 0.0f },
                        { min.X, max.Y,   0.0f, 0.0f }

            };

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Background1);

            GL.BindVertexArray(_mouseVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _mouseVBO);

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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
            SetIdentityMatrix();
            DrawItemBar();

            if (_mainModel.GetModelManager.InventoryOpen)
            {
                SetIdentityMatrix();
                DrawInventory();
            }
        }

        private void DrawItemBar()
        {
            //Draw Itembar Frame
            DrawElements(_invBarVAO, _invBarVBO, 4, _gameTextures.InventoryBar);

            //Draw itembar Items
            
            Item[] items = new Item[10];
            for(int i = 0; i < items.Length; i++)
            {
                items[i] = _mainModel.GetModelManager.Player.ItemInventory.GetItem(i, 0);
            }

            int itemLength = 0;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                if (items[i].ID == 0) continue;
                itemLength++;
            }

            List<ItemPositionAmount> itemPositions;
            float [,] vertices = GetInvBarItemIndices(items, itemLength, out itemPositions);
            DrawElements(_itemInvBarVAO, _itemInvBarVBO, vertices.Length * sizeof(float), vertices, itemLength * 4, _gameTextures.Items);
            

            //DrawItemSelector
            int selected = _mainModel.GetModelManager.SelectedInventorySlot;
            float[,] verticesSelector = GetInvBarSelectorIndices(selected);
            DrawElements(_invBarSelectorVAO, _invBarSelectorVBO, sizeof(float) * verticesSelector.Length, verticesSelector, 4, _gameTextures.Itembar_Selector);

            DrawNumbers(itemPositions);     //Zeichnet die dazugehörigen Zahlen
        }

        private void DrawElements(uint vao, uint vbo, int bufferSize, float[,] vertices, int verticesToDraw, uint gameTexture)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, bufferSize, vertices, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gameTexture);

            GL.DrawArrays(PrimitiveType.QuadsExt, 0, verticesToDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Disable(EnableCap.Blend);
        }

        private void DrawElements(uint vao, uint vbo, int verticesToDraw, uint gameTexture)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gameTexture);

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.DrawArrays(PrimitiveType.Quads, 0, verticesToDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void DrawNumbers(List<ItemPositionAmount> itemPositions)
        {
            Vector2 offset = new Vector2(25f, -5f);

            Matrix4 projectionMatrix = Matrix4.CreateOrthographic(ClientRectangle.Width, ClientRectangle.Height, -1.0f, 1.0f);
            _drawing.ProjectionMatrix = projectionMatrix;
            _drawing.DrawingPimitiveses.Clear();

            var textOpts = new QFontRenderOptions()
            {
                Colour = Color.FromArgb(new Color4(0.0f, 1.0f, 1.0f, 1.0f).ToArgb()),
                DropShadowActive = true
            };

            foreach(ItemPositionAmount item in itemPositions)
            {
                _drawing.Print(_font, item.Amount.ToString(), new Vector3(item.Position.X + offset.X, item.Position.Y + offset.Y, 0.0f), QFontAlignment.Right, textOpts);
            }

            _drawing.RefreshBuffers();
            _drawing.Draw();
        }

        private void DrawInventory()
        {
            //Zeichne Hintergrund
            DrawElements(_inventoryVAO, _inventoryVBO, 4, _gameTextures.Inventory);

            //Zeichne einzelne Items
            int itemCount;
            List<ItemPositionAmount> itemPositions;
            List<ViewItemPositions> viewItemPositions;
            float[,] vertices = GetInventoryItemsPos(_mainModel.GetModelManager.Player.ItemInventory, out itemCount, out itemPositions, out viewItemPositions);
            _mainModel.GetModelManager.ViewItemPositions = viewItemPositions;   //Setze aktuelle Positionen, wo die Inventar Items gezeichnet werden, für Inventar Funktionen
            DrawElements(_invItemsPosVAO, _invItemsPosVBO, vertices.Length, vertices, itemCount * 4, _gameTextures.Items);

            Item holdItem = _mainModel.GetModelManager.Player.ItemInventory.ActiveHoldingItem;
            if (holdItem != null)
            {
                Vector2 mousePosition = new Vector2(PointToClient(Control.MousePosition).X, PointToClient(Control.MousePosition).Y);
                Vector2 mouseMiddle = new Vector2(mousePosition.X - (Width / 2), -(mousePosition.Y - (Height / 2)));

                float[,] holdItemVertices = GetVerticesHoldItem(holdItem, mouseMiddle);
                //DrawElements(_invHoldItemVAO, _invHoldItemVBO, holdItemVertices.Length, holdItemVertices, 4, _gameTextures.Items);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _gameTextures.Items);

                GL.BindVertexArray(_invHoldItemVAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _invHoldItemVBO);

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * holdItemVertices.Length, holdItemVertices);
                GL.DrawArrays(PrimitiveType.Quads, 0, 4);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Blend);
            }

            DrawNumbers(itemPositions);
        }

        private void DrawMousePointer()
        {
            _shader.SetVector4("blockColor", new Vector4(1f, 1f, 1f, 1f));
            float mouseSize = 0.2f;
            Vector2 min = new Vector2(_mainModel.GetModelManager.WorldMousePosition.X - mouseSize, _mainModel.GetModelManager.WorldMousePosition.Y - mouseSize);
            Vector2 max = new Vector2(_mainModel.GetModelManager.WorldMousePosition.X + mouseSize, _mainModel.GetModelManager.WorldMousePosition.Y + mouseSize);

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

            GL.BindVertexArray(_mouseVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _mouseVBO);

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
            _font = new QFont("fonts/Depredationpixie.ttf", 15, new QuickFont.Configuration.QFontBuilderConfiguration(true));
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

        private void AlterVertexBufferBlocks(uint vao, uint vbo, float [,] vertices)
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
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

        private void InitVertexBufferWaterBlocks()
        {
            GL.GenVertexArrays(1, out _waterBlocksVAO);
            GL.GenBuffers(1, out _waterBlocksVBO);
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

        private void InitVertexBufferInvBar()
        {
            float[,] indices = GetUpdatedInvBarPos();

            GL.GenVertexArrays(1, out _invBarVAO);
            GL.GenBuffers(1, out _invBarVBO);

            GL.BindVertexArray(_invBarVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _invBarVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, indices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void InitVertexBufferInvBarItems()
        {
            GL.GenVertexArrays(1, out _itemInvBarVAO);
            GL.GenBuffers(1, out _itemInvBarVBO);

        }

        private void InitVertexBufferInvBarSelector()
        {
            GL.GenVertexArrays(1, out _invBarSelectorVAO);
            GL.GenBuffers(1, out _invBarSelectorVBO);
        }

        private void InitVertexBufferInvHoldItem()
        {
            GL.GenVertexArrays(1, out _invHoldItemVAO);
            GL.GenBuffers(1, out _invHoldItemVBO);

            GL.BindVertexArray(_invHoldItemVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _invHoldItemVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void InitVertexBufferBackground()
        {
            GL.GenVertexArrays(1, out _background1VAO);
            GL.GenBuffers(1, out _background1VBO);


            GL.BindVertexArray(_mouseVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _mouseVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void AlterVertexBufferInvBar()
        {
            float[,] indices = GetUpdatedInvBarPos();

            GL.BindVertexArray(_invBarVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _invBarVBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * 4 * 4, indices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void AlterVertexBufferInventory()
        {
            float[,] indices = GetUpdatedInventoryPos();

            GL.BindVertexArray(_inventoryVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _inventoryVBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * 4 * 4, indices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private float[,] GetUpdatedInvBarPos()
        {
            Vector2 position = new Vector2(0f, (Height / 2) - 50f);
            Vector2 size = new Vector2(300f, 40f);

            float[,] indices = new float[4, 4]
            {
                { position.X - size.X, position.Y - size.Y,   0.0f, 1.0f },
                { position.X + size.X, position.Y - size.Y,   1.0f, 1.0f },
                { position.X + size.X, position.Y + size.Y,   1.0f, 0.0f },
                { position.X - size.X, position.Y + size.Y,   0.0f, 0.0f }
            };

            return indices;
        }

        private float[,] GetInvBarItemIndices(Item[] item, int itemLength, out List<ItemPositionAmount> itemPosition)
        {
            int count = 0;
            itemPosition = new List<ItemPositionAmount>();

            Vector2 gridSize = new Vector2(8f, 8f);
            Vector2 size = new Vector2(20f, 20f);
            Vector2 position = new Vector2(-255f, (Height / 2) - 50f);
            float step = 56.8f;

            float[,] vertices = new float[4 * itemLength, 4];

            for (int i = 0; i < 10; i++)
            {
                if(item[i] == null)
                {
                    position.X += step;
                    continue;
                }
                if (item[i].ID == 0)
                {
                    position.X += step;
                    continue;
                }

                Vector2 min = new Vector2();
                Vector2 max = new Vector2();

                GetTextureCoord(item[i].ID, gridSize, out min, out max);

                float[,] vert = new float[4, 4]
                {
                    { position.X - size.X, position.Y - size.Y,   min.X, max.Y },
                    { position.X + size.X, position.Y - size.Y,   max.X, max.Y },
                    { position.X + size.X, position.Y + size.Y,   max.X, min.Y },
                    { position.X - size.X, position.Y + size.Y,   min.X, min.Y }
                };

                for (int ic = 0; ic < 4; ic++)
                {
                    for (int ia = 0; ia < 4; ia++)
                    {
                        vertices[count, ia] = vert[ic, ia];
                    }
                    count++;
                }

                itemPosition.Add(new ItemPositionAmount(new Vector2(position.X, position.Y), item[i].Amount));
                position.X += step;
            }

            return vertices;
        }

        private float[,] GetInvBarSelectorIndices(int selecteditem)
        {
            Vector2 size = new Vector2(40f, 40f);
            Vector2 position = new Vector2(-255f, (Height / 2) - 50f);
            float step = 56.8f;

            position.X += (step * selecteditem);

            float[,] vertices = new float[4, 4]
                {
                    { position.X - size.X, position.Y - size.Y,   0.0f, 1.0f },
                    { position.X + size.X, position.Y - size.Y,   1.0f, 1.0f },
                    { position.X + size.X, position.Y + size.Y,   1.0f, 0.0f },
                    { position.X - size.X, position.Y + size.Y,   0.0f, 0.0f }
                };

            return vertices;
        }

        private float[,] GetUpdatedInventoryPos()
        {
            Vector2 position = new Vector2(0f, 0f);
            Vector2 size = new Vector2(300f, 300f);

            float[,] indices = new float[4, 4]
            {
                { position.X - size.X, position.Y - size.Y,   0.0f, 1.0f },
                { position.X + size.X, position.Y - size.Y,   1.0f, 1.0f },
                { position.X + size.X, position.Y + size.Y,   1.0f, 0.0f },
                { position.X - size.X, position.Y + size.Y,   0.0f, 0.0f }
            };

            return indices;
        }
        private void InitVertexBufferInventory()
        {
            float[,] indices = GetUpdatedInventoryPos();

            GL.GenVertexArrays(1, out _inventoryVAO);
            GL.GenBuffers(1, out _inventoryVBO);

            GL.BindVertexArray(_inventoryVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _inventoryVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * 4, indices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void InitInventoryItemsPos()
        {
            GL.GenVertexArrays(1, out _invItemsPosVAO);
            GL.GenBuffers(1, out _invItemsPosVBO);
        }

        private float[,] GetInventoryItemsPos(Inventory inventory, out int itemCount, out List<ItemPositionAmount> itemPositions, out List<ViewItemPositions> viewItemPositions)
        {
            int x = 10, y = 4;
            Vector2 steps = new Vector2(50f, 50f);
            Vector2 size = new Vector2(25f, 25f);
            Vector2 startPos = new Vector2(-225f, 200f);
            Vector2 position = new Vector2(startPos.X, startPos.Y);

            float[,] indices = new float[x * y * 4, 4];
            int count = 0;
            itemCount = 0;
            itemPositions = new List<ItemPositionAmount>();
            viewItemPositions = new List<ViewItemPositions>();

            for(int iy = 0; iy < y; iy++)
            {
                for(int ix = 0; ix < x; ix++)
                {
                    viewItemPositions.Add(new ViewItemPositions(new Vector2(position.X, position.Y), size, ix, iy));    //Jeder verfügbare Itemslot soll hinzugefügt werden

                    if (inventory.GetItem(ix, iy) != null)
                    {
                        Vector2 min, max;
                        GetTextureCoord(inventory.GetItem(ix, iy).ID, new Vector2(8, 8), out min, out max);

                        float[,] vert = new float[4, 4]
                        {
                            { position.X - size.X, position.Y - size.Y,   min.X, max.Y },
                            { position.X + size.X, position.Y - size.Y,   max.X, max.Y },
                            { position.X + size.X, position.Y + size.Y,   max.X, min.Y },
                            { position.X - size.X, position.Y + size.Y,   min.X, min.Y }
                        };

                        for (int ic = 0; ic < 4; ic++)
                        {
                            for (int ia = 0; ia < 4; ia++)
                            {
                                indices[count, ia] = vert[ic, ia];
                            }
                            count++;
                        }

                        itemPositions.Add(new ItemPositionAmount(new Vector2(position.X, position.Y), inventory.GetItem(ix, iy).Amount));
                        itemCount++;
                    }

                    position.X += steps.X;
                }

                position.X = startPos.X;
                position.Y -= steps.Y;
            }

            return indices;

            /*
            GL.BindVertexArray(_invItemsPosVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _invItemsPosVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 4 * indices.Length, indices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            */
        }

        private float[,] GetVerticesHoldItem(Item holdItem, Vector2 mousePosition)
        {
            Vector2 size = new Vector2(20f, 20f);
            Vector2 min;
            Vector2 max;
            GetTextureCoord(holdItem.ID, new Vector2(8f, 8f), out min, out max);

            
            float[,] vertices = new float[4, 4]
                {
                    { mousePosition.X - size.X, mousePosition.Y - size.Y,   min.X, max.Y },
                    { mousePosition.X + size.X, mousePosition.Y - size.Y,   max.X, max.Y },
                    { mousePosition.X + size.X, mousePosition.Y + size.Y,   max.X, min.Y },
                    { mousePosition.X - size.X, mousePosition.Y + size.Y,   min.X, min.Y }
                };
            

            return vertices;
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

            System.Numerics.Vector2 upperLeft = _logic.CalculateViewToWorldPosition(new System.Numerics.Vector2(0f, 0f), playerPosition, winPos);
            System.Numerics.Vector2 lowerRight = _logic.CalculateViewToWorldPosition(new System.Numerics.Vector2(winPos.Width, winPos.Height), playerPosition, winPos);

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

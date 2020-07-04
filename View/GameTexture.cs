using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.View
{
    class GameTextures
    {
        public uint InventoryBar;
        public uint Itembar_Selector;
        public uint Inventory;
        public uint Block;
        public uint Debug;
        public uint Debug2;
        public uint lolDoerte;
        public uint Items;

        public GameTextures()
        {
            LoadTextures();
        }

        private void LoadTextures()
        {
            InventoryBar = LoadTexture("Content/textures/Itembar.png");
            Itembar_Selector = LoadTexture("Content/textures/Itembar_Selector.png");
            Inventory = LoadTexture("Content/textures/Inventory.png");
            Block = LoadTexture("Content/textures/Block.png");
            Debug = LoadTexture("Content/textures/Debug.png");
            lolDoerte = LoadTexture("Content/textures/dumbdoerte.png");
            Debug2 = LoadTexture("Content/textures/Debug2.png");
            Items = LoadTexture("Content/textures/Items.png");
        }

        private uint LoadTexture(string file)
        {
            uint tex;

            Bitmap bitmap = new Bitmap(file);

            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return tex;
        }
    }
}

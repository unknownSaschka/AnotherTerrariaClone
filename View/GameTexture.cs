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
        public uint ItemsBack;
        public uint InventoryDebug;
        public uint Tree;

        public uint Background1;

        public uint Darkness;
        public uint LightSource1;
        public uint BlockDamage;

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
            ItemsBack = LoadTexture("Content/textures/ItemsBack.png");
            InventoryDebug = LoadTexture("Content/textures/InventoryDebug.png");
            Tree = LoadTexture("Content/textures/Tree.png");

            Background1 = LoadTexture("Content/images/background1.png");

            Darkness = LoadTexture("Content/textures/Darkness.png");
            LightSource1 = LoadTexture("Content/textures/LightSource1.png");
            BlockDamage = LoadTexture("Content/textures/Block_Damage.png");
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

        public void DeleteTextures()
        {
            GL.DeleteTexture(InventoryBar);
            GL.DeleteTexture(Itembar_Selector);
            GL.DeleteTexture(Inventory);
            GL.DeleteTexture(Block);
            GL.DeleteTexture(Debug);
            GL.DeleteTexture(lolDoerte);
            GL.DeleteTexture(Debug2);
            GL.DeleteTexture(Items);
            GL.DeleteTexture(ItemsBack);
            GL.DeleteTexture(InventoryDebug);
            GL.DeleteTexture(Tree);
            GL.DeleteTexture(Background1);
            GL.DeleteTexture(Darkness);
            GL.DeleteTexture(LightSource1);
            GL.DeleteTexture(BlockDamage);
        }
    }

    class MenuTextures
    {
        public uint MenuBackground;
        public uint Button;

        public MenuTextures()
        {
            LoadTextures();
        }

        private void LoadTextures()
        {
            MenuBackground = LoadTexture("Content/images/background2.png");
            Button = LoadTexture("Content/textures/Inventory.png");
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

        public void UnloadTextures()
        {
            GL.DeleteTexture(MenuBackground);
            GL.DeleteTexture(Button);
        }
    }
}

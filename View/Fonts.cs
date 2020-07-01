using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.View
{
    public class Fonts
    {
        public struct Character
        {
            public uint TextureID;     //ID handle of the glyph texture
            public Vector2 Size;       //Size of glyph
            public Vector2 Bearing;    //Offset from baseline to left/right
            public int Advance;       //Offset to advance to next glyph

            public Character(uint textureID, Vector2 size, Vector2 bearing, int advance)
            {
                TextureID = textureID;
                Size = size;
                Bearing = bearing;
                Advance = advance;
            }
        }

        public static Dictionary<char, Character> Characters;

        public Fonts(string fontPath)
        {
            Init(fontPath);
        }

        private void Init(string fontPath)
        {
            Characters = new Dictionary<char, Character>();
            Library library = new Library();

            Face face = new Face(library, fontPath);
            face.SetPixelSizes(0, 48);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);  //disable pixel-alignment restriction

            //Lädt jede Texture für die Character in den Grafikspeicher
            for (uint c = 0; c < 128; c++)
            {
                face.LoadChar(c, LoadFlags.Default, LoadTarget.Normal);

                uint textureID;

                GL.GenTextures(1, out textureID);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                GL.TexImage2D(
                    TextureTarget.Texture2D, 
                    0, 
                    PixelInternalFormat.Rgba, 
                    face.Glyph.Bitmap.Width, 
                    face.Glyph.Bitmap.Rows, 
                    0, 
                    PixelFormat.Rgba, 
                    PixelType.UnsignedByte, 
                    face.Glyph.Bitmap.Buffer);

                //Setze Textur optionen
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                Character character = new Character(
                    textureID,
                    new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                    new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                    face.Glyph.Advance.X.ToInt32());

                Characters.Add((char)c, character);
            }

            face.Dispose();
            library.Dispose();
        }
    }
}

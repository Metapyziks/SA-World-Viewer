using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using GTAMapViewer.Resource;

namespace GTAMapViewer.Graphics
{
    internal class Texture2D : Texture
    {
        public static readonly Texture2D Missing;

        static Texture2D()
        {
            Bitmap missingBmp = new Bitmap( 16, 16 );
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    bool black = ( ( x / 4 ) % 2 == ( y / 4 ) % 2 );
                    missingBmp.SetPixel( x, y, black ? Color.Black : Color.Magenta );
                }
            }
            Missing = new Texture2D( missingBmp );
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public Texture2D( Bitmap bitmap )
            : base( TextureTarget.Texture2D )
        {
            Width = bitmap.Width;
            Height = bitmap.Height;

            Bitmap = bitmap;
        }

        public Vector2 GetCoords( Vector2 pos )
        {
            return GetCoords( pos.X, pos.Y );
        }

        public Vector2 GetCoords( float x, float y )
        {
            return new Vector2
            {
                X = x / Bitmap.Width,
                Y = y / Bitmap.Height
            };
        }

        public Color GetPixel( int x, int y )
        {
            return Bitmap.GetPixel( x, y );
        }

        public void SetPixel( int x, int y, Color colour )
        {
            if ( this == Missing )
                return;

            Bitmap.SetPixel( x, y, colour );
            Update();
        }

        protected override void Load()
        {
            GL.TexEnv( TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float) TextureEnvMode.Modulate );

            BitmapData data = Bitmap.LockBits( new Rectangle( 0, 0, Bitmap.Width, Bitmap.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );

            Bitmap.UnlockBits( data );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) TextureMagFilter.Linear );
        }
    }
}

using System;
using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace GTAMapViewer.Graphics
{
    public class Texture2DArray : Texture
    {
        private Texture2D[] myTextures;

        private UInt32[] myData;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Count { get; private set; }

        public Texture2DArray( Texture2D[] textures )
            : base( TextureTarget.Texture2DArray )
        {
            Width = textures[ 0 ].Bitmap.Width;
            Height = textures[ 0 ].Bitmap.Height;

            myTextures = textures;

            Count = 1;
            while ( Count < myTextures.Length )
                Count <<= 1;

            int tileLength = Width * Height;

            myData = new uint[ tileLength * Count ];

            for ( int i = 0; i < myTextures.Length; ++i )
            {
                Bitmap tile = myTextures[ i ].Bitmap;

                int xScale = tile.Width / Width;
                int yScale = tile.Height / Height;

                for ( int x = 0; x < Width; ++x )
                {
                    for ( int y = 0; y < Height; ++y )
                    {
                        int tx = x * xScale;
                        int ty = y * yScale;

                        Color clr = tile.GetPixel( tx, ty );

                        myData[ i * tileLength + x + y * Width ]
                            = (UInt32) ( clr.R << 24 | clr.G << 16 | clr.B << 08 | clr.A << 00 );
                    }
                }
            }
        }

        protected override void Load()
        {
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapNearest );
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest );
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp );
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp );
            GL.TexImage3D( TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
                Width, Height, Count, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, myData );
            GL.GenerateMipmap( GenerateMipmapTarget.Texture2DArray );
        }
    }
}

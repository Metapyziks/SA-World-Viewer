using System;
using System.Collections.Generic;
using System.Drawing;
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
            byte[] data = new byte[ ( 16 * 16 ) << 2 ];
            int i = 0;
            for ( int x = 0; x < 16; ++x )
            {
                for ( int y = 0; y < 16; ++y )
                {
                    bool black = ( ( x / 4 ) % 2 == ( y / 4 ) % 2 );
                    data[ i++ ] = (byte) ( black ? 0 : 0xff );
                    data[ i++ ] = 0;
                    data[ i++ ] = (byte) ( black ? 0 : 0xff );
                    data[ i++ ] = 0xff;
                }
            }
            Missing = new Texture2D( 16, 16, data );
        }

        private static TextureWrapMode FindWrapMode( TextureNativeSectionData.WrapMode value )
        {
            switch ( value )
            {
                case TextureNativeSectionData.WrapMode.Wrap:
                    return TextureWrapMode.Repeat;
                case TextureNativeSectionData.WrapMode.Mirror:
                    return TextureWrapMode.MirroredRepeat;
                case TextureNativeSectionData.WrapMode.Clamp:
                    return TextureWrapMode.Clamp;
                default:
                    return TextureWrapMode.Repeat;
            }
        }

        private static PixelInternalFormat FindInternalFormat(
            TextureNativeSectionData.CompressionMode compression,
            TextureNativeSectionData.RasterFormat format )
        {
            switch ( (TextureNativeSectionData.RasterFormat) ( (int) format & 0x0fff ) )
            {
                case TextureNativeSectionData.RasterFormat.R5G6B5:
                    if ( compression == TextureNativeSectionData.CompressionMode.DXT1 )
                        return PixelInternalFormat.CompressedSrgbS3tcDxt1Ext;
                    goto default;
                case TextureNativeSectionData.RasterFormat.A1R5G5B5:
                    if ( compression == TextureNativeSectionData.CompressionMode.DXT1 )
                        return PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    goto default;
                case TextureNativeSectionData.RasterFormat.B8G8R8:
                    if ( compression == TextureNativeSectionData.CompressionMode.None )
                        return PixelInternalFormat.Rgba;
                    goto default;
                case TextureNativeSectionData.RasterFormat.B8G8R8A8:
                    if ( compression == TextureNativeSectionData.CompressionMode.None )
                        return PixelInternalFormat.Rgba;
                    goto default;
                case TextureNativeSectionData.RasterFormat.R4G4B4A4:
                    if ( compression == TextureNativeSectionData.CompressionMode.DXT3 )
                        return PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    goto default;
                default:
                    throw new UnhandledImageFormatException( compression, format );
            }
        }

        private static PixelFormat FindExternalFormat(
            TextureNativeSectionData.CompressionMode compression,
            TextureNativeSectionData.RasterFormat format )
        {
            if ( compression != TextureNativeSectionData.CompressionMode.None )
                return PixelFormat.Alpha;

            switch ( (TextureNativeSectionData.RasterFormat) ( (int) format & 0x0fff ) )
            {
                case TextureNativeSectionData.RasterFormat.B8G8R8:
                    return PixelFormat.Bgra;
                case TextureNativeSectionData.RasterFormat.B8G8R8A8:
                    return PixelFormat.Bgra;
                default:
                    throw new UnhandledImageFormatException( compression, format );
            }
        }

        private static int FindImageDataSize( int width, int height,
            TextureNativeSectionData.CompressionMode compression,
            TextureNativeSectionData.RasterFormat format )
        {
            switch ( (TextureNativeSectionData.RasterFormat) ( (int) format & 0x0fff ) )
            {
                case TextureNativeSectionData.RasterFormat.R5G6B5:
                    if ( compression == TextureNativeSectionData.CompressionMode.DXT1 )
                        return ( width * height ) >> 1;
                    goto default;
                case TextureNativeSectionData.RasterFormat.A1R5G5B5:
                    if ( compression == TextureNativeSectionData.CompressionMode.DXT1 )
                        return ( width * height ) >> 1;
                    goto default;
                case TextureNativeSectionData.RasterFormat.B8G8R8:
                    if ( compression == TextureNativeSectionData.CompressionMode.None )
                        return ( width * height ) << 2;
                    goto default;
                case TextureNativeSectionData.RasterFormat.B8G8R8A8:
                    if ( compression == TextureNativeSectionData.CompressionMode.None )
                        return ( width * height ) << 2;
                    goto default;
                case TextureNativeSectionData.RasterFormat.R4G4B4A4:
                    if ( compression == TextureNativeSectionData.CompressionMode.DXT3 )
                        return ( width * height );
                    goto default;
                default:
                    throw new UnhandledImageFormatException( compression, format );
            }
        }

        public readonly int Width;
        public readonly int Height;

        public readonly TextureWrapMode WrapModeU;
        public readonly TextureWrapMode WrapModeV;

        public readonly PixelInternalFormat InternalFormat;
        public readonly PixelFormat ExternalFormat;
        public readonly bool Compressed;
        public readonly bool Alpha;

        public readonly byte MipMapCount;
        public readonly bool ContainsMipMaps;
        public readonly bool GenerateMipMaps;

        public readonly byte[][] ImageLevelData;
        public readonly int[] ImageLevelSizes;

        public Texture2D( TextureNativeSectionData tex )
            : base( TextureTarget.Texture2D )
        {
            Width = tex.Width;
            Height = tex.Height;

            WrapModeU = FindWrapMode( tex.WrapU );
            WrapModeV = FindWrapMode( tex.WrapV );

            InternalFormat = FindInternalFormat( tex.Compression, tex.Format );
            ExternalFormat = FindExternalFormat( tex.Compression, tex.Format );

            Compressed = tex.Compression != TextureNativeSectionData.CompressionMode.None;
            Alpha = tex.Alpha;

            MipMapCount = 1; //tex.MipMapCount;
            ContainsMipMaps = ( tex.Format & TextureNativeSectionData.RasterFormat.ExtMipMap ) != 0;
            GenerateMipMaps = ( tex.Format & TextureNativeSectionData.RasterFormat.ExtAutoMipMap ) != 0;

            ImageLevelSizes = new int[ MipMapCount ];
            for ( int i = 0; i < MipMapCount; ++i )
                ImageLevelSizes[ i ] = FindImageDataSize( Width >> i, Height >> i, tex.Compression, tex.Format );

            ImageLevelData = new byte[ MipMapCount ][];
            int offset = 0;
            for ( int i = 0; i < MipMapCount; ++i )
            {
                ImageLevelData[ i ] = new byte[ ImageLevelSizes[ i ] ];
                Array.Copy( tex.ImageData, offset, ImageLevelData[ i ], 0, ImageLevelSizes[ i ] );
                offset += ImageLevelSizes[ i ];
            }
        }

        public Texture2D( int width, int height, byte[] data )
            : base( TextureTarget.Texture2D )
        {
            Width = width;
            Height = height;

            WrapModeU = TextureWrapMode.Repeat;
            WrapModeV = TextureWrapMode.Repeat;

            InternalFormat = PixelInternalFormat.Rgba;
            ExternalFormat = PixelFormat.Bgra;

            Compressed = false;
            Alpha = true;

            MipMapCount = 1;
            ContainsMipMaps = false;
            GenerateMipMaps = false;

            ImageLevelSizes = new int[] { ( width * height ) << 2 };
            ImageLevelData = new byte[][] { data };
        }

        protected override void Load()
        {
            GL.TexEnv( TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate );

            ErrorCheck( "loadtex_0" );

            if ( Compressed )
                GL.CompressedTexImage2D( TextureTarget, 0, InternalFormat, Width, Height, 0, ImageLevelSizes[ 0 ], ImageLevelData[ 0 ] );
            else
                GL.TexImage2D( TextureTarget, 0, InternalFormat, Width, Height, 0, ExternalFormat, PixelType.UnsignedInt8888Reversed, ImageLevelData[ 0 ] );

            ErrorCheck( "loadtex_1" );

            GL.TexParameter( TextureTarget, TextureParameterName.TextureWrapS, (int) WrapModeU );
            GL.TexParameter( TextureTarget, TextureParameterName.TextureWrapT, (int) WrapModeV );

            ErrorCheck( "loadtex_2" );

            GL.TexParameter( TextureTarget, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear );
            
            ErrorCheck( "loadtex_3" );
        }
    }
}

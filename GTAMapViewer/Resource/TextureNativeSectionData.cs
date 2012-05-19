﻿using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.TextureNative )]
    internal class TextureNativeSectionData : SectionData
    {
        public enum Filter : ushort
        {
            None = 0x0,
            Nearest = 0x1,
            Linear = 0x2,
            MipNearest = 0x3,
            MipLinear = 0x4,
            LinearMipNearest = 0x5,
            LinearMipLinear = 0x6,
            Unknown = 0x1101
        }

        public enum WrapMode : byte
        {
            None = 0,
            Wrap = 1,
            Mirror = 2,
            Clamp = 3
        }

        public enum CompressionMode : byte
        {
            None = 0,
            DXT1 = 1,
            DXT3 = 3
        }

        public enum RasterFormat : uint
        {
            Default = 0x0000,
            A1R5G5B5 = 0x0100,
            R5G6B5 = 0x0200,
            R4G4B4A4 = 0x0300,
            LUM8 = 0x0400,
            B8R8G8A8 = 0x0500,
            B8R8G8 = 0x0600,
            R5G5B5 = 0x0a00,

            ExtAutoMipMap = 0x1000,
            ExtPal8 = 0x2000,
            ExtPal4 = 0x4000,
            ExtMipMap = 0x8000
        }

        public readonly UInt32 PlatformID;
        public readonly Filter FilterFlags;
        public readonly WrapMode WrapV;
        public readonly WrapMode WrapU;
        public readonly String DiffuseName;
        public readonly String AlphaName;
        public readonly RasterFormat Format;
        public readonly bool Alpha;
        public readonly CompressionMode Compression;
        public readonly UInt16 Width;
        public readonly UInt16 Height;
        public readonly byte BPP;
        public readonly byte MipMapCount;
        public readonly byte RasterType;
        public readonly UInt32 ImageDataSize;

        public readonly long DataStartPosition;

        public Texture2D Texture;

        public bool Loaded { get { return Texture != null; } }

        public TextureNativeSectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );
            BinaryReader reader = new BinaryReader( stream );

            PlatformID = reader.ReadUInt32();
            FilterFlags = (Filter) reader.ReadUInt16();
            WrapV = (WrapMode) reader.ReadByte();
            WrapU = (WrapMode) reader.ReadByte();
            DiffuseName = reader.ReadString( 32 );
            AlphaName = reader.ReadString( 32 );
            Format = (RasterFormat) reader.ReadUInt32();

            if ( PlatformID == 9 )
            {
                String dxt = reader.ReadString( 4 );
                switch ( dxt )
                {
                    case "DXT1":
                        Compression = CompressionMode.DXT1; break;
                    case "DXT3":
                        Compression = CompressionMode.DXT3; break;
                    default:
                        Compression = CompressionMode.None; break;
                }
            }
            else
                Alpha = reader.ReadUInt32() == 0x1;

            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BPP = (byte) ( reader.ReadByte() >> 3 );
            MipMapCount = reader.ReadByte();
            RasterType = reader.ReadByte();

            if ( RasterType != 0x4 )
                throw new Exception( "Unexpected RasterType, expected 0x04." );

            if ( PlatformID == 9 )
                Alpha = ( reader.ReadByte() & 0x1 ) == 0x1;
            else
                Compression = (CompressionMode) reader.ReadByte();

            ImageDataSize = reader.ReadUInt32();

            DataStartPosition = stream.GlobalPosition;
        }

        private static Color DecodeColor( UInt16 val )
        {
            int r = ( ( val >> 11 ) & 0x1f ) << 3;
            int g = ( ( val >> 5 ) & 0x3f ) << 2;
            int b = ( val & 0x1f ) << 3;
            return Color.FromArgb( r, g, b );
        }

        public void Load( FramedStream stream )
        {
            Bitmap bmp = new Bitmap( Width, Height, PixelFormat.Format32bppArgb );

            switch ( (RasterFormat) ( (int) Format & 0xf000 ) )
            {
                case RasterFormat.Default:
                    break;
                case RasterFormat.ExtAutoMipMap:
                    break;
                case RasterFormat.ExtMipMap:
                    break;
                default:
                    throw new Exception( "Unhandled image format encountered" );
            }

            switch ( (RasterFormat) ( (int) Format & 0x0fff ) )
            {
                case RasterFormat.R5G6B5:
                    byte[,] clrs = new byte[ 4, 3 ];
                    for ( int y = 0; y < Height; y += 4 )
                    {
                        for ( int x = 0; x < Width; x += 4 )
                        {
                            Color clr0 = DecodeColor( BitConverter.ToUInt16( stream.ReadBytes( 2 ), 0 ) );
                            Color clr1 = DecodeColor( BitConverter.ToUInt16( stream.ReadBytes( 2 ), 0 ) );

                            clrs[ 0, 0 ] = clr0.R; clrs[ 0, 1 ] = clr0.G; clrs[ 0, 2 ] = clr0.B;
                            clrs[ 1, 0 ] = clr1.R; clrs[ 1, 1 ] = clr1.G; clrs[ 1, 2 ] = clr1.B;

                            for ( int i = 0; i < 3; ++i )
                            {
                                if ( clrs[ 0, i ] > clrs[ 1, i ] )
                                {
                                    clrs[ 2, i ] = (byte) ( 2 * clrs[ 0, i ] / 3 + clrs[ 1, i ] / 3 );
                                    clrs[ 3, i ] = (byte) ( clrs[ 0, i ] / 3 + 2 * clrs[ 1, i ] / 3 );
                                }
                                else
                                {
                                    clrs[ 2, i ] = (byte) ( clrs[ 0, i ] / 2 + clrs[ 1, i ] / 2 );
                                    clrs[ 3, i ] = 0x00;
                                }
                            }

                            UInt32 lookup = BitConverter.ToUInt32( stream.ReadBytes( 4 ), 0 );

                            for ( int i = 0; i < 16; ++i )
                            {
                                byte clr = (byte) ( ( lookup >> ( i << 1 ) ) & 0x3 );
                                bmp.SetPixel( x + ( i % 4 ), y + ( i / 4 ),
                                    Color.FromArgb( clrs[ clr, 0 ], clrs[ clr, 1 ], clrs[ clr, 2 ] ) );
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception( "Unhandled image format encountered" );
            }

            Texture = new Texture2D( bmp );
        }
    }
}
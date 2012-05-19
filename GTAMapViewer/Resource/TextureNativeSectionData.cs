using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using GTAMapViewer;

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

        public enum RasterFormat : uint
        {
            Default = 0x0000,
            A1R5G5B5 = 0x0100,
            AR5G6B5 = 0x0200,
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
        public readonly byte Compression;
        public readonly UInt16 Width;
        public readonly UInt16 Height;
        public readonly byte BPP;
        public readonly byte MipMapCount;
        public readonly byte RasterType;
        public readonly UInt32 ImageDataSize;

        public readonly long DataStartPosition;

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
                Compression = (byte) ( reader.ReadUInt32() & 0xff );
            else
                Alpha = reader.ReadUInt32() == 0x1;

            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BPP = reader.ReadByte();
            MipMapCount = reader.ReadByte();
            RasterType = reader.ReadByte();

            if ( RasterType != 0x4 )
                throw new Exception( "Unexpected RasterType, expected 0x04." );

            if ( PlatformID == 9 )
                Alpha = ( reader.ReadByte() & 0x1 ) == 0x1;
            else
                Compression = reader.ReadByte();

            ImageDataSize = reader.ReadUInt32();

            DataStartPosition = stream.GlobalPosition;
        }
    }
}

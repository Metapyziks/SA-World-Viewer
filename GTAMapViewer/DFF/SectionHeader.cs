using System;
using System.IO;

namespace GTAMapViewer.DFF
{
    internal struct SectionHeader
    {
        public readonly SectionType Type;
        public readonly UInt32 Size;
        public readonly UInt16 Version;

        public SectionHeader( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            Type = (SectionType) reader.ReadUInt32();
            Size = reader.ReadUInt32();
            reader.ReadUInt16(); // Unknown
            Version = reader.ReadUInt16();
        }
    }
}

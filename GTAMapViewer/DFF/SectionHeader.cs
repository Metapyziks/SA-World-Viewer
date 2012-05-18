using System;
using System.IO;

namespace GTAMapViewer.DFF
{
    internal struct SectionHeader
    {
        public readonly SectionType Type;
        public readonly UInt32 Size;
        public readonly UInt16 Version;

        public SectionHeader( FramedStream stream )
        {
            BinaryReader reader = new BinaryReader( stream );
            Type = (SectionType) reader.ReadUInt32();
            Size = reader.ReadUInt32();
            reader.ReadUInt16(); // Unknown
            Version = reader.ReadUInt16();
        }

        public override string ToString()
        {
            return String.Format( "{0}, Size: {1}, Vers: {2}", Type.ToString(), Size, Version );
        }
    }
}

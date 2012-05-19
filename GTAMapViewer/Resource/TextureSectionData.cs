using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.Texture )]
    internal class TextureSectionData : SectionData
    {
        public readonly UInt16 FilterMode;
        public readonly String TextureName;
        public readonly String MaskName;

        public TextureSectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );
            BinaryReader reader = new BinaryReader( stream );

            FilterMode = reader.ReadUInt16();
            reader.ReadUInt16(); // Unknown

            TextureName = ( new Section( stream ).Data as StringSectionData ).Value;
            MaskName = ( new Section( stream ).Data as StringSectionData ).Value;
        }
    }
}

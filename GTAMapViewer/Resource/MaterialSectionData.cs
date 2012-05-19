using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.Material )]
    internal class MaterialSectionData : SectionData
    {
        public readonly UInt32 Colour;
        public readonly UInt32 TextureCount;
        public readonly TextureSectionData[] Textures;

        public MaterialSectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );
            BinaryReader reader = new BinaryReader( stream );

            reader.ReadUInt32(); // Unknown
            Colour = reader.ReadUInt32();
            var unk = reader.ReadUInt32(); // Unknown
            TextureCount = reader.ReadUInt32();
            Textures = new TextureSectionData[ TextureCount ]; 
            reader.ReadSingle(); // Unknown
            reader.ReadSingle(); // Unknown
            reader.ReadSingle(); // Unknown

            for ( int i = 0; i < TextureCount; ++i )
                Textures[ i ] = new Section( stream ).Data as TextureSectionData;
        }

        public override void LoadAdditionalResources()
        {
            foreach ( TextureSectionData tex in Textures )
                tex.LoadAdditionalResources();
        }
    }
}

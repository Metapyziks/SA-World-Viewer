using System;
using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.TextureDictionary )]
    internal class TextureDictionarySectionData : SectionData
    {
        public UInt16 TextureCount;
        public TextureNativeSectionData[] Textures;

        public TextureDictionarySectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );
            BinaryReader reader = new BinaryReader( stream );

            TextureCount = reader.ReadUInt16();
            Textures = new TextureNativeSectionData[ TextureCount ];
            reader.ReadUInt16(); // Unknown

            for ( int i = 0; i < TextureCount; ++i )
                Textures[ i ] = new Section( stream ).Data as TextureNativeSectionData;
        }
    }
}

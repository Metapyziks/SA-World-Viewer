using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.TextureDictionary )]
    internal class TextureDictionarySectionData : SectionData
    {
        public readonly UInt32 ID;
        public readonly UInt32 ChunkSize;
        public readonly UInt32 Marker;
        public TextureNativeSectionData[] Textures;

        public TextureDictionarySectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );

            List<TextureNativeSectionData> textures = new List<TextureNativeSectionData>();
            while( stream.CanRead )
            {
                Section section = new Section( stream );
                if ( section.Type == SectionType.TextureNative )
                    textures.Add( section.Data as TextureNativeSectionData );
                else
                    break;
            }

            Textures = textures.ToArray();
        }
    }
}

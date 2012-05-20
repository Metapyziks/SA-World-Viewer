using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using GTAMapViewer.Graphics;

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

    internal class TextureDictionary
    {
        private Dictionary<String, TextureNativeSectionData> myDiffuseTextures;
        private Dictionary<String, TextureNativeSectionData> myMaskTextures;

        public TextureDictionary( FramedStream stream )
        {
            Section sec = new Section( stream );
            TextureDictionarySectionData data = sec.Data as TextureDictionarySectionData;

            myDiffuseTextures = new Dictionary<string, TextureNativeSectionData>();
            myMaskTextures = new Dictionary<string, TextureNativeSectionData>();

            foreach ( TextureNativeSectionData tex in data.Textures )
            {
                if ( tex.DiffuseName.Length > 0 )
                    myDiffuseTextures.Add( tex.DiffuseName, tex );
                if ( tex.AlphaName.Length > 0 )
                    myMaskTextures.Add( tex.AlphaName, tex );
            }
        }

        public TextureNativeSectionData this[ String name, TextureType type ]
        {
            get
            {
                if ( type == TextureType.Diffuse )
                    return myDiffuseTextures[ name ];
                else
                    return myMaskTextures[ name ];
            }
        }

        public Texture2D Load( String name, TextureType type )
        {
            return this[ name, type ].Texture;
        }
    }
}

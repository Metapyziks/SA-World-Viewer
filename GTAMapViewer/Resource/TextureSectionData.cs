using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.Texture )]
    internal class TextureSectionData : SectionData
    {
        public readonly TextureNativeSectionData.Filter FilterMode;
        public readonly String TextureName;
        public readonly String MaskName;

        public Texture2D Texture;
        public Texture2D Mask;

        public TextureSectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );
            BinaryReader reader = new BinaryReader( stream );

            FilterMode = (TextureNativeSectionData.Filter) reader.ReadUInt16();
            var unk = reader.ReadUInt16(); // Unknown

            TextureName = ( new Section( stream ).Data as StringSectionData ).Value;
            MaskName = ( new Section( stream ).Data as StringSectionData ).Value;
        }

        public void LoadTextures( TextureDictionary txd )
        {
            if ( TextureName.Length > 0 )
            {
                if ( txd.Contains( TextureName, TextureType.Diffuse ) )
                    Texture = txd[ TextureName, TextureType.Diffuse ];
                else
                    Texture = Texture2D.Missing;
            }
            if ( MaskName.Length > 0 )
            {
                if ( txd.Contains( MaskName, TextureType.Mask ) )
                    Mask = txd[ MaskName, TextureType.Mask ];
                else
                    Mask = null;
            }
        }
    }
}

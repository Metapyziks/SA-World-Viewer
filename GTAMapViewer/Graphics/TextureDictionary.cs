using System;
using System.Collections.Generic;

using GTAMapViewer.Resource;

namespace GTAMapViewer.Graphics
{
    internal class TextureDictionary : IDisposable
    {
        private Dictionary<String, Texture2D> myDiffuseTextures;
        private Dictionary<String, Texture2D> myMaskTextures;

        public readonly String Name;

        public TextureDictionary( String name, FramedStream stream )
        {
            Name = name;

            Section sec = new Section( stream );
            TextureDictionarySectionData data = sec.Data as TextureDictionarySectionData;

            myDiffuseTextures = new Dictionary<string, Texture2D>();
            myMaskTextures = new Dictionary<string, Texture2D>();

            foreach ( TextureNativeSectionData tex in data.Textures )
            {
                Texture2D t2d = new Texture2D( tex );
                if ( tex.DiffuseName.Length > 0 && !myDiffuseTextures.ContainsKey( tex.DiffuseName ) )
                    myDiffuseTextures.Add( tex.DiffuseName, t2d );
                if ( tex.AlphaName.Length > 0 && !myMaskTextures.ContainsKey( tex.AlphaName ) )
                    myMaskTextures.Add( tex.AlphaName, t2d );
            }
        }

        public bool Contains( String name, TextureType type )
        {
            if ( type == TextureType.Diffuse )
                return myDiffuseTextures.ContainsKey( name );
            else
                return myMaskTextures.ContainsKey( name );
        }

        public Texture2D this[ String name, TextureType type ]
        {
            get
            {
                if ( type == TextureType.Diffuse )
                    return myDiffuseTextures[ name ];
                else
                    return myMaskTextures[ name ];
            }
        }

        public void Dispose()
        {
            foreach ( Texture2D tex in myDiffuseTextures.Values )
                tex.Dispose();

            foreach ( Texture2D tex in myMaskTextures.Values )
                tex.Dispose();
        }
    }
}

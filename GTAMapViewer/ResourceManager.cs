using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using GTAMapViewer.Resource;
using GTAMapViewer.Graphics;

namespace GTAMapViewer
{
    internal enum TextureType : byte
    {
        Diffuse,
        Mask
    }

    internal static class ResourceManager
    {
        private class Resource<T>
        {
            public readonly T Value;
            public int Uses;

            public Resource( T resource )
            {
                Value = resource;
                Uses = 0;
            }

            public bool Used { get { return Uses > 0; } }
        }

        private class ImageArchive
        {
            private struct ImageArchiveEntry
            {
                public readonly UInt32 Offset;
                public readonly UInt32 Size;
                public readonly String Name;

                public ImageArchiveEntry( Stream stream )
                {
                    BinaryReader reader = new BinaryReader( stream );
                    Offset = reader.ReadUInt32() << 11;
                    ushort sizeSecond = reader.ReadUInt16();
                    ushort sizeFirst = reader.ReadUInt16();
                    Size = (UInt32) ( ( sizeFirst != 0 ) ? sizeFirst << 11 : sizeSecond << 11 );
                    Name = new String( reader.ReadChars( 24 ) ).TrimNullChars();
                }
            }

            public static ImageArchive Load( String filePath )
            {
                return new ImageArchive( new FileStream( filePath, FileMode.Open, FileAccess.Read ) );
            }

            private Stream myStream;

            private Dictionary<String, ImageArchiveEntry> myFileDict;
            private Dictionary<String, TextureNativeSectionData> myDiffTextureDict;
            private Dictionary<String, TextureNativeSectionData> myMaskTextureDict;

            public readonly String Version;
            public readonly UInt32 Length;

            public ImageArchive( Stream stream )
            {
                myStream = stream;

                BinaryReader reader = new BinaryReader( stream );
                Version = new String( reader.ReadChars( 4 ) );
                Length = reader.ReadUInt32();

                myFileDict = new Dictionary<string, ImageArchiveEntry>();

                for ( int i = 0; i < Length; ++i )
                {
                    ImageArchiveEntry entry = new ImageArchiveEntry( stream );
                    myFileDict.Add( entry.Name, entry );
                }

                myDiffTextureDict = new Dictionary<string, TextureNativeSectionData>();
                myMaskTextureDict = new Dictionary<string, TextureNativeSectionData>();

                if ( myFileDict.ContainsKey( "des.txd" ) )
                    return;

                FramedStream str = new FramedStream( myStream );
                foreach ( ImageArchiveEntry entry in myFileDict.Values.Where( x => x.Name.EndsWith( ".txd" ) ) )
                {
                    str.PushFrame( entry.Offset, entry.Size );
                    TextureDictionarySectionData tdic = new Section( str ).Data as TextureDictionarySectionData;
                    str.PopFrame();
                    foreach ( TextureNativeSectionData tex in tdic.Textures )
                    {
                        if ( tex.DiffuseName.Length > 0 && !myDiffTextureDict.ContainsKey( tex.DiffuseName ) )
                            myDiffTextureDict.Add( tex.DiffuseName, tex );
                        if ( tex.AlphaName.Length > 0 && !myMaskTextureDict.ContainsKey( tex.AlphaName ) )
                            myMaskTextureDict.Add( tex.AlphaName, tex );
                    }
                }
            }

            public bool ContainsFile( String name )
            {
                return myFileDict.ContainsKey( name );
            }

            public bool ContainsTexture( String name, TextureType type )
            {
                if( type == TextureType.Diffuse )
                    return myDiffTextureDict.ContainsKey( name );

                return myMaskTextureDict.ContainsKey( name );
            }

            public FramedStream ReadFile( String name )
            {
                ImageArchiveEntry entry = myFileDict[ name ];
                FramedStream stream = new FramedStream( myStream );
                stream.PushFrame( entry.Offset, entry.Size );
                return stream;
            }

            public Texture2D LoadTexture( String name, TextureType type )
            {
                TextureNativeSectionData tex = ( type == TextureType.Diffuse ?
                    myDiffTextureDict[ name ] : myMaskTextureDict[ name ] );

                if ( !tex.Loaded )
                {
                    FramedStream stream = new FramedStream( myStream );
                    stream.PushFrame( tex.DataStartPosition, tex.ImageDataSize );
                    tex.Load( stream );
                }

                return tex.Texture;
            }
        }

        private static List<ImageArchive> stLoadedArchives = new List<ImageArchive>();

        private static Dictionary<String, Resource<Model>> stLoadedModels
            = new Dictionary<string, Resource<Model>>();
        private static Dictionary<String, Resource<Texture2D>> stLoadedTextures
            = new Dictionary<string, Resource<Texture2D>>();

        public static void LoadArchive( String filePath )
        {
            stLoadedArchives.Add( ImageArchive.Load( filePath ) );
        }

        public static Model LoadModel( String name )
        {
            name = name.ToLower();

            if ( !name.EndsWith( ".dff" ) )
                name += ".dff";

            Resource<Model> res = null;

            if ( !stLoadedModels.ContainsKey( name ) )
            {
                foreach ( ImageArchive archive in stLoadedArchives )
                {
                    if ( archive.ContainsFile( name ) )
                    {
                        res = new Resource<Model>( new Model( archive.ReadFile( name ) ) );
                        res.Value.LoadAdditionalResources();
                        break;
                    }
                }

                if ( res == null )
                    throw new KeyNotFoundException( "File with name \"" + name + "\" not present in a loaded archive." );

                stLoadedModels.Add( name, res );
            }
            else
                res = stLoadedModels[ name ];

            ++res.Uses;

            return res.Value;
        }

        public static void UnloadModel( String name )
        {
            --stLoadedModels[ name ].Uses;
        }

        private static String GetTextureName( String name, TextureType type )
        {
            return type.ToString()[ 0 ] + "_" + name;
        }

        public static Texture2D LoadTexture( String name, TextureType type )
        {
            Resource<Texture2D> res = null;

            String rname = GetTextureName( name, type );

            if ( !stLoadedTextures.ContainsKey( rname ) )
            {
                foreach ( ImageArchive archive in stLoadedArchives )
                {
                    if ( archive.ContainsTexture( name, type ) )
                    {
                        res = new Resource<Texture2D>( archive.LoadTexture( name, type ) );
                        break;
                    }
                }

                if ( res == null )
                    res = new Resource<Texture2D>( Texture2D.Missing );

                stLoadedTextures.Add( rname, res );
            }
            else
                res = stLoadedTextures[ rname ];

            ++res.Uses;

            return res.Value;
        }

        public static void UnloadTexture( String name, TextureType type )
        {
            String rname = GetTextureName( name, type );
            --stLoadedTextures[ rname ].Uses;
        }
    }
}

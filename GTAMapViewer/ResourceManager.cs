using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using GTAMapViewer.Resource;
using GTAMapViewer.Graphics;

namespace GTAMapViewer
{
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
            private Dictionary<String, TextureNativeSectionData> myTextureDict;

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

                myTextureDict = new Dictionary<string, TextureNativeSectionData>();

                if ( myFileDict.ContainsKey( "des.txd" ) )
                    return;

                foreach ( ImageArchiveEntry entry in myFileDict.Values.Where( x => x.Name.EndsWith( ".txd" ) ) )
                {
                    FramedStream str = new FramedStream( myStream );
                    str.PushFrame( entry.Offset, entry.Size );
                    TextureDictionarySectionData tdic = new Section( str ).Data as TextureDictionarySectionData;
                    foreach ( TextureNativeSectionData tex in tdic.Textures )
                        if( !myTextureDict.ContainsKey( tex.DiffuseName ) )
                            myTextureDict.Add( tex.DiffuseName, tex );
                }
            }

            public bool ContainsFile( String name )
            {
                return myFileDict.ContainsKey( name );
            }

            public bool ContainsTexture( String name )
            {
                return myTextureDict.ContainsKey( name );
            }

            public FramedStream ReadFile( String name )
            {
                ImageArchiveEntry entry = myFileDict[ name ];
                FramedStream stream = new FramedStream( myStream );
                stream.PushFrame( entry.Offset, entry.Size );
                return stream;
            }

            public Texture2D LoadTexture( String name )
            {
                return null;
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

        public static Texture2D LoadTexture( String name )
        {
            Resource<Texture2D> res = null;

            if ( !stLoadedTextures.ContainsKey( name ) )
            {
                foreach ( ImageArchive archive in stLoadedArchives )
                {
                    if ( archive.ContainsTexture( name ) )
                    {
                        res = new Resource<Texture2D>( archive.LoadTexture( name ) );
                        break;
                    }
                }

                if ( res == null )
                    throw new KeyNotFoundException( "File with name \"" + name + "\" not present in a loaded archive." );
            }
            else
                res = stLoadedTextures[ name ];

            ++res.Uses;

            return res.Value;
        }
    }
}

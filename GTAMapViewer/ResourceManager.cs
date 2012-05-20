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

                List<String> keys = new List<string>();
                keys.AddRange( myFileDict.Keys.Where( x => x.EndsWith( ".ipl" ) ) );
            }

            public bool ContainsFile( String name )
            {
                return myFileDict.ContainsKey( name );
            }

            public FramedStream ReadFile( String name )
            {
                ImageArchiveEntry entry = myFileDict[ name ];
                FramedStream stream = new FramedStream( myStream );
                stream.PushFrame( entry.Offset, entry.Size );
                return stream;
            }
        }

        private static List<ImageArchive> stLoadedArchives = new List<ImageArchive>();

        private static Dictionary<String, Resource<Model>> stLoadedModels
            = new Dictionary<string, Resource<Model>>();
        private static Dictionary<String, Resource<TextureDictionary>> stLoadedTexDicts
            = new Dictionary<string, Resource<TextureDictionary>>();

        public static void LoadArchive( String filePath )
        {
            stLoadedArchives.Add( ImageArchive.Load( filePath ) );
        }

        public static Model LoadModel( String name, String txdName )
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
                        break;
                    }
                }

                if ( res == null )
                    throw new KeyNotFoundException( "File with name \"" + name + "\" not present in a loaded archive." );

                TextureDictionary txd = LoadTextureDictionary( txdName );
                res.Value.LoadTextures( txd );

                stLoadedModels.Add( name, res );
            }
            else
                res = stLoadedModels[ name ];

            ++res.Uses;

            return res.Value;
        }

        public static void UnloadModel( String name, String txdName )
        {
            --stLoadedModels[ name ].Uses;
            UnloadTextureDictionary( txdName );
        }

        private static String GetTextureName( String name, String txdName, TextureType type )
        {
            return txdName + "_" + type.ToString()[ 0 ] + "_" + name;
        }

        public static TextureDictionary LoadTextureDictionary( String name )
        {
            name = name.ToLower();

            if ( !name.EndsWith( ".txd" ) )
                name += ".txd";

            Resource<TextureDictionary> res = null;

            if ( !stLoadedTexDicts.ContainsKey( name ) )
            {
                foreach ( ImageArchive archive in stLoadedArchives )
                {
                    if ( archive.ContainsFile( name ) )
                    {
                        res = new Resource<TextureDictionary>( new TextureDictionary( archive.ReadFile( name ) ) );
                        break;
                    }
                }

                if ( res == null )
                    throw new KeyNotFoundException( "File with name \"" + name + "\" not present in a loaded archive." );

                stLoadedTexDicts.Add( name, res );
            }
            else
                res = stLoadedTexDicts[ name ];

            ++res.Uses;

            return res.Value;
        }

        public static void UnloadTextureDictionary( String name )
        {
            --stLoadedTexDicts[ name ].Uses;
        }
    }
}

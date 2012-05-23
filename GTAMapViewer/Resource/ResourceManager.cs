using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;

using GTAMapViewer.Graphics;
using GTAMapViewer.World;

namespace GTAMapViewer.Resource
{
    internal enum TextureType : byte
    {
        Diffuse = 1,
        Mask = 2
    }

    internal static class ResourceManager
    {
        private const double ResourceDisposeDelay = 10.0;

        private enum JobType : byte
        {
            Load        = 0, Unload        = 1,
            LoadModel   = 2, UnloadModel   = 3,
            LoadTexDict = 4, UnloadTexDict = 5
        }

        private class Job
        {
            public readonly JobType Type;
            public readonly Object[] Args;

            public Job( JobType type, params Object[] args )
            {
                Type = type;
                Args = args;
            }
        }

        private class Resource<T>
        {
            private int myUses;
            private DateTime myLastUsedTime;

            public readonly T Value;
            public int Uses
            {
                get { return myUses; }
                set
                {
                    if ( myUses != value )
                    {
                        myUses = value;
                        if ( value == 0 )
                            myLastUsedTime = DateTime.Now;
                    }
                }
            }

            public bool Used { get { return Uses > 0; } }
            public DateTime LastUsedTime
            {
                get
                {
                    if ( Used )
                        return DateTime.Now;

                    return myLastUsedTime;
                }
            }

            public Resource( T resource )
            {
                Value = resource;
                Uses = 0;
            }
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
        private static LinkedList<Resource<Model>> stUnusedModels
            = new LinkedList<Resource<Model>>();
        private static Dictionary<String, Resource<TextureDictionary>> stLoadedTexDicts
            = new Dictionary<string, Resource<TextureDictionary>>();
        private static LinkedList<Resource<TextureDictionary>> stUnusedTexDicts
            = new LinkedList<Resource<TextureDictionary>>();

        private static Stopwatch stModelTimer = new Stopwatch();
        private static Stopwatch stTexTimer = new Stopwatch();

        private static Thread stManagerThread;
        private static Queue<Job> stThreadJobs;
        private static bool stStopThread;

        private static Queue<VertexBuffer> stVBDisposals = new Queue<VertexBuffer>();
        private static Queue<TextureDictionary> stTDDisposals = new Queue<TextureDictionary>();

        public static double ModelLoadTime
        {
            get { return stModelTimer.Elapsed.TotalSeconds; }
        }
        public static double TextureLoadTime
        {
            get { return stTexTimer.Elapsed.TotalSeconds; }
        }

        public static void LoadArchive( String filePath )
        {
            stLoadedArchives.Add( ImageArchive.Load( filePath ) );
        }

        public static void StartThread()
        {
            if ( stManagerThread != null && stManagerThread.IsAlive )
                return;

            stStopThread = false;
            stThreadJobs = new Queue<Job>();
            stManagerThread = new Thread( ThreadEntry );
            stManagerThread.Start();
        }

        public static void StopThread()
        {
            stStopThread = true;
            stManagerThread = null;
        }

        private static void ThreadEntry()
        {
            while ( !stStopThread )
            {
                Job job = null;
                lock ( stThreadJobs )
                    if ( stThreadJobs.Count > 0 )
                        job = stThreadJobs.Dequeue();

                if ( job != null )
                {
                    ObjectDefinition targ;
                    switch ( job.Type )
                    {
                        case JobType.LoadModel:
                            targ = job.Args[ 0 ] as ObjectDefinition;
                            targ.Model = LoadModel( targ.ModelName.ToLower(), targ.TextureDictName );
                            break;
                        case JobType.UnloadModel:
                            targ = job.Args[ 0 ] as ObjectDefinition;
                            UnloadModel( targ.ModelName.ToLower() );
                            break;
                    }
                }
                else
                {
                    CheckUnusedResources();
                    Thread.Sleep( 33 );
                }
            }
        }

        public static bool FileExists( String name )
        {
            foreach ( ImageArchive arch in stLoadedArchives )
                if ( arch.ContainsFile( name ) )
                    return true;

            return false;
        }

        public static FramedStream ReadFile( String name )
        {
            foreach ( ImageArchive arch in stLoadedArchives )
                if ( arch.ContainsFile( name ) )
                    return arch.ReadFile( name );

            return null;
        }

        private static void CheckUnusedResources()
        {
            DateTime now = DateTime.Now;
            while ( stUnusedModels.Count > 0 )
            {
                Resource<Model> res = stUnusedModels.First.Value;
                if ( res.Used )
                {
                    stUnusedModels.RemoveFirst();
                    continue;
                }

                if ( ( now - res.LastUsedTime ).Seconds < ResourceDisposeDelay )
                    break;

                res.Value.Dispose();

                stLoadedModels.Remove( res.Value.Name );
                stUnusedModels.RemoveFirst();
            }

            while ( stUnusedTexDicts.Count > 0 )
            {
                Resource<TextureDictionary> res = stUnusedTexDicts.First.Value;
                if ( res.Used )
                {
                    stUnusedTexDicts.RemoveFirst();
                    continue;
                }

                if ( ( now - res.LastUsedTime ).Seconds < ResourceDisposeDelay )
                    break;

                DisposeTextureDictionary( res.Value );

                stLoadedTexDicts.Remove( res.Value.Name );
                stUnusedTexDicts.RemoveFirst();
            }
        }

        public static void CheckGLDisposals()
        {
            lock ( stVBDisposals )
                while ( stVBDisposals.Count > 0 )
                    stVBDisposals.Dequeue().Dispose();

            lock ( stTDDisposals )
                while ( stTDDisposals.Count > 0 )
                    stTDDisposals.Dequeue().Dispose();
        }

        public static void DisposeVertexBuffer( VertexBuffer vb )
        {
            lock ( stVBDisposals )
                stVBDisposals.Enqueue( vb );
        }

        public static void DisposeTextureDictionary( TextureDictionary txd )
        {
            lock ( stTDDisposals )
                stTDDisposals.Enqueue( txd );
        }

        public static void UnloadAll()
        {
            foreach ( Resource<Model> model in stLoadedModels.Values )
                model.Value.Dispose();

            foreach ( Resource<TextureDictionary> txd in stLoadedTexDicts.Values )
                txd.Value.Dispose();

            stLoadedModels.Clear();
            stLoadedTexDicts.Clear();

            stUnusedModels.Clear();
            stUnusedTexDicts.Clear();
        }

        public static void RequestModel( ObjectDefinition target )
        {
            lock ( stThreadJobs )
                stThreadJobs.Enqueue( new Job( JobType.LoadModel, target ) );
        }

        private static Model LoadModel( String name, String txdName )
        {
            String fileName = name + ".dff";

            Resource<Model> res = null;

            if ( !stLoadedModels.ContainsKey( name ) )
            {
                foreach ( ImageArchive archive in stLoadedArchives )
                {
                    if ( archive.ContainsFile( fileName ) )
                    {
                        stModelTimer.Start();
                        res = new Resource<Model>( new Model( name, archive.ReadFile( fileName ) ) );
                        stModelTimer.Stop();
                        break;
                    }
                }

                if ( res == null )
                    throw new KeyNotFoundException( "File with name \"" + fileName + "\" not present in a loaded archive." );

                res.Value.LoadTextures( txdName );
                stLoadedModels.Add( name, res );
            }
            else
            {
                res = stLoadedModels[ name ];

                if ( !res.Used )
                    stUnusedModels.Remove( res );
            }

            ++res.Uses;

            return res.Value;
        }

        public static void UnloadModel( ObjectDefinition target )
        {
            lock ( stThreadJobs )
                stThreadJobs.Enqueue( new Job( JobType.UnloadModel, target ) );
        }

        private static void UnloadModel( String name )
        {
            Resource<Model> res = stLoadedModels[ name ];
            --res.Uses;

            if ( res.Uses < 0 )
                throw new Exception( "You done messed up" );

            if ( !res.Used )
                stUnusedModels.AddLast( res );
        }

        public static TextureDictionary LoadTextureDictionary( String name )
        {
            if ( stManagerThread == null || Thread.CurrentThread == stManagerThread )
            {
                name = name.ToLower();
                String fileName = name + ".txd";

                Resource<TextureDictionary> res = null;

                if ( !stLoadedTexDicts.ContainsKey( name ) )
                {
                    foreach ( ImageArchive archive in stLoadedArchives )
                    {
                        if ( archive.ContainsFile( fileName ) )
                        {
                            stTexTimer.Start();
                            res = new Resource<TextureDictionary>( new TextureDictionary( name, archive.ReadFile( fileName ) ) );
                            stTexTimer.Stop();
                            break;
                        }
                    }

                    if ( res == null )
                        throw new KeyNotFoundException( "File with name \"" + fileName + "\" not present in a loaded archive." );

                    stLoadedTexDicts.Add( name, res );
                }
                else
                {
                    res = stLoadedTexDicts[ name ];

                    if ( !res.Used )
                        stUnusedTexDicts.Remove( res );
                }

                ++res.Uses;

                return res.Value;
            }
            else
                throw new Exception( "Texture dictionaries can only be loaded from the resource manager thread." );
        }

        public static void UnloadTextureDictionary( String name )
        {
            if ( stManagerThread == null || Thread.CurrentThread == stManagerThread )
            {
                name = name.ToLower();
                Resource<TextureDictionary> res = stLoadedTexDicts[ name ];
                --res.Uses;

                if ( res.Uses < 0 )
                    throw new Exception( "You done messed up" );

                if ( !res.Used )
                    stUnusedTexDicts.AddLast( res );
            }
            else
                throw new Exception( "Texture dictionaries can only be unloaded from the resource manager thread." );
        }
    }
}

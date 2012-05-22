using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;

using GTAMapViewer.Resource;

namespace GTAMapViewer.World
{
    internal static class ItemManager
    {
        private enum DefType : byte
        {
            None = 0,
            Objs = 1,
            TObj = 2,
            Anim = 3
        }

        private enum PlaceType : byte
        {
            None = 0,
            Inst = 1
        }

        private static SortedDictionary<UInt32, ObjectDefinition> stObjects
            = new SortedDictionary<uint, ObjectDefinition>();

        private static List<Cell> stCells;

        public static void LoadDefinitionFiles( String dirPath )
        {
            foreach ( String file in Directory.GetFiles( dirPath, "*.ide" ) )
                LoadDefinitionFile( file );

            foreach ( String dir in Directory.GetDirectories( dirPath ) )
                LoadDefinitionFiles( dir );
        }

        public static void LoadDefinitionFile( String filePath )
        {
            using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                StreamReader reader = new StreamReader( stream );
                DefType curType = DefType.None;
                while ( !reader.EndOfStream )
                {
                    String line = reader.ReadLine().Trim();
                    if ( line.Length == 0 || line.StartsWith( "#" ) )
                        continue;

                    if ( curType == DefType.None )
                    {
                        if ( line.Length == 4 )
                        {
                            switch ( line.ToLower() )
                            {
                                case "objs":
                                    curType = DefType.Objs; break;
                                case "tobj":
                                    curType = DefType.TObj; break;
                                case "anim":
                                    curType = DefType.Anim; break;
                            }
                        }
                    }
                    else
                    {
                        if ( line.ToLower() == "end" )
                            curType = DefType.None;
                        else
                        {
                            String[] split = line.Split( ',' );
                            for ( int i = 0; i < split.Length; ++i )
                                split[ i ] = split[ i ].Trim();

                            uint id;

                            switch ( curType )
                            {
                                case DefType.Objs:
                                case DefType.TObj:
                                    id = uint.Parse( split[ 0 ] );
                                    if ( stObjects.ContainsKey( id ) )
                                        break;

                                    if ( split.Length == 6 || split.Length == 8 )
                                        stObjects.Add( id, new ObjectDefinition(
                                            split[ 1 ],
                                            split[ 2 ],
                                            float.Parse( split[ 4 ] ),
                                            (ObjectFlag) uint.Parse( split[ 5 ] )
                                        ) );
                                    else if ( split.Length == 5 )
                                        stObjects.Add( id, new ObjectDefinition(
                                            split[ 1 ],
                                            split[ 2 ],
                                            float.Parse( split[ 3 ] ),
                                            (ObjectFlag) uint.Parse( split[ 4 ] )
                                        ) );
                                    break;
                                case DefType.Anim:
                                    id = uint.Parse( split[ 0 ] );
                                    if ( stObjects.ContainsKey( id ) )
                                        break;

                                    stObjects.Add( id, new ObjectDefinition(
                                        split[ 1 ],
                                        split[ 2 ],
                                        float.Parse( split[ 4 ] ),
                                        (ObjectFlag) uint.Parse( split[ 5 ] )
                                    ) );
                                    break;

                            }
                        }
                    }
                }
            }
        }

        public static void LoadGameFile( String filePath )
        {
            stCells = new List<Cell>();
            stCells.Add( new Exterior( 0 ) );

            using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                StreamReader reader = new StreamReader( stream );
                while ( !reader.EndOfStream )
                {
                    String line = reader.ReadLine().Trim();
                    if ( line.Length == 0 || line.StartsWith( "#" ) )
                        continue;

                    if ( line.StartsWith( "IMG" ) )
                    {
                        String path = line.Substring( 3 ).Trim();
                        ResourceManager.LoadArchive( path );
                    }
                    else if ( line.StartsWith( "IDE" ) )
                    {
                        String path = line.Substring( 3 ).Trim();
                        ItemManager.LoadDefinitionFile( path );
                    }
                    else if ( line.StartsWith( "IPL" ) )
                    {
                        String path = line.Substring( 3 ).Trim();
                        ItemManager.LoadPlacementFile( path );
                    }
                }
            }

            foreach ( Cell cell in stCells )
                cell.FinalisePlacements();
        }

        public static void LoadPlacementFile( String filePath )
        {
            List<InstPlacement> newPlacements = new List<InstPlacement>();

            using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                StreamReader reader = new StreamReader( stream );
                PlaceType curType = PlaceType.None;

                while ( !reader.EndOfStream )
                {
                    String line = reader.ReadLine().Trim();
                    if ( line.Length == 0 || line.StartsWith( "#" ) )
                        continue;

                    if ( curType == PlaceType.None )
                    {
                        if ( line.Length == 4 )
                        {
                            switch ( line.ToLower() )
                            {
                                case "inst":
                                    curType = PlaceType.Inst;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if ( line.ToLower() == "end" )
                        {
                            curType = PlaceType.None;
                        }
                        else
                        {
                            String[] split = line.Split( ',' );
                            for ( int i = 0; i < split.Length; ++i )
                                split[ i ] = split[ i ].Trim();

                            switch ( curType )
                            {
                                case PlaceType.Inst:
                                    newPlacements.Add( new InstPlacement( split ) );
                                    break;
                            }
                        }
                    }
                }
            }

            String namePrefix = Path.GetFileNameWithoutExtension( filePath ).ToLower() + "_stream";
            for ( int i = 0; i < 100; ++i )
            {
                String name = namePrefix + i.ToString() + ".ipl";
                FramedStream stream = ResourceManager.ReadFile( name );

                if ( stream == null )
                    break;

                BinaryReader reader = new BinaryReader( stream );

                String ident = reader.ReadString( 4 );
                if ( ident != "bnry" )
                    break;

                int instCount = reader.ReadInt32();
                stream.Seek( 12, SeekOrigin.Current );
                int carsCount = reader.ReadInt32();
                stream.Seek( 4, SeekOrigin.Current );
                int instOffset = reader.ReadInt32();
                stream.Seek( 28, SeekOrigin.Current );
                int carsOffset = reader.ReadInt32();
                stream.Seek( instOffset, SeekOrigin.Begin );

                for ( int j = 0; j < instCount; ++j )
                    newPlacements.Add( new InstPlacement( stream ) );
            }

            for ( int i = 0; i < newPlacements.Count; ++i )
                newPlacements[ i ].FindLODPlacement( newPlacements );

            foreach ( InstPlacement p in newPlacements.Where( x => !x.IsLOD && x.Object != null &&
                ( x.CellID == 0 || x.CellID == 13 || x.CellID > 18 ) ) )
            {
                stCells[ 0 ].AddPlacement( p );
            }
        }

        public static ObjectDefinition GetObject( UInt32 id )
        {
            if( stObjects.ContainsKey( id ) )
                return stObjects[ id ];

            return null;
        }

        public static Cell GetCell( int id )
        {
            return stCells[ id ];
        }
    }
}

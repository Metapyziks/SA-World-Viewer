using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;

namespace GTAMapViewer.Items
{
    internal static class ItemManager
    {
        private enum DefType : byte
        {
            None = 0,
            Objs = 1,
            TObj = 2
        }

        private enum PlaceType : byte
        {
            None = 0,
            Inst = 1
        }

        private class InstPlacement
        {
            public readonly UInt32 ID;
            public readonly String Modelname;
            public readonly Int32 Interior;
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly Int32 LODIndex;

            public InstPlacement( String[] args )
            {
                ID = uint.Parse( args[ 0 ] );
                Modelname = args[ 1 ];
                Interior = int.Parse( args[ 2 ] );
                float posX = -float.Parse( args[ 3 ] );
                float posZ = float.Parse( args[ 4 ] );
                float posY = float.Parse( args[ 5 ] );
                Position = new Vector3( posX, posY, posZ );
                float rotX = -float.Parse( args[ 6 ] );
                float rotZ = float.Parse( args[ 7 ] );
                float rotY = float.Parse( args[ 8 ] );
                float rotW = float.Parse( args[ 9 ] );
                Rotation = new Quaternion( rotX, rotY, rotZ, rotW );
                LODIndex = int.Parse( args[ 10 ] );
            }

            public void Place( Instance inst, Instance[] batch )
            {
                inst.Place( Position, Rotation, ( LODIndex != -1 ? batch[ LODIndex ] : null ) );
            }
        }

        private static SortedDictionary<UInt32, ObjectDefinition> stObjects
            = new SortedDictionary<uint, ObjectDefinition>();

        // TODO: Write some sort of spacially partitioned structure
        //       to store instances for fast searching
        private static List<Instance> stInstances = new List<Instance>();

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
                                    id = uint.Parse( split[ 0 ] );
                                    if ( stObjects.ContainsKey( id ) )
                                        break;

                                    if ( split.Length == 6 )
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
                            }
                        }
                    }
                }
            }
        }

        public static void LoadPlacementFile( String filePath )
        {
            using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                StreamReader reader = new StreamReader( stream );
                PlaceType curType = PlaceType.None;

                List<InstPlacement> newPlacements = new List<InstPlacement>();

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
                                    newPlacements = new List<InstPlacement>();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if ( line.ToLower() == "end" )
                        {
                            switch ( curType )
                            {
                                case PlaceType.Inst:
                                    Instance[] newInsts = new Instance[ newPlacements.Count ];
                                    for ( int i = 0; i < newInsts.Length; ++i )
                                        newInsts[ i ] = new Instance( newPlacements[ i ].ID );
                                    for ( int i = 0; i < newInsts.Length; ++i )
                                        newPlacements[ i ].Place( newInsts[ i ], newInsts );
                                    stInstances.AddRange( newInsts.Where( x => !x.IsLOD && x.Object != null ) );
                                    break;
                            }
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
        }

        public static ObjectDefinition GetObject( UInt32 id )
        {
            if( stObjects.ContainsKey( id ) )
                return stObjects[ id ];

            return null;
        }

        public static ICollection<Instance> GetInstances()
        {
            return stInstances;
        }
    }
}

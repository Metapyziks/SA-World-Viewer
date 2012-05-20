using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Items
{
    internal static class ItemManager
    {
        private static SortedDictionary<UInt32, ObjectDefinition> stObjects
            = new SortedDictionary<uint, ObjectDefinition>();

        private enum DefType : byte
        {
            None = 0,
            Objs = 1,
            TObj = 2,
            Path = 3,
            TDFX = 4,
            Anim = 5,
            TXDP = 6
        }

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
                            switch ( line )
                            {
                                case "objs":
                                    curType = DefType.Objs; break;
                                case "tobj":
                                    curType = DefType.TObj; break;
                                case "path":
                                    curType = DefType.Path; break;
                                case "2dfx":
                                    curType = DefType.TDFX; break;
                                case "anim":
                                    curType = DefType.Anim; break;
                                case "txdp":
                                    curType = DefType.TXDP; break;
                            }
                        }
                    }
                    else
                    {
                        if ( line == "end" )
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
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}

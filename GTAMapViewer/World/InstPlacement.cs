using System;
using System.Collections.Generic;
using System.IO;

using OpenTK;

namespace GTAMapViewer.World
{
    internal class InstPlacement
    {
        public readonly UInt32 ObjectID;
        public readonly ObjectDefinition Object;
        public readonly String Modelname;
        public readonly UInt16 CellID;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Int32 LODIndex;

        public bool HasLOD { get { return LODIndex != -1; } }
        public InstPlacement LODPlacement { get; private set; }
        public bool IsLOD { get; private set; }

        public InstPlacement( String[] args )
        {
            ObjectID = uint.Parse( args[ 0 ] );
            Object = ItemManager.GetObject( ObjectID );
            Modelname = args[ 1 ];
            CellID = (UInt16) int.Parse( args[ 2 ] );
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
            IsLOD = false;
        }

        public InstPlacement( FramedStream stream )
        {
            BinaryReader reader = new BinaryReader( stream );

            float posX = -reader.ReadSingle();
            float posZ = reader.ReadSingle();
            float posY = reader.ReadSingle();
            Position = new Vector3( posX, posY, posZ );
            float rotX = -reader.ReadSingle();
            float rotZ = reader.ReadSingle();
            float rotY = reader.ReadSingle();
            float rotW = reader.ReadSingle();
            Rotation = new Quaternion( rotX, rotY, rotZ, rotW );
            ObjectID = reader.ReadUInt32();
            Object = ItemManager.GetObject( ObjectID );
            CellID = (UInt16) reader.ReadInt32();
            LODIndex = reader.ReadInt32();
            IsLOD = false;
        }

        public void FindLODPlacement( List<InstPlacement> batch )
        {
            if ( HasLOD )
            {
                LODPlacement = batch[ LODIndex ];
                LODPlacement.IsLOD = true;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;

namespace GTAMapViewer.DFF
{
    internal enum GeometryFlag : ushort
    {
        TexCoords = 4,
        Colors = 8,
        Normals = 16
    }

    internal struct FaceInfo
    {
        public readonly GeometryFlag Flags;
        public readonly UInt16 Vertex0;
        public readonly UInt16 Vertex1;
        public readonly UInt16 Vertex2;

        public FaceInfo( BinaryReader reader )
        {
            Vertex1 = reader.ReadUInt16();
            Vertex0 = reader.ReadUInt16();
            Flags = (GeometryFlag) reader.ReadUInt16();
            Vertex2 = reader.ReadUInt16();
        }
    }

    internal struct BoundingSphere
    {
        public readonly Vector3 Offset;
        public readonly float Radius;

        public BoundingSphere( BinaryReader reader )
        {
            Offset = reader.ReadVector3();
            Radius = reader.ReadSingle();
        }
    }

    [SectionType( SectionType.Geometry )]
    internal class GeometrySectionData : SectionData
    {
        public readonly GeometryFlag Flags;
        public readonly UInt32 FaceCount;
        public readonly UInt32 VertexCount;
        public readonly UInt32 FrameCount;

        public readonly float Ambient;
        public readonly float Diffuse;
        public readonly float Specular;

        public readonly UInt32[] Colours;
        public readonly Vector2[] TexCoords;
        public readonly FaceInfo[] Faces;

        public readonly BoundingSphere BoundingSphere;

        public readonly UInt32 HasPosition;
        public readonly UInt32 HasNormals;

        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;

        public GeometrySectionData( SectionHeader header, FramedStream stream )
        {
            SectionHeader dataHeader = new SectionHeader( stream );
            BinaryReader reader = new BinaryReader( stream );
            
            Flags = (GeometryFlag) reader.ReadUInt16();
            reader.ReadUInt16(); // Unknown
            FaceCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();
            FrameCount = reader.ReadUInt32();

            if ( dataHeader.Version == 4099 )
            {
                Ambient = reader.ReadSingle();
                Diffuse = reader.ReadSingle();
                Specular = reader.ReadSingle();
            }

            if ( ( Flags & GeometryFlag.Colors ) != 0 )
            {
                Colours = new UInt32[ VertexCount ];
                for ( int i = 0; i < VertexCount; ++i )
                    Colours[ i ] = reader.ReadUInt32();
            }

            if ( ( Flags & GeometryFlag.TexCoords ) != 0 )
            {
                TexCoords = new Vector2[ VertexCount ];
                for ( int i = 0; i < VertexCount; ++i )
                    TexCoords[ i ] = reader.ReadVector2();
            }

            Faces = new FaceInfo[ FaceCount ];
            for ( int i = 0; i < FaceCount; ++i )
                Faces[ i ] = new FaceInfo( reader );

            BoundingSphere = new BoundingSphere( reader );

            HasPosition = reader.ReadUInt32();
            HasNormals = reader.ReadUInt32();

            if ( HasPosition > 1 || HasNormals > 1 )
                throw new Exception( "Well there you go" );

            Vertices = new Vector3[ VertexCount ];
            for ( int i = 0; i < VertexCount; ++i )
                Vertices[ i ] = reader.ReadVector3();

            if ( ( Flags & GeometryFlag.Normals ) != 0 )
            {
                Normals = new Vector3[ VertexCount ];
                for ( int i = 0; i < VertexCount; ++i )
                    Normals[ i ] = reader.ReadVector3();
            }
        }

        public float[] GetVertices()
        {
            float[] data = new float[ VertexCount * 5 ];
            for ( int i = 0; i < VertexCount; ++i )
            {
                data[ i * 5 ] = Vertices[ i ].X;
                data[ i * 5 + 1 ] = Vertices[ i ].Y;
                data[ i * 5 + 2 ] = Vertices[ i ].Z;
                if ( TexCoords != null )
                {
                    data[ i * 5 + 3 ] = TexCoords[ i ].X;
                    data[ i * 5 + 4 ] = TexCoords[ i ].Y;
                }
            }
            return data;
        }

        public ushort[] GetIndices()
        {
            ushort[] data = new ushort[ FaceCount * 3 ];
            for ( int i = 0; i < FaceCount; ++i )
            {
                data[ i * 3 ] = Faces[ i ].Vertex0;
                data[ i * 3 + 1 ] = Faces[ i ].Vertex1;
                data[ i * 3 + 2 ] = Faces[ i ].Vertex2;
            }
            return data;
        }
    }
}

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.Resource
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

        public readonly Color4[] Colours;
        public readonly Vector2[] TexCoords;
        public readonly FaceInfo[] Faces;

        public readonly BoundingSphere BoundingSphere;

        public readonly UInt32 HasPosition;
        public readonly UInt32 HasNormals;

        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;

        public readonly MaterialSectionData[] Materials;
        public readonly MaterialSplit[] MaterialSplits;
        public readonly UInt16 IndexCount;

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
                Colours = new Color4[ VertexCount ];
                for ( int i = 0; i < VertexCount; ++i )
                {
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();
                    byte a = reader.ReadByte();
                    Colours[ i ] = new Color4( r, g, b, a );
                }
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

            Materials = ( new Section( stream ).Data as MaterialListSectionData ).Materials;

            SectionHeader extHeader = new SectionHeader( stream );
            MaterialSplitSectionData msplits = new Section( stream ).Data as MaterialSplitSectionData;
            MaterialSplits = msplits.MaterialSplits;
            FaceCount = msplits.FaceCount;
            IndexCount = msplits.IndexCount;

            foreach ( MaterialSplit mat in MaterialSplits )
                mat.Material = Materials[ mat.MaterialIndex ];
        }

        public float[] GetVertices()
        {
            int s = 9;
            float[] data = new float[ VertexCount * s ];
            for ( int i = 0; i < VertexCount; ++i )
            {
                int o = i * s;
                data[ o ] = Vertices[ i ].X;
                data[ o + 1 ] = Vertices[ i ].Y;
                data[ o + 2 ] = Vertices[ i ].Z;
                if ( TexCoords != null )
                {
                    data[ o + 3 ] = TexCoords[ i ].X;
                    data[ o + 4 ] = TexCoords[ i ].Y;
                }
                if ( Colours != null )
                {
                    data[ o + 5 ] = Colours[ i ].R;
                    data[ o + 6 ] = Colours[ i ].G;
                    data[ o + 7 ] = Colours[ i ].B;
                    data[ o + 8 ] = Colours[ i ].A;
                }
            }
            return data;
        }

        public ushort[] GetIndices()
        {
            ushort[] data = new ushort[ IndexCount ];
            int i = 0;
            foreach ( MaterialSplit split in MaterialSplits )
            {
                UInt16[] indices = split.FaceIndices;
                for ( int j = 0; j < indices.Length; ++j )
                    data[ i++ ] = indices[ j ];
            }
            return data;
        }

        public void LoadTextures( TextureDictionary txd )
        {
            foreach ( MaterialSectionData mat in Materials )
                mat.LoadTextures( txd );
        }
    }
}

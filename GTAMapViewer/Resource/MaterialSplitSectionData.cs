using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Resource
{
    internal class MaterialSplit
    {
        public readonly UInt16 Offset;
        public readonly UInt16 VertexCount;
        public readonly UInt16 MaterialIndex;
        public readonly UInt16[] FaceIndices;

        public MaterialSplit( UInt16 offset, FramedStream stream )
        {
            Offset = offset;
            BinaryReader reader = new BinaryReader( stream );
            VertexCount = (UInt16) reader.ReadUInt32();
            FaceIndices = new UInt16[ VertexCount + 1 ];
            MaterialIndex = (UInt16) reader.ReadUInt32();

            for ( int i = 0; i < VertexCount; ++i )
                FaceIndices[ i ] = (UInt16) reader.ReadUInt32();

            FaceIndices[ VertexCount++ ] = 0xffff;
        }
    }

    [SectionType( SectionType.MaterialSplit )]
    internal class MaterialSplitSectionData : SectionData
    {
        public bool TriangleStrip;
        public UInt32 SplitCount;
        public UInt32 FaceCount;
        public UInt16 IndexCount;
        public MaterialSplit[] MaterialSplits;

        public MaterialSplitSectionData( SectionHeader header, FramedStream stream )
        {
            BinaryReader reader = new BinaryReader( stream );

            TriangleStrip = reader.ReadUInt32() == 1;
            SplitCount = reader.ReadUInt32();
            MaterialSplits = new MaterialSplit[ SplitCount ];
            FaceCount = reader.ReadUInt32();

            IndexCount = 0;
            for ( UInt16 i = 0; i < SplitCount; ++i )
            {
                MaterialSplits[ i ] = new MaterialSplit( IndexCount, stream );
                IndexCount += MaterialSplits[ i ].VertexCount;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.MaterialList )]
    internal class MaterialListSectionData : SectionData
    {
        public readonly UInt32 MaterialCount;
        public readonly MaterialSectionData[] Materials;

        public MaterialListSectionData( SectionHeader header, FramedStream stream )
        {
            Section data = new Section( stream );
            MaterialCount = BitConverter.ToUInt32( ( data.Data as DataSectionData ).Value, 0 );
            Materials = new MaterialSectionData[ MaterialCount ];

            for ( int i = 0; i < MaterialCount; ++i )
                Materials[ i ] = new Section( stream ).Data as MaterialSectionData;
        }
    }
}

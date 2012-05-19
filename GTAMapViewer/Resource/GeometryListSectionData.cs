using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.GeometryList )]
    internal class GeometryListSectionData : SectionData
    {
        public readonly UInt32 GeometryCount;
        public readonly GeometrySectionData[] Geometry;

        public GeometryListSectionData( SectionHeader header, FramedStream stream )
        {
            DataSectionData data = (DataSectionData) new Section( stream ).Data;
            GeometryCount = BitConverter.ToUInt32( data.Value, 0 );
            Geometry = new GeometrySectionData[ GeometryCount ];

            for( int i = 0; i < GeometryCount; ++ i )
                Geometry[ i ] = (GeometrySectionData) new Section( stream ).Data;
        }
    }
}

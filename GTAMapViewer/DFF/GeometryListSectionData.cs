using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAMapViewer.DFF
{
    [SectionType( SectionType.GeometryList )]
    internal class GeometryListSectionData : SectionData
    {
        public readonly UInt32 GeometryCount;
        public readonly GeometrySectionData[] Geometry;

        public GeometryListSectionData( SectionHeader header, FramedStream stream )
        {
            int curGeo = 0;

            while ( stream.CanRead )
            {
                Section section = new Section( stream );
                switch ( section.Type )
                {
                    case SectionType.Data:
                        DataSectionData data = (DataSectionData) section.Data;
                        GeometryCount = BitConverter.ToUInt32( data.Data, 0 );
                        Geometry = new GeometrySectionData[ GeometryCount ];
                        curGeo = 0;
                        break;
                    case SectionType.Geometry:
                        Geometry[ curGeo++ ] = (GeometrySectionData) section.Data;
                        break;
                    case SectionType.Null:
                        return;
                    default:
                        throw new UnexpectedSectionTypeException( SectionType.GeometryList, section.Type );
                }
            }
        }
    }
}

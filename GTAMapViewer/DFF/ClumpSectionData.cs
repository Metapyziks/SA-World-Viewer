using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.DFF
{
    [SectionType( SectionType.Clump )]
    internal class ClumpSectionData : SectionData
    {
        public readonly UInt32 ObjectCount;
        public readonly GeometryListSectionData GeometryList;

        public ClumpSectionData( SectionHeader header, FramedStream stream )
        {
            while ( stream.CanRead )
            {
                Section section = new Section( stream );
                switch ( section.Type )
                {
                    case SectionType.Data:
                        var data = (DataSectionData) section.Data;
                        ObjectCount = BitConverter.ToUInt32( data.Data, 0 );
                        break;
                    case SectionType.FrameList:
                        break;
                    case SectionType.GeometryList:
                        GeometryList = (GeometryListSectionData) section.Data;
                        break;
                    case SectionType.Atomic:
                        break;
                    case SectionType.Extension:
                        break;
                    case SectionType.Null:
                        return;
                    default:
                        throw new UnexpectedSectionTypeException( SectionType.Clump, section.Type );
                }
            }
        }
    }
}

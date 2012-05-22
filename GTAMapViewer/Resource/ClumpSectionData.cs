using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.Clump )]
    internal class ClumpSectionData : SectionData
    {
        public readonly UInt32 ObjectCount;
        public readonly GeometryListSectionData GeometryList;

        public ClumpSectionData( SectionHeader header, FramedStream stream )
        {
            DataSectionData dat = (DataSectionData) new Section( stream ).Data;
            if ( dat == null )
                return;

            ObjectCount = BitConverter.ToUInt32( dat.Value, 0 );
            var frameList = new Section( stream );
            GeometryList = (GeometryListSectionData) new Section( stream ).Data;
        }
    }
}

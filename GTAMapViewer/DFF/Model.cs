using System.Collections.Generic;
using System.IO;

namespace GTAMapViewer.DFF
{
    public class Model
    {
        private ClumpSectionData[] myClumps;

        public Model( FramedStream stream )
        {
            List<ClumpSectionData> clumps = new List<ClumpSectionData>();
            while ( stream.CanRead )
            {
                SectionHeader header = new SectionHeader( stream );
                if ( header.Type == SectionType.Clump )
                {
                    ClumpSectionData data = SectionData.FromStream<ClumpSectionData>( header, stream );
                    clumps.Add( data );
                }
            }
            myClumps = clumps.ToArray();
        }
    }
}

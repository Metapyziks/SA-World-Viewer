using System;
using System.Collections.Generic;
using System.IO;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.DFF
{
    internal class Model : IDisposable
    {
        private ClumpSectionData[] myClumps;

        public VertexBuffer VertexBuffer { get; private set; }

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

            VertexBuffer = new VertexBuffer( 5 );
            GeometrySectionData geo = myClumps[ 0 ].GeometryList.Geometry[ 0 ];
            VertexBuffer.SetData( geo.GetVertices(), geo.GetIndices() );
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
        }
    }
}

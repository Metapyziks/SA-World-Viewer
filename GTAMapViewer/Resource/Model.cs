using System;
using System.Collections.Generic;
using System.IO;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.Resource
{
    internal class Model : IDisposable
    {
        private GeometrySectionData[] myGeometry;

        public VertexBuffer VertexBuffer { get; private set; }

        public Model( FramedStream stream )
        {
            List<GeometrySectionData> geos = new List<GeometrySectionData>();
            while ( stream.CanRead )
            {
                SectionHeader header = new SectionHeader( stream );
                if ( header.Type == SectionType.Clump )
                {
                    ClumpSectionData data = SectionData.FromStream<ClumpSectionData>( header, stream );
                    geos.AddRange( data.GeometryList.Geometry );
                }
            }
            myGeometry = geos.ToArray();

            VertexBuffer = new VertexBuffer( 5 );
            GeometrySectionData geo = myGeometry[ 0 ];
            VertexBuffer.SetData( geo.GetVertices(), geo.GetIndices() );
        }

        public void LoadAdditionalResources()
        {
            foreach ( GeometrySectionData geo in myGeometry )
                geo.LoadAdditionalResources();
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
        }
    }
}

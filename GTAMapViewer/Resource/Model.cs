using System;
using System.Collections.Generic;
using System.IO;

using OpenTK.Graphics.OpenGL;

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

        public void Render( ModelShader shader )
        {
            if ( !VertexBuffer.DataSet )
                return;

            foreach ( GeometrySectionData geo in myGeometry )
            {
                foreach ( MaterialSplit mat in geo.MaterialSplits )
                {
                    shader.SetTexture( "tex", geo.Materials[ mat.MaterialIndex ].Textures[ 0 ].Texture );
                    GL.DrawElements( BeginMode.TriangleStrip, mat.VertexCount, DrawElementsType.UnsignedShort, mat.Offset );
                }
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
        }
    }
}

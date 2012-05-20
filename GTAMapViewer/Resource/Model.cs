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

        public int MaterialNumber;

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

            VertexBuffer = new VertexBuffer( 9 );
            GeometrySectionData geo = myGeometry[ 0 ];
            VertexBuffer.SetData( geo.GetVertices(), geo.GetIndices() );

            MaterialNumber = geo.MaterialSplits.Length;
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

            int i = 0;
            GeometrySectionData geo = myGeometry[ 0 ];
            int count = geo.MaterialSplits.Length;
            int mno = MaterialNumber % ( count + 1 );
            foreach ( MaterialSplit mat in geo.MaterialSplits )
            {
                if ( i++ == mno || mno == count )
                {
                    if ( mat.Material.TextureCount > 0 )
                    {
                        shader.SetTexture( "tex", mat.Material.Textures[ 0 ].Texture );
                        GL.DrawElements( BeginMode.TriangleStrip, mat.VertexCount, DrawElementsType.UnsignedShort, mat.Offset * sizeof( UInt16 ) );
                    }
                }
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
        }
    }
}

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

            VertexBuffer = new VertexBuffer( 9 );
            if ( myGeometry.Length > 0 )
            {
                GeometrySectionData geo = myGeometry[ 0 ];
                VertexBuffer.SetData( geo.GetVertices(), geo.GetIndices() );
            }
        }

        public void LoadTextures( TextureDictionary txd )
        {
            foreach ( GeometrySectionData geo in myGeometry )
                geo.LoadTextures( txd );
        }

        public void Render( ModelShader shader )
        {
            if ( !VertexBuffer.DataSet )
                return;

            GeometrySectionData geo = myGeometry[ 0 ];
            foreach ( MaterialSplit mat in geo.MaterialSplits )
            {
                if ( mat.Material.TextureCount > 0 )
                {
                    TextureSectionData tex = mat.Material.Textures[ 0 ];
                    if ( tex.Texture != null )
                        shader.SetTexture( "tex_diffuse", tex.Texture );
                    if ( tex.Mask != null )
                    {
                        shader.SetTexture( "tex_mask", tex.Mask );
                        //shader.AlphaMask = true;
                    }
                    else
                        shader.AlphaMask = false;

                    shader.Colour = mat.Material.Colour;
                    GL.DrawElements( BeginMode.TriangleStrip, mat.VertexCount, DrawElementsType.UnsignedShort, mat.Offset * sizeof( UInt16 ) );
                }
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
        }
    }
}

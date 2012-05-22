using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.Resource
{
    internal class Model : IDisposable
    {
        private GeometrySectionData[] myGeometry;

        public readonly String Name;

        public TextureDictionary TextureDict { get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }

        public Model( String name, FramedStream stream )
        {
            Name = name;

            List<GeometrySectionData> geos = new List<GeometrySectionData>();
            while ( stream.CanRead )
            {
                SectionHeader header = new SectionHeader( stream );
                if ( header.Type == SectionType.Clump )
                {
                    ClumpSectionData data = SectionData.FromStream<ClumpSectionData>( header, stream );
                    if( data.GeometryList != null )
                        geos.AddRange( data.GeometryList.Geometry );
                }
                break;
            }
            myGeometry = geos.ToArray();

            VertexBuffer = new VertexBuffer( 9 );
            if ( myGeometry.Length > 0 )
            {
                GeometrySectionData geo = myGeometry[ 0 ];
                VertexBuffer.SetData( geo.GetVertices(), geo.GetIndices() );
            }
        }

        public void LoadTextures( String txdName )
        {
            TextureDict = ResourceManager.LoadTextureDictionary( txdName );

            foreach ( GeometrySectionData geo in myGeometry )
                geo.LoadTextures( TextureDict );
        }

        public void UnloadTextures()
        {
            ResourceManager.UnloadTextureDictionary( TextureDict.Name );
            TextureDict = null;
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
            ResourceManager.UnloadTextureDictionary( TextureDict.Name );
            VertexBuffer.Dispose();
        }
    }
}

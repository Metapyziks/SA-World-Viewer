using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace GTAMapViewer.Graphics
{
    internal class VertexBuffer : IDisposable
    {
        private int myStride;

        private bool myDataSet = false;

        private int myVertVboID;
        private int myIndVboID;

        private int myLength;

        private int VertVboID
        {
            get
            {
                if ( myVertVboID == 0 )
                    GL.GenBuffers( 1, out myVertVboID );

                return myVertVboID;
            }
        }
        private int IndVboID
        {
            get
            {
                if ( myIndVboID == 0 )
                    GL.GenBuffers( 1, out myIndVboID );

                return myIndVboID;
            }
        }

        public VertexBuffer( int stride )
        {
            myStride = stride;
        }

        public void SetData<T0, T1>( T0[] vertices, T1[] indices )
            where T0 : struct
            where T1 : struct
        {
            myLength = indices.Length;

            GL.BindBuffer( BufferTarget.ArrayBuffer, VertVboID );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, IndVboID );
            GL.BufferData( BufferTarget.ArrayBuffer, new IntPtr( vertices.Length * Marshal.SizeOf( typeof( T0 ) ) ), vertices, BufferUsageHint.StaticDraw );
            GL.BufferData( BufferTarget.ElementArrayBuffer, new IntPtr( indices.Length * Marshal.SizeOf( typeof( T1 ) ) ), indices, BufferUsageHint.StaticDraw );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );

            CheckForError();

            myDataSet = true;
        }

        private void CheckForError()
        {
            ErrorCode error = GL.GetError();

            if ( error != ErrorCode.NoError )
                throw new Exception( "OpenGL hates your guts: " + error.ToString() );
        }

        public void StartBatch( ShaderProgram shader )
        {
            GL.BindBuffer( BufferTarget.ArrayBuffer, VertVboID );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, IndVboID );

            foreach ( AttributeInfo info in shader.Attributes )
            {
                GL.VertexAttribPointer( info.Location, info.Size, info.PointerType,
                    info.Normalize, shader.VertexDataStride, info.Offset );
                GL.EnableVertexAttribArray( info.Location );
            }
        }

        public void Render( ShaderProgram shader, int first = 0, int count = -1 )
        {
            if ( myDataSet )
            {
                if ( count == -1 )
                    count = myLength - first;

                GL.DrawElements( shader.BeginMode, count, DrawElementsType.UnsignedShort, first );
            }
        }

        public void EndBatch( ShaderProgram shader )
        {
            foreach ( AttributeInfo info in shader.Attributes )
                GL.DisableVertexAttribArray( info.Location );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
        }

        public void Dispose()
        {
            if ( myDataSet )
            {
                GL.DeleteBuffers( 1, ref myVertVboID );
                GL.DeleteBuffers( 1, ref myIndVboID );
            }

            myDataSet = false;
        }
    }
}
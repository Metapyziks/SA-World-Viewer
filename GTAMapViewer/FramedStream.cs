using System;
using System.Collections.Generic;
using System.IO;

namespace GTAMapViewer
{
    public class FramedStream : Stream
    {
        private struct Frame
        {
            public readonly long Offset;
            public readonly long Size;

            public Frame( long offset, long size )
            {
                Offset = offset;
                Size = size;
            }

            public override string ToString()
            {
                return String.Format( "{0}, {1}", Offset, Size );
            }
        }

        private Stream myStream;
        private Frame myBaseFrame;
        private Stack<Frame> myFrameStack;

        public FramedStream( Stream baseStream )
        {
            if ( baseStream is FramedStream )
                throw new Exception( "Wrapping a FramedStream around a FramedStream? No." );

            myStream = baseStream;
            myBaseFrame = new Frame( 0, myStream.Length );
            myFrameStack = new Stack<Frame>();
            Position = 0;
        }

        public void PushFrame( long size )
        {
            myFrameStack.Push( new Frame( myStream.Position, size ) );
        }

        public void PushFrame( long offset, long size )
        {
            myFrameStack.Push( new Frame( offset + CurrentFrame.Offset, size ) );
            Position = 0;
        }

        public void PopFrame()
        {
            Position = CurrentFrame.Size;
            myFrameStack.Pop();
        }

        private Frame CurrentFrame
        {
            get
            {
                if ( myFrameStack.Count == 0 )
                    return myBaseFrame;
                return myFrameStack.Peek();
            }
        }

        public override bool CanRead
        {
            get { return myStream.CanRead && ( myStream.Position - CurrentFrame.Offset ) < CurrentFrame.Size; }
        }

        public override bool CanSeek
        {
            get { return myStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return CurrentFrame.Size; }
        }

        public override long Position
        {
            get
            {
                return myStream.Position - CurrentFrame.Offset;
            }
            set
            {
                myStream.Position = value + CurrentFrame.Offset;
            }
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            return myStream.Read( buffer, offset, count );
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            switch ( origin )
            {
                case SeekOrigin.Begin:
                    return myStream.Seek( offset + CurrentFrame.Offset, SeekOrigin.Begin );
                case SeekOrigin.Current:
                    return myStream.Seek( offset, SeekOrigin.Current );
                case SeekOrigin.End:
                    return myStream.Seek( offset + CurrentFrame.Offset + CurrentFrame.Size, SeekOrigin.End );
            }

            return 0;
        }

        public override void SetLength( long value )
        {
            throw new NotImplementedException();
        }

        public override void Write( byte[] buffer, int offset, int count )
        {
            throw new NotImplementedException();
        }
    }
}

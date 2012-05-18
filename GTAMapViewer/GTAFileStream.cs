using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer
{
    public class GTAFileStream : Stream
    {
        private Stream myStream;
        private UInt32 myOffset;
        private UInt32 mySize;

        public GTAFileStream( Stream fileStream, UInt32 offset, UInt32 size )
        {
            myStream = fileStream;
            myOffset = offset;
            mySize = size;
        }

        public override bool CanRead
        {
            get { return myStream.CanRead && ( myStream.Position - myOffset ) < mySize; }
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
            get { return mySize; }
        }

        public override long Position
        {
            get
            {
                return myStream.Position - myOffset;
            }
            set
            {
                myStream.Position = value + myOffset;
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
                    return myStream.Seek( offset + myOffset, SeekOrigin.Begin );
                case SeekOrigin.Current:
                    return myStream.Seek( offset, SeekOrigin.Current );
                case SeekOrigin.End:
                    return myStream.Seek( offset + myOffset + mySize, SeekOrigin.End );
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

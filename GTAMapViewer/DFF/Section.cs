using System;
using System.IO;

namespace GTAMapViewer.DFF
{
    internal struct Section
    {
        public readonly SectionHeader Header;
        public readonly SectionData Data;

        public SectionType Type { get { return Header.Type; } }

        public Section( FramedStream stream )
        {
            stream.PushFrame( 12 );
            Header = new SectionHeader( stream );
            stream.PopFrame();
            stream.PushFrame( Header.Size );
            Data = SectionData.FromStream( Header,  stream );
            stream.PopFrame();
        }

        public override string ToString()
        {
            return Header.ToString();
        }
    }
}

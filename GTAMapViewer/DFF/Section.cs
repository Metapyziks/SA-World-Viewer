using System.IO;

namespace GTAMapViewer.DFF
{
    internal struct Section
    {
        public readonly SectionHeader Header;
        public readonly SectionData Data;

        public Section( Stream stream )
        {
            Header = new SectionHeader( stream );
            Data = SectionData.FromStream( Header, stream );
        }

        public Section( SectionHeader header, SectionData data )
        {
            Header = header;
            Data = data;
        }
    }
}

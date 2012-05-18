using System.IO;

namespace GTAMapViewer.DFF
{
    [SectionType( SectionType.Data )]
    internal class DataSectionData : SectionData
    {
        public readonly byte[] Data;

        public DataSectionData( SectionHeader header, FramedStream stream )
        {
            Data = new byte[ header.Size ];
            stream.Read( Data, 0, (int) header.Size );
        }
    }
}

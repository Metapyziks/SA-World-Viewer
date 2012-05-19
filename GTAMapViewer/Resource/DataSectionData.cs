using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.Data )]
    internal class DataSectionData : SectionData
    {
        public readonly byte[] Value;

        public DataSectionData( SectionHeader header, FramedStream stream )
        {
            Value = new byte[ header.Size ];
            stream.Read( Value, 0, (int) header.Size );
        }
    }
}

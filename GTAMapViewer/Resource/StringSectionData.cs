using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.Resource
{
    [SectionType( SectionType.String )]
    internal class StringSectionData : SectionData
    {
        public readonly String Value;

        public StringSectionData( SectionHeader header, FramedStream stream )
        {
            Value = UnicodeEncoding.UTF8.GetString( stream.ReadBytes( (int) header.Size ) ).TrimNullChars();
        }
    }
}

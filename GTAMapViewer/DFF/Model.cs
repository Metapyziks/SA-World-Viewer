using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAMapViewer.DFF
{
    public class Model
    {
        private Section[] mySections;

        public Model( Stream stream )
        {
            List<Section> sections = new List<Section>();
            while ( stream.CanRead )
                sections.Add( new Section( stream ) );
            mySections = sections.ToArray();
        }
    }
}

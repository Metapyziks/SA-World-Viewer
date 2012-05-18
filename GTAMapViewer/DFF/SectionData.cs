using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace GTAMapViewer.DFF
{
    internal class SectionTypeAttribute : Attribute
    {
        public readonly SectionType Value;

        public SectionTypeAttribute( SectionType value )
        {
            Value = value;
        }
    }

    internal abstract class SectionData
    {
        private static Dictionary<SectionType, Type> myDataTypes;

        private static void FindTypes()
        {
            myDataTypes = new Dictionary<SectionType, Type>();
            foreach ( Type t in Assembly.GetExecutingAssembly().GetTypes() )
            {
                if ( t.BaseType == typeof( SectionData ) )
                {
                    object[] attribs = t.GetCustomAttributes( false );
                    foreach ( object attrib in attribs )
                    {
                        if ( attrib is SectionTypeAttribute )
                        {
                            myDataTypes.Add( ( (SectionTypeAttribute) attrib ).Value, t );
                        }
                    }
                }
            }
        }

        public static SectionData FromStream( SectionHeader header, Stream stream )
        {
            if ( myDataTypes == null )
                FindTypes();

            SectionData data = null;
            long newPos = stream.Position + header.Size;

            if ( myDataTypes.ContainsKey( header.Type ) )
            {
                Type t = myDataTypes[ header.Type ];
                ConstructorInfo cons = t.GetConstructor( new Type[] { typeof( SectionHeader ), typeof( Stream ) } );
                if ( cons != null )
                    data = (SectionData) cons.Invoke( new object[] { header, stream } );
            }

            stream.Position = newPos;
            return null;
        }

        public SectionData( SectionHeader header, Stream stream )
        {

        }
    }
}

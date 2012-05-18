using System;
using System.Collections.Generic;
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

        public static SectionData FromStream( SectionHeader header, FramedStream stream )
        {
            return FromStream<SectionData>( header, stream );
        }

        public static T FromStream<T>( SectionHeader header, FramedStream stream )
            where T : SectionData
        {
            if ( myDataTypes == null )
                FindTypes();

            T data = null;
            if ( myDataTypes.ContainsKey( header.Type ) )
            {
                Type t = myDataTypes[ header.Type ];
                ConstructorInfo cons = t.GetConstructor( new Type[] { typeof( SectionHeader ), typeof( FramedStream ) } );
                if ( cons != null )
                {
                    try
                    {
                        data = (T) cons.Invoke( new object[] { header, stream } );
                    }
                    catch ( TargetInvocationException e )
                    {
                        throw e.InnerException;
                    }
                }
            }
            return data;
        }

        public SectionData()
        {

        }
    }
}

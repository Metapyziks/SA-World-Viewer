using System;

namespace GTAMapViewer.Resource
{
    internal class UnexpectedSectionTypeException : Exception
    {
        public readonly SectionType ParentType;
        public readonly SectionType Type;

        public UnexpectedSectionTypeException( SectionType type )
            : this( SectionType.Null, type ) { }

        public UnexpectedSectionTypeException( SectionType parentType, SectionType type )
            : base( String.Format( "Unexpected section type {0} encountered in type {1}", type, parentType ) )
        {
            ParentType = parentType;
            Type = type;
        }
    }
}

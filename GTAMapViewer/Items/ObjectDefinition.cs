using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAMapViewer.Items
{
    internal class ObjectDefinition
    {
        public readonly String ModelName;
        public readonly String TextureDictName;

        public readonly float DrawDist;
        public readonly ObjectFlag Flags;

        public ObjectDefinition( String model, String txd, float drawDist, ObjectFlag flags )
        {
            ModelName = model;
            TextureDictName = txd;
            DrawDist = drawDist;
            Flags = flags;
        }

        public bool HasFlags( ObjectFlag flag )
        {
            return ( Flags & flag ) == flag;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;

namespace GTAMapViewer
{
    internal static class Tools
    {
        internal static Vector2 ReadVector2( this BinaryReader reader )
        {
            return new Vector2
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle()
            };
        }

        internal static Vector3 ReadVector3( this BinaryReader reader )
        {
            return new Vector3
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };
        }
    }
}

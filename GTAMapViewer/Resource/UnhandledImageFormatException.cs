using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAMapViewer.Resource
{
    internal class UnhandledImageFormatException : Exception
    {
        public UnhandledImageFormatException( TextureNativeSectionData.CompressionMode compression,
            TextureNativeSectionData.RasterFormat format )
            : base( "Unhandled image format encountered: " +
                ( compression != TextureNativeSectionData.CompressionMode.None ?
                    compression.ToString() + "-" : "" ) + format.ToString() )
        {

        }
    }
}

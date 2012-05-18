using System;

using GTAMapViewer.IMG;
using GTAMapViewer.DFF;

namespace GTAMapViewer
{
    class Program
    {
        static void Main( string[] args )
        {
            ImageArchive arch = ImageArchive.Load( args[ 0 ] );
            Console.WriteLine( "Archive Version: {0}", arch.Version );
            Model bridge_1 = new Model( arch.ReadFile( "bridge_1.dff" ) );
            Console.ReadKey();
        }
    }
}

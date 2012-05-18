using System;

using GTAMapViewer.IMG;
using GTAMapViewer.DFF;

namespace GTAMapViewer
{
    internal class Program
    {
        static void Main( string[] args )
        {
            ImageArchive arch = ImageArchive.Load( args[ 0 ] );
            Console.WriteLine( "Archive Version: {0}", arch.Version );
            ViewerWindow window = new ViewerWindow( arch );
            window.Run();
            window.Dispose();
        }
    }
}

using System;

using GTAMapViewer.Resource;

namespace GTAMapViewer
{
    internal class Program
    {
        static void Main( string[] args )
        {
            if ( args.Length == 0 )
                return;

            foreach ( String arg in args )
                ResourceManager.LoadArchive( arg );

            ViewerWindow window = new ViewerWindow();
            window.Run();
            window.Dispose();
        }
    }
}

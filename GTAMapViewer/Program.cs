using System;
using System.IO;

using GTAMapViewer.Resource;
using GTAMapViewer.Items;

namespace GTAMapViewer
{
    internal class Program
    {
        static void Main( string[] args )
        {
            if ( args.Length > 0 )
                Directory.SetCurrentDirectory( args[ 0 ] );

            char sep = Path.DirectorySeparatorChar;

            String modelPath = "models" + sep;
            String dataPath = "data" + sep;

            try
            {
                ResourceManager.LoadArchive( modelPath + "gta3.img" );
                ResourceManager.LoadArchive( modelPath + "gta_int.img" );
                ResourceManager.LoadArchive( modelPath + "player.img" );

                ItemManager.LoadGameFile( dataPath + "gta.dat" );
            }
            catch ( FileNotFoundException )
            {
                return;
            }

            ViewerWindow window = new ViewerWindow();
            window.Run();
            window.Dispose();
        }
    }
}

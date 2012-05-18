using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAMapViewer
{
    class Program
    {
        static void Main( string[] args )
        {
            ImageArchive arch = ImageArchive.Load( args[ 0 ] );
            Console.WriteLine( arch.Version );
            Console.ReadKey();
        }
    }
}

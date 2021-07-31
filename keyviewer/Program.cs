using System;
using Gdk;
using Gtk;

namespace keyviewer
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            new MainWindow();
            Application.Run();
        }
    }
}

using System;
using System.IO;

namespace UlasimRotaPlanlama
{
    internal static class main
    {

        static void DosyaOkuma()
        {
            string writeText = "hello bedirhan";
            File.WriteAllText("bedirhan.txt" , writeText);

            string readText;
            readText = File.ReadAllText("bedirhan.txt");

            Console.WriteLine(readText);
        }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("agmþe");
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

        }
    }
}
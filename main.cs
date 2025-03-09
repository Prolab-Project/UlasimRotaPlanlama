using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac.Otobus;

namespace UlasimRotaPlanlama
{
    internal static class main
    {

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("agmþe");
            //ApplicationConfiguration.Initialize();
           // Application.Run(new Form1());

            double openingFee,costPerKm;
             
            string DosyaOku;
            DosyaOku = File.ReadAllText("Dataset/bedirhan.json");

            using JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            JsonElement taxi = root.GetProperty("taxi");
                
            openingFee = taxi.GetProperty("openingFee").GetDouble();
            costPerKm = taxi.GetProperty("costPerKm").GetDouble();

            Console.WriteLine(costPerKm + "," + openingFee);

            bus_otogar bus  = new bus_otogar();
            bus.KonumBilgisi(); 
        }
    }
}
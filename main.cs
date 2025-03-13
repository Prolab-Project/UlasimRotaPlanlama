using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac.Otobus;
using UlasimRotaPlanlama.Models.Arac;

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

            //Console.WriteLine(costPerKm + "," + openingFee);

            bus_otogar bus  = new bus_otogar();
            //bus.DurakBilgisi();

            double lat_konum = Convert.ToDouble(Console.ReadLine());
            double lon_konum = Convert.ToDouble(Console.ReadLine());
            
            Taksi taksi = new Taksi();
            taksi.MesafeHesaplama(lat_konum , lon_konum , 29.94533 , 40.78317);
            Console.WriteLine(taksi.taksi_bin);
            taksi.UcretHesapla();
            
        }
    }
}
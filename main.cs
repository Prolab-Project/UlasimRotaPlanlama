using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac.Otobus;
using UlasimRotaPlanlama.Models.Arac;

namespace UlasimRotaPlanlama
{
   /* {
            string DosyaOku;
    DosyaOku = File.ReadAllText("dataset/bedirhan.json");

            using JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

    id = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("id").GetString();
    name = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("name").GetString();
    type = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("type").GetString();
    sonDurak = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("sonDurak").GetBoolean();
    lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("lat").GetDouble();
    lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("lon").GetDouble();

    Console.WriteLine("id: " + id);
            Console.WriteLine("name: " + name);
            Console.WriteLine("type: " + type);
            Console.WriteLine("son Durak: " + sonDurak);
            Console.WriteLine("lat: " + lat);
            Console.WriteLine("lon: " + lon);*/
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

            double lat_konum = Convert.ToDouble(Console.ReadLine());
            double lon_konum = Convert.ToDouble(Console.ReadLine());
            
            Taksi taksi = new Taksi();
            taksi.MesafeHesaplama(40.75359, 29.95328 , 40.78259, 29.94628);
            Console.WriteLine(taksi.taksi_bin);
            taksi.UcretHesapla();
            taksi.DurakBilgisi();


            //Otobus kngof = new Otobus();
            
        }
    }
}
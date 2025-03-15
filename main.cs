using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac;

namespace UlasimRotaPlanlama
{
    internal static class main
    {

        static string JsonCekme(int x)
        {
            string json = File.ReadAllText("dataset/bedirhan.json");

            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            double lat;
            double lon;
            string id;
            bool sondurak;
            string type;

            lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("lat").GetDouble();
            lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("lon").GetDouble();
            id = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("id").GetString();
            sondurak = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("sonDurak").GetBoolean();
            type = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("type").GetString();

            string data = $"{lat} , {lon} , {id} , {sondurak} , {type}";

            return data;
        }

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

            //double lat_konum = Convert.ToDouble(Console.ReadLine());
            //double lon_konum = Convert.ToDouble(Console.ReadLine());
            
            Taksi taksi = new Taksi();
            //taksi.MesafeHesaplama(40.75359, 29.95328, 40.78259, 29.94628);
            //taksi.UcretHesapla();
            //taksi.DurakBilgisi();

            string data = JsonCekme(1);
            Console.WriteLine(data);
        }
    }
}
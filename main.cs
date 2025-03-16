using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac;
using System.Collections;
using System.Xml.Linq;
using UlasimRotaPlanlama.Models.Yolcu;

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
            string name;
            
            lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("lat").GetDouble();
            lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("lon").GetDouble();
            id = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("id").GetString();
            sondurak = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("sonDurak").GetBoolean();
            type = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("type").GetString();
            name = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("name").GetString();

            string data = $"{lat} / {lon} / {id} / {sondurak} / {type} / {name}";

            return data;
        }

        public static Otobus OtobusOlustur(string data)
        {
            string id;
            string name;
            string type;
            double lat;
            double lon;
            bool sonDurak;

            string[] value = data.Split("/");

            lat = Convert.ToDouble(value[0]);
            lon = Convert.ToDouble(value[1]);
            id = value[2];
            sonDurak = Convert.ToBoolean(value[3]);
            type = value[4];
            name = value[5];

            return new Otobus { id = id, name = name, type = type, lat = lat, lon = lon, sonDurak = sonDurak };
        }

        public static Tramvay TramvayOlustur(string data)
        {
            string id;
            string name;
            string type;
            double lat;
            double lon;
            bool sonDurak;

            string[] value = data.Split("/");

            lat = Convert.ToDouble(value[0]);
            lon = Convert.ToDouble(value[1]);
            id = value[2];
            sonDurak = Convert.ToBoolean(value[3]);
            type = value[4];
            name = value[5];
            
            return new Tramvay { id = id, name = name, type = type, lat = lat, lon = lon, sonDurak = sonDurak };
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

            ArrayList BusData = new ArrayList();
            BusData.Add(JsonCekme(0));
            BusData.Add(JsonCekme(1));
            BusData.Add(JsonCekme(2));
            BusData.Add(JsonCekme(3));
            BusData.Add(JsonCekme(4));
            BusData.Add(JsonCekme(5));

            ArrayList TramvayData = new ArrayList();
            TramvayData.Add(JsonCekme(6));
            TramvayData.Add(JsonCekme(7));
            TramvayData.Add(JsonCekme(8));
            TramvayData.Add(JsonCekme(9));

            foreach (var elements in BusData)
            {
                Console.WriteLine(elements);
            }

            foreach(var elements in TramvayData)
            {
                Console.WriteLine(elements);   
            }

            Otobus BusOtogar = OtobusOlustur(BusData[0].ToString());
            double lon = BusOtogar.lon;
            Console.WriteLine(lon);

            Otobus BusSekapark = OtobusOlustur(BusData[1].ToString());
            double lon1 = BusSekapark.lon;
            Console.WriteLine(lon1);

            Otobus BusYahyakaptan = OtobusOlustur(BusData[2].ToString());
            double lon2 = BusYahyakaptan.lon;
            Console.WriteLine(lon2);

            Otobus BusUmuttepe = OtobusOlustur(BusData[3].ToString());
            double lon3 = BusUmuttepe.lon;
            Console.WriteLine(lon3);

            Otobus BusSymbolavm = OtobusOlustur(BusData[4].ToString());
            double lon4 = BusSymbolavm.lon;
            Console.WriteLine(lon4);

            Otobus Bus41Burada = OtobusOlustur(BusData[5].ToString());
            double lon5 = Bus41Burada.lon;
            Console.WriteLine(lon5);

            Tramvay TramOtogar = TramvayOlustur(TramvayData[0].ToString());
        }
    }
}
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
        public static double MesafeHesapla(double lat, double lon, double durak_lat, double durak_lon)
        {
            double derece_lat = Math.Abs(lat - durak_lat);
            double metre_lat = derece_lat * 111320;

            double derece_lon = Math.Abs(lon - durak_lon);
            double metre_lon = derece_lon * 85170;

            double pisagor_lat = metre_lat * metre_lat;
            double pisagor_lon = metre_lon * metre_lon;

            double mesafe = Math.Sqrt(pisagor_lon + pisagor_lat);


            return mesafe; // Metre cinsinden mesafe
        }
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

        public static void EnYakinDuragiBul(List<Arac> Durak, Taksi taksi)
        {
            Console.Write(" enlem (lat): ");
            double lat_konum = Convert.ToDouble(Console.ReadLine());

            Console.Write(" boylam (lon): ");
            double lon_konum = Convert.ToDouble(Console.ReadLine());

            double minMesafe = double.MaxValue;
            Arac enYakinDurak = null;

            for (int i = 0; i < Durak.Count; i++)
            {
                Console.WriteLine($"Durak {i}: " + Durak[i].name);
            }

            foreach (var durak in Durak)
            {
                double mesafe = MesafeHesapla(lat_konum, lon_konum, durak.lat, durak.lon);

                if (mesafe < minMesafe)
                {
                    minMesafe = mesafe;
                    enYakinDurak = durak;
                }
            }

            if (enYakinDurak != null)
            {
                Console.WriteLine($"En yakýn duragi: {enYakinDurak.name}, {minMesafe:F2} metre uzaklýkta.");
                if (minMesafe > 3000)
                {
                    Console.WriteLine("Mesafe 3 km'den fazla, taksi kullanmanýz önerilir.");
                    taksi.MesafeHesaplama(lat_konum, lon_konum, enYakinDurak.lat, enYakinDurak.lon);
                    taksi.UcretHesapla();
                }
            }
            else
            {
                Console.WriteLine("Yakýnlarda otobüs duraðý bulunamadý.");
            }
        }

            [STAThread]
        static void Main()
        {
            //ApplicationConfiguration.Initialize();
            // Application.Run(new Form1());

            double openingFee, costPerKm;

            string DosyaOku;
            DosyaOku = File.ReadAllText("Dataset/bedirhan.json");

            using JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            JsonElement taxi = root.GetProperty("taxi");

            openingFee = taxi.GetProperty("openingFee").GetDouble();
            costPerKm = taxi.GetProperty("costPerKm").GetDouble();

            Taksi taksi = new Taksi();
            
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

            foreach (var elements in TramvayData)
            {
                Console.WriteLine(elements);
            }


            List<Arac> otobusDuraklari = new List<Arac>();

            Otobus BusOtogar = OtobusOlustur(BusData[0].ToString());
            otobusDuraklari.Add(BusOtogar);
            
            Otobus BusSekapark = OtobusOlustur(BusData[1].ToString());
            otobusDuraklari.Add(BusSekapark);
            
            Otobus BusYahyakaptan = OtobusOlustur(BusData[2].ToString());
            otobusDuraklari.Add(BusYahyakaptan);
            
            Otobus BusUmuttepe = OtobusOlustur(BusData[3].ToString());
            otobusDuraklari.Add(BusUmuttepe);
            
            Otobus BusSymbolavm = OtobusOlustur(BusData[4].ToString());
            otobusDuraklari.Add(BusSymbolavm);
            
            Otobus Bus41Burada = OtobusOlustur(BusData[5].ToString());
            otobusDuraklari.Add(Bus41Burada);

            List<Arac> tramDuraklari = new List<Arac>();

            Tramvay TramOtogar = TramvayOlustur(TramvayData[0].ToString());
            tramDuraklari.Add(TramOtogar);
            
            Tramvay TramYahyakaptan = TramvayOlustur(TramvayData[1].ToString());
            tramDuraklari.Add(TramYahyakaptan);
            
            Tramvay TramSekapark = TramvayOlustur(TramvayData[2].ToString());
            tramDuraklari.Add(TramSekapark);
            
            Tramvay TramHalkevi = TramvayOlustur(TramvayData[3].ToString());
            tramDuraklari.Add(TramHalkevi);

            //EnYakinDuragiBul(otobusDuraklari, taksi);

            EnYakinDuragiBul(tramDuraklari, taksi); 
            
        }
    }
}
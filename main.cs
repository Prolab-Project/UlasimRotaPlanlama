using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac;
using System.Collections;
using System.Xml.Linq;
using UlasimRotaPlanlama.Models.Yolcu;
using UlasimRotaPlanlama.Models;

public class json
{
    public string JsonCekme(int x)
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
       
        List<string> nextStopsList = new List<string>();
        if (root.GetProperty("duraklar").EnumerateArray().ElementAt(x).TryGetProperty("nextStops", out JsonElement nextStops))
        {
            foreach (JsonElement stop in nextStops.EnumerateArray())
            {
                string stopId = stop.GetProperty("stopId").GetString();
                double mesafe = stop.GetProperty("mesafe").GetDouble();
                int sure = stop.GetProperty("sure").GetInt32();
                double ucret = stop.GetProperty("ucret").GetDouble();

                nextStopsList.Add($"{stopId}({mesafe}km, {sure}dk, {ucret}TL)");
            }
        }

        string nextStopsData = nextStopsList.Count > 0 ? string.Join(", ", nextStopsList) : "None";

        string data = $"{lat} / {lon} / {id} / {sondurak} / {type} / {name} / {nextStopsData}";

        return data;
    }
}

public class AracSinifOlustur
{
    public static Otobus OtobusOlustur(string data)
    {
        string[] value = data.Split("/");

        double lat = Convert.ToDouble(value[0]);
        double lon = Convert.ToDouble(value[1]);
        string id = value[2];
        bool sonDurak = Convert.ToBoolean(value[3]);
        string type = value[4];
        string name = value[5];
        List<string> nextStops = new List<string>();

        if (value.Length > 6)
        {
            nextStops = value[6].Split(", ").ToList();
        }

        return new Otobus { id = id, name = name, type = type, lat = lat, lon = lon, sonDurak = sonDurak, NextStops= nextStops };
    }

    public static Tramvay TramvayOlustur(string data)
    {
        string[] value = data.Split("/");

        double lat = Convert.ToDouble(value[0]);
        double lon = Convert.ToDouble(value[1]);
        string id = value[2];
        bool sonDurak = Convert.ToBoolean(value[3]);
        string type = value[4];
        string name = value[5];
        List<string> nextStops = new List<string>();

        if (value.Length > 6)
        {
            nextStops = value[6].Split(", ").ToList();
        }

        return new Tramvay { id = id, name = name, type = type, lat = lat, lon = lon, sonDurak = sonDurak, NextStops= nextStops };
    }
}

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


        public static void EnYakinDuragiBul(List<Arac> Durak, Taksi taksi)
        {
            Console.Write(" mevcut enlem (lat): ");
            double lat_konum = Convert.ToDouble(Console.ReadLine());

            Console.Write(" mevcut boylam (lon): ");
            double lon_konum = Convert.ToDouble(Console.ReadLine());

            Console.Write(" hedef enlem (lat): ");
            double hedef_lat = Convert.ToDouble(Console.ReadLine());

            Console.Write(" hedef boylam (lon): ");
            double hedef_lon = Convert.ToDouble(Console.ReadLine());



            double minMesafe = double.MaxValue;
            double minMesafeHedef= double.MaxValue;
            Arac enYakinDurak = null;
            Arac hedefEnYakinDurak = null; 

            for (int i = 0; i < Durak.Count; i++)
            {
                Console.WriteLine($"Durak {i}: " + Durak[i].name);
            }

            foreach (var durak in Durak)
            {
                double mesafe = MesafeHesapla(lat_konum, lon_konum, durak.lat, durak.lon);
                double mesafeHedef = MesafeHesapla(hedef_lat, hedef_lon, durak.lat, durak.lon);

                if (mesafe < minMesafe)
                {
                    minMesafe = mesafe;
                    enYakinDurak = durak;
                }

                if (mesafeHedef < minMesafeHedef )
                {
                    minMesafeHedef = mesafeHedef;
                    hedefEnYakinDurak = durak;
                }
            }


            if (enYakinDurak != null)
            {
                Console.WriteLine($"En yakýn baslangic duragi: {enYakinDurak.name}, {minMesafe:F2} metre uzaklýkta.");
                Console.WriteLine($"En yakýn baslangic duragi: {hedefEnYakinDurak.name}, {minMesafeHedef:F2} metre uzaklýkta.");

                if (minMesafe > 3000)
                {
                    Console.WriteLine("Mesafe 3 km'den fazla, taksi kullanmanýz önerilir.");
                    taksi.MesafeHesaplama(lat_konum, lon_konum, enYakinDurak.lat, enYakinDurak.lon);
                    taksi.UcretHesapla();
                }

                if (minMesafeHedef > 3000)
                {
                    Console.WriteLine("Hedefe Mesafe 3 km'den fazla, taksi kullanmanýz önerilir.");
                    taksi.MesafeHesaplama(hedef_lat, hedef_lon, hedefEnYakinDurak.lat, hedefEnYakinDurak.lon);
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

            Taksi taksi = new Taksi();

            json jsonc = new json();
            
            ArrayList BusData = new ArrayList();
            BusData.Add(jsonc.JsonCekme(0));
            BusData.Add(jsonc.JsonCekme(1));
            BusData.Add(jsonc.JsonCekme(2));
            BusData.Add(jsonc.JsonCekme(3));
            BusData.Add(jsonc.JsonCekme(4));
            BusData.Add(jsonc.JsonCekme(5));

            ArrayList TramvayData = new ArrayList();
            TramvayData.Add(jsonc.JsonCekme(6));
            TramvayData.Add(jsonc.JsonCekme(7));
            TramvayData.Add(jsonc.JsonCekme(8));
            TramvayData.Add(jsonc.JsonCekme(9));

            foreach (var elements in BusData)
            {
                Console.WriteLine(elements);

            }

            foreach (var elements in TramvayData)
            {
                Console.WriteLine(elements);
            }

            List<Arac> otobusDuraklari = new List<Arac>();

            Otobus BusOtogar = AracSinifOlustur.OtobusOlustur(BusData[0].ToString());
            otobusDuraklari.Add(BusOtogar);
            
            Otobus BusSekapark = AracSinifOlustur.OtobusOlustur(BusData[1].ToString());
            otobusDuraklari.Add(BusSekapark);
            
            Otobus BusYahyakaptan = AracSinifOlustur.OtobusOlustur(BusData[2].ToString());
            otobusDuraklari.Add(BusYahyakaptan);
            
            Otobus BusUmuttepe = AracSinifOlustur.OtobusOlustur(BusData[3].ToString());
            otobusDuraklari.Add(BusUmuttepe);
            
            Otobus BusSymbolavm = AracSinifOlustur.OtobusOlustur(BusData[4].ToString());
            otobusDuraklari.Add(BusSymbolavm);
            
            Otobus Bus41Burada = AracSinifOlustur.OtobusOlustur(BusData[5].ToString());
            otobusDuraklari.Add(Bus41Burada);

            List<Arac> tramDuraklari = new List<Arac>();

            Tramvay TramOtogar = AracSinifOlustur.TramvayOlustur(TramvayData[0].ToString());
            tramDuraklari.Add(TramOtogar);
            
            Tramvay TramYahyakaptan = AracSinifOlustur.TramvayOlustur(TramvayData[1].ToString());
            tramDuraklari.Add(TramYahyakaptan);
            
            Tramvay TramSekapark = AracSinifOlustur.TramvayOlustur(TramvayData[2].ToString());
            tramDuraklari.Add(TramSekapark);
            
            Tramvay TramHalkevi = AracSinifOlustur.TramvayOlustur(TramvayData[3].ToString());
            tramDuraklari.Add(TramHalkevi);

            //EnYakinDuragiBul(otobusDuraklari, taksi);

            // EnYakinDuragiBul(tramDuraklari, taksi); 


            foreach (var stop in BusYahyakaptan.NextStops)
            {
                Console.WriteLine(stop);
            }

            foreach (var stop in TramSekapark.NextStops)
            {
                Console.WriteLine(stop);
            }
        }
    }
}
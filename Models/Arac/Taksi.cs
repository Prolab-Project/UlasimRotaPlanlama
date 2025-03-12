using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json; 
namespace UlasimRotaPlanlama.Models.Arac
{
    internal class Taksi : Arac, Durak
    {
        public Taksi() { }
        public double openingFee;
        public double costPerKm;
        public bool taksi_bin;
        public void DurakBilgisi()
        {
            string DosyaOku;
            DosyaOku = File.ReadAllText("dataset/bedirhan.json");

            JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            JsonElement taxi = root.GetProperty("taxi");

            openingFee = taxi.GetProperty("openingFee").GetDouble();
            costPerKm = taxi.GetProperty("costPerKm").GetDouble();

            Console.WriteLine(openingFee + "," + costPerKm);
        }

        public void MesafeHesaplama(double lat , double lon , double durak_lon , double durak_lat)
        {
            double derece_lat = Math.Abs(lat - durak_lat);
            double metre_lat = derece_lat * 111320;

            double derece_lon = Math.Abs(lon - durak_lon);
            double metre_lon = derece_lon * 85170;

            double pisagor_lat = metre_lat * metre_lat;
            double pisagor_lon = metre_lon * metre_lon;

            double mesafe = Math.Sqrt(pisagor_lon + pisagor_lat);

            Console.WriteLine("mesafe hesaplanıyor:");
            Console.WriteLine(mesafe);
        
            if(mesafe / 1000 > 3)
            {
                taksi_bin = true;
            }
            else
            {
                taksi_bin = false;
            }
            
        }


        /*public override double UcretHesapla(double mevcut_lat, double mevcut_lon, double hedef_lat , double hedef_lon)
        {
            Console.WriteLine("Mevcut konum: " + mevcut_lat + mevcut_lon); 
            double mesafe = Haversine(mevcut_lat, mevcut_lon , hedef_lat, hedef_lon);
            Console.WriteLine ("Gidilmesi gereken mesafe " + mesafe);
            double ucret = openingFee + (costPerKm * mesafe);
            Console.WriteLine("Odenecek ucret : " + ucret);
            return ucret; 

            throw new NotImplementedException();
        }*/
    }
}

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
        public void KonumBilgisi()
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
        public override double UcretHesapla(double mevcut_lat, double mevcut_lon, double hedef_lat , double hedef_lon)
        {
            Console.WriteLine("Mevcut konum: " + mevcut_lat + mevcut_lon); 
            double mesafe = Haversine(mevcut_lat, mevcut_lon , hedef_lat, hedef_lon);
            Console.WriteLine ("Gidilmesi gereken mesafe " + mesafe);
            double ucret = openingFee + (costPerKm * mesafe);
            Console.WriteLine("Odenecek ucret : " + ucret);
            return ucret; 

            throw new NotImplementedException();
        }
    }
}

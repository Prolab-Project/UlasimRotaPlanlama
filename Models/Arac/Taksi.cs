using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Arac
{
    internal class Taksi : Arac
    {
        public Taksi() { }
        double acilisUcreti = 10;
        double kmUcreti = 4;
        public override double UcretHesapla(double mevcut_lat, double mevcut_lon, double hedef_lat , double hedef_lon)
        {
            Console.WriteLine("Mevcut konum: " + mevcut_lat + mevcut_lon); 
            double mesafe = Haversine(mevcut_lat, mevcut_lon , hedef_lat, hedef_lon);
            Console.WriteLine ("Gidilmesi gereken mesafe " + mesafe);
            double ucret = acilisUcreti + (kmUcreti * mesafe);
            Console.WriteLine("Odenecek ucret : " + ucret);
            return ucret; 

            throw new NotImplementedException();
        }
    }
}

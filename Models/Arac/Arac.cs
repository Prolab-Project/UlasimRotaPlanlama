using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Arac
{
    public abstract class Arac
    {
        public string id;
        public string name;
        public string type;
        public double lat;
        public double lon;
        public bool sonDurak;
        public List<NextStop> NextStops { get; set; } 


        /*public abstract double UcretHesapla(double mevcut_lat , double mevcut_lon , double hedef_lat, double hedef_lon);
        public static double Haversine(double mevcut_lat, double mevcut_lon, double hedef_lat, double hedef_lon)
        {

            double R = 6371.0;
            double dLat = ToRadians(hedef_lat - mevcut_lat);
            double dLon = ToRadians(hedef_lon - mevcut_lon);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(mevcut_lat)) * Math.Cos(ToRadians(hedef_lat)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = (double)(2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));

            return R * c; //Km cinsinden donduruyor.

        }
        private static double ToRadians(double derece)
        {
            return derece * ( Math.PI / 180 ); 
        }*/


    }
}

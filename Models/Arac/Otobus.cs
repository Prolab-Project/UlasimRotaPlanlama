using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Arac
{
    public class Otobus : Arac
    {
        public List<string> NextStops { get; set; } // Sadece otobüsler icin

        public Otobus()
        {
            id = "0";
            name = "Bilinmeyen";
            type = "Standart";
            lat = 0.0;
            lon = 0.0;
            sonDurak = false;
            NextStops = new List<string>(); 

        }

        public Otobus(string id, string name, string type, double lat, double lon, bool sonDurak)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.lat = lat;
            this.lon = lon;
            this.sonDurak = sonDurak;
            this.NextStops = NextStops ?? new List<string>(); 
        }
    }
}

using System;
using System.Collections.Generic;

namespace UlasimRotaPlanlama.Models.Arac
{
    public class Otobus : Arac
    {
        public List<NextStop> NextStops { get; set; }
        public Otobus()
        {
            id = "0";
            name = "Bilinmeyen";
            type = "Standart";
            lat = 0.0;
            lon = 0.0;
            sonDurak = false;
            NextStops = new List<NextStop>();
        }

        public Otobus(string id, string name, string type, double lat, double lon, bool sonDurak, List<NextStop> nextStops = null)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.lat = lat;
            this.lon = lon;
            this.sonDurak = sonDurak;
            this.NextStops = nextStops ?? new List<NextStop>(); // Eğer nextStops null ise boş liste oluştur
        }
    }
}

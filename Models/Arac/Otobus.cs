using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Arac
{
    class Otobus : Arac
    {
        public string id;
        public string name;
        public string type;
        public double lat;
        public double lon;
        public bool sonDurak;
    
        Otobus(string id, string name, string type, double lat, double lon, bool sonDurak)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.lat = lat;
            this.lon = lon;
            this.sonDurak = sonDurak;
        }
    }
}

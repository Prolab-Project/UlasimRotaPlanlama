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
        
    }
}
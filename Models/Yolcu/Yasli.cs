using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Yolcu
{
    internal class Yasli : Yolcu
    {
        public Yasli()
        {
            indirimOrani = 0.30f; // %30 indirim
        }
    }
}

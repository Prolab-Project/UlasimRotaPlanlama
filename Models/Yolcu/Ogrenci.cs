using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Yolcu
{
    internal class Ogrenci : Yolcu
    {
        public Ogrenci()
        {
            indirimOrani = 0.50f; // %50 indirim
        }
    }
}

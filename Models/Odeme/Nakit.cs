using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Odeme
{
    internal class Nakit : Odeme
    {
        public Nakit()
        {
            indirimOrani = 0; // Nakit ödemede indirim yok
            komisyonOrani = 0; // Nakit ödemede komisyon yok
        }

        public override double Hesapla(double toplamMaliyet)
        {
            return toplamMaliyet; // Nakit ödemede indirim yok
        }
    }
}

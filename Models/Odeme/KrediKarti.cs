using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Odeme
{
    internal class KrediKarti : Odeme
    {
        public KrediKarti()
        {
            indirimOrani = 0; // Kredi kartında indirim yok
            komisyonOrani = 0.015; // %1.5 komisyon
        }

        public override double Hesapla(double toplamMaliyet)
        {
            return toplamMaliyet * (1 + komisyonOrani); // %1.5 komisyon
        }
    }
}

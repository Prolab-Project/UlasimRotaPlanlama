using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Odeme
{
    internal class KentKart : Odeme
    {
        public KentKart()
        {
            indirimOrani = 0.20; // %20 indirim
            komisyonOrani = 0; // Kentkart ödemede komisyon yok
        }

        public override double Hesapla(double toplamMaliyet)
        {
            Console.WriteLine($"[DEBUG] `Hesapla()` metodu çağrıldı.");
            Console.WriteLine($"[DEBUG] toplamMaliyet: {toplamMaliyet}");
            Console.WriteLine($"[DEBUG] indirimOrani: {indirimOrani}");

            double hesaplananUcret = toplamMaliyet * (1 - indirimOrani);

            Console.WriteLine($"[DEBUG] hesaplananUcret: {hesaplananUcret}");

            return hesaplananUcret;
        }

    }
}

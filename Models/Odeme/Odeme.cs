using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Odeme
{
    abstract class Odeme
    {
        protected double indirimOrani; // İndirim oranı
        protected double komisyonOrani; // Komisyon oranı

        public abstract double Hesapla(double toplamMaliyet); // Toplam maliyeti alır ve ödenecek tutarı döner

        public double GetIndirimOrani()
        {
            return indirimOrani;
        }

        public double GetKomisyonOrani()
        {
            return komisyonOrani;
        }
    }
}

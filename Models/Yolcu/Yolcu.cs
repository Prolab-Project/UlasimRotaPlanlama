using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Yolcu
{
    public abstract class Yolcu
    {
        public float indirimOrani; 

        public virtual double UcretHesapla(double ucret)
        {
            return ucret * (1 - indirimOrani);
        }
    }
}

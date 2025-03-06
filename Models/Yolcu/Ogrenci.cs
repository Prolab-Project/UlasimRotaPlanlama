using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Yolcu
{
    internal class Ogrenci : Yolcu , Indirim
    {
        void Indirim.indirim()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models
{
    interface Durak
    {
        void DurakBilgisi();
    }

    interface Mesafe
    {
        void MesafeHesaplama(double lat, double lon, double durak_lat, double durak_lon);
    }

    interface Ucret
    {
        double UcretHesapla();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace UlasimRotaPlanlama.Models.Arac.Otobus
{
    class bus_symbolavm : Durak
    {
        public double lat;
        public double lon;
        public void KonumBilgisi()
        {
            string DosyaOku;
            DosyaOku = File.ReadAllText("C:\\Users\\Ömer\\Desktop\\moovit\\bedirhan.json");

            JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(4).GetProperty("lat").GetDouble();
            lon = root.GetProperty("durakalr").EnumerateArray().ElementAt(4).GetProperty("lon").GetDouble();
        }
    }
}

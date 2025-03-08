using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Arac.Tramvay
{
    class tram_otogar : Durak
    {
        public double lat;
        public double lon;
        public void KonumBilgisi()
        {
            string DosyaOku;
            DosyaOku = File.ReadAllText("dataset/bedirhan.json");

            JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(6).GetProperty("lat").GetDouble();
            lon = root.GetProperty("durakalr").EnumerateArray().ElementAt(6).GetProperty("lon").GetDouble();
        }
    }
}

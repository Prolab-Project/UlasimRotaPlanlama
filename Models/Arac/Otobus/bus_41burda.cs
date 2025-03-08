using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace UlasimRotaPlanlama.Models.Arac.Otobus
{
    class bus_41burda : Durak
    {
        public double lat;
        public double lon; 
        public void KonumBilgisi()
        {
            string DosyaOku;
            DosyaOku = File.ReadAllText("dataset/bedirhan.json");

            using JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("lat").GetDouble();
            lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("lon").GetDouble();
        }
    }
}

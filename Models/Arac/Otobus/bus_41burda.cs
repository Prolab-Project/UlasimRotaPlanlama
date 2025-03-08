using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace UlasimRotaPlanlama.Models.Arac.Otobus
{
    class bus_41burda : Otobus , Durak
    {
        public void KonumBilgisi()
        {
            string DosyaOku;
            DosyaOku = File.ReadAllText("dataset/bedirhan.json");

            using JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            id = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("id").GetString();
            name = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("name").GetString();
            type = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("type").GetString();
            sonDurak = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("sonDurak").GetBoolean();
            lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("lat").GetDouble();
            lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(5).GetProperty("lon").GetDouble();

            Console.WriteLine("id: " + id);
            Console.WriteLine("name: " + name);
            Console.WriteLine("type: " + type);
            Console.WriteLine("son Durak: " + sonDurak);
            Console.WriteLine("lat: " + lat);
            Console.WriteLine("lon: " + lon);
        }
    }
}

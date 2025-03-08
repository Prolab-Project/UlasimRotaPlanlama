using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace UlasimRotaPlanlama.Models.Arac.Otobus
{
    class bus_otogar : Durak
    {
            public string id; 
            public string name;
            public string type;
            public double lat;
            public double lon;
            public bool sonDurak;
            public void KonumBilgisi()
            {
                string DosyaOku;
                DosyaOku = File.ReadAllText("dataset/bedirhan.json");

                JsonDocument doc = JsonDocument.Parse(DosyaOku);
                JsonElement root = doc.RootElement;
                id = root.GetProperty("duraklar").EnumerateArray().ElementAt(0).GetProperty("id").GetString();
                name = root.GetProperty("duraklar").EnumerateArray().ElementAt(0).GetProperty("name").GetString();
                type = root.GetProperty("duraklar").EnumerateArray().ElementAt(0).GetProperty("type").GetString();
                sonDurak = root.GetProperty("duraklar").EnumerateArray().ElementAt(0).GetProperty("sonDurak").GetBoolean();
                lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(0).GetProperty("lat").GetDouble();
                lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(0).GetProperty("lon").GetDouble();
            
                Console.WriteLine("id: " + id);
                Console.WriteLine("name: " + name);
                Console.WriteLine("type: " + type);
                Console.WriteLine("son Durak: " + sonDurak);
                Console.WriteLine("lat: " + lat);
                Console.WriteLine("lon: " + lon);

            }

    }
}

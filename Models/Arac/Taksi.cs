using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json; 
namespace UlasimRotaPlanlama.Models.Arac
{
    public class Taksi : Arac, Durak
    {
        
        public double openingFee;
        public double costPerKm;
        public bool taksi_bin;
        public double toplamucret;
        public double mesafe;

        public Taksi() {
            DurakBilgisi();
        }

        public void DurakBilgisi()
        {
            string DosyaOku;
            DosyaOku = File.ReadAllText("dataset/bedirhan.json");

            JsonDocument doc = JsonDocument.Parse(DosyaOku);
            JsonElement root = doc.RootElement;

            JsonElement taxi = root.GetProperty("taxi");

            this.openingFee = taxi.GetProperty("openingFee").GetDouble();
            this.costPerKm = taxi.GetProperty("costPerKm").GetDouble();

            Console.WriteLine(openingFee + "," + costPerKm);
        }

        public void MesafeHesaplama(double lat , double lon , double durak_lat , double durak_lon)
        {
            double derece_lat = Math.Abs(lat - durak_lat);
            double metre_lat = derece_lat * 111320;

            double derece_lon = Math.Abs(lon - durak_lon);
            double metre_lon = derece_lon * 85170;

            double pisagor_lat = metre_lat * metre_lat;
            double pisagor_lon = metre_lon * metre_lon;

            mesafe = Math.Sqrt(pisagor_lon + pisagor_lat);
            mesafe = mesafe / 1000;
            Console.WriteLine("mesafe hesaplanıyor:");
            Console.WriteLine(mesafe);
        
            if(mesafe > 3)
            {
                taksi_bin = true;
            }
            else
            {
                taksi_bin = false;
            }   
        }

        public double UcretHesapla()
        {
            
                //Console.WriteLine(costPerKm);
                toplamucret = costPerKm * (mesafe );
                Console.WriteLine(toplamucret);
                
            return toplamucret;
        }
    }
}

using System;
using System.IO;
using Microsoft.VisualBasic.Logging;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using UlasimRotaPlanlama.Models.Arac;
using System.Collections;
using System.Xml.Linq;
using UlasimRotaPlanlama.Models.Yolcu;
using UlasimRotaPlanlama.Models;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;

public static class jsonreader
{
    public static string JsonReader(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return json;

    }
}
public class json
{
    public string JsonCekme(int x, List<int> surelist)
    {
        string json = jsonreader.JsonReader("dataset/bedirhan.json");

        JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        double lat;
        double lon;
        string id;
        bool sondurak;
        string type;
        string name;

        lat = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("lat").GetDouble();
        lon = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("lon").GetDouble();
        id = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("id").GetString();
        sondurak = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("sonDurak").GetBoolean();
        type = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("type").GetString();
        name = root.GetProperty("duraklar").EnumerateArray().ElementAt(x).GetProperty("name").GetString();

        List<string> nextStopsList = new List<string>();

        if (root.GetProperty("duraklar").EnumerateArray().ElementAt(x).TryGetProperty("nextStops", out JsonElement nextStops))
        {
            foreach (JsonElement stop in nextStops.EnumerateArray())
            {
                string stopId = stop.GetProperty("stopId").GetString();
                double mesafe = stop.GetProperty("mesafe").GetDouble();
                int sure = stop.GetProperty("sure").GetInt32();
                double ucret = stop.GetProperty("ucret").GetDouble();

                surelist.Add(sure);
                nextStopsList.Add($"{stopId}({mesafe}km, {sure}dk, {ucret}TL)");
            }
        }

        string nextStopsData = nextStopsList.Count > 0 ? string.Join(", ", nextStopsList) : "None";

        string data = $"{lat} / {lon} / {id} / {sondurak} / {type} / {name} / {nextStopsData}";

        return data;
    }
}
public class OtobusSinifOlustur
{
    public static Otobus OtobusOlustur(string data)
    {
        string[] value = data.Split("/");

        double lat = Convert.ToDouble(value[0]);
        double lon = Convert.ToDouble(value[1]);
        string id = value[2];
        bool sonDurak = Convert.ToBoolean(value[3]);
        string type = value[4];
        string name = value[5];
        List<string> nextStops = new List<string>();

        if (value.Length > 6)
        {
            nextStops = value[6].Split(", ").ToList();
        }

        return new Otobus { id = id, name = name, type = type, lat = lat, lon = lon, sonDurak = sonDurak, NextStops = nextStops };
    }
}
public class TramSinifOlustur
{
    public static Tramvay TramvayOlustur(string data)
    {
        string[] value = data.Split("/");

        double lat = Convert.ToDouble(value[0]);
        double lon = Convert.ToDouble(value[1]);
        string id = value[2];
        bool sonDurak = Convert.ToBoolean(value[3]);
        string type = value[4];
        string name = value[5];
        List<string> nextStops = new List<string>();

        if (value.Length > 6)
        {
            nextStops = value[6].Split(", ").ToList();
        }

        return new Tramvay { id = id, name = name, type = type, lat = lat, lon = lon, sonDurak = sonDurak, NextStops = nextStops };
    }
}

namespace HaritaUygulamasi
{
    public class Form1 : Form
    {
        private GMapControl gMapControl;
        private List<Arac> aracListesi;

        public Form1(List<Arac> arac)
        {
            this.aracListesi = arac;
            this.Text = "Harita Uygulamas ";
            this.Width = 800;
            this.Height = 600;

            gMapControl = new GMapControl();
            gMapControl.Dock = DockStyle.Fill;

            gMapControl.MapProvider = GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl.Position = new PointLatLng(40.7696, 29.9405);
            gMapControl.MinZoom = 5;
            gMapControl.MaxZoom = 100;
            gMapControl.Zoom = 12;
            gMapControl.ShowCenter = false;

            this.Controls.Add(gMapControl);

            this.Load += new System.EventHandler(this.Form1_Load);

            this.ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var arac in aracListesi)
            {
                AddMarkerAtLocation(arac.lat, arac.lon, arac.name);
            }
        }

        private void AddMarkerAtLocation(double lat, double lon, string description)
        {
            GMapMarker marker = new GMarkerGoogle(new GMap.NET.PointLatLng(lat, lon), GMarkerGoogleType.green);
            marker.ToolTipText = description;

            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(marker);
            gMapControl.Overlays.Add(markers);

            gMapControl.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    GMap.NET.PointLatLng clickPos = gMapControl.FromLocalToLatLng(e.X, e.Y);

                    if (clickPos.Lat == lat && clickPos.Lng == lon)
                    {
                        MessageBox.Show("T klanan yerin a  klamas : " + description);
                    }
                }
            };
        }
        public static class mesafeHesapla
        {
            public static double MesafeHesapla(double lat, double lon, double durak_lat, double durak_lon)
            {
                double derece_lat = Math.Abs(lat - durak_lat);
                double metre_lat = derece_lat * 111320;

                double derece_lon = Math.Abs(lon - durak_lon);
                double metre_lon = derece_lon * 85170;

                double pisagor_lat = metre_lat * metre_lat;
                double pisagor_lon = metre_lon * metre_lon;

                double mesafe = Math.Sqrt(pisagor_lon + pisagor_lat);

                return mesafe / 1000; // km cinsinden mesafe
            }
        }

        public static class yakinDurakBul
        {
            public static void EnYakinDuragiBul(List<Arac> Durak, Taksi taksi)
            {
                Console.Write(" mevcut enlem (lat): ");
                double lat_konum = Convert.ToDouble(Console.ReadLine());

                Console.Write(" mevcut boylam (lon): ");
                double lon_konum = Convert.ToDouble(Console.ReadLine());

                Console.Write(" hedef enlem (lat): ");
                double hedef_lat = Convert.ToDouble(Console.ReadLine());

                Console.Write(" hedef boylam (lon): ");
                double hedef_lon = Convert.ToDouble(Console.ReadLine());

                double minMesafe = double.MaxValue;
                double minMesafeHedef = double.MaxValue;
                Arac enYakinDurak = null;
                Arac hedefEnYakinDurak = null;

                for (int i = 0; i < Durak.Count; i++)
                {
                    Console.WriteLine($"Durak {i}: " + Durak[i].name);
                }

                foreach (var durak in Durak)
                {
                    double mesafe = mesafeHesapla.MesafeHesapla(lat_konum, lon_konum, durak.lat, durak.lon);
                    double mesafeHedef = mesafeHesapla.MesafeHesapla(hedef_lat, hedef_lon, durak.lat, durak.lon);

                    if (mesafe < minMesafe)
                    {
                        minMesafe = mesafe;
                        enYakinDurak = durak;
                    }

                    if (mesafeHedef < minMesafeHedef)
                    {
                        minMesafeHedef = mesafeHedef;
                        hedefEnYakinDurak = durak;
                    }
                }

                if (enYakinDurak != null)
                {
                    Console.WriteLine($"En yak n baslangic duragi: {enYakinDurak.name}, {minMesafe:F2} km uzakl kta.");
                    Console.WriteLine($"En yak n hedef duragi: {hedefEnYakinDurak.name}, {minMesafeHedef:F2} km uzakl kta.");

                    if (minMesafe > 3)
                    {
                        Console.WriteLine("Mesafe 3 km'den fazla, taksi kullanman z  nerilir.");
                        taksi.MesafeHesaplama(lat_konum, lon_konum, enYakinDurak.lat, enYakinDurak.lon);
                        taksi.UcretHesapla();
                    }

                    if (minMesafeHedef > 3)
                    {
                        Console.WriteLine("Hedefe Mesafe 3 km'den fazla, taksi kullanman z  nerilir.");
                        taksi.MesafeHesaplama(hedef_lat, hedef_lon, hedefEnYakinDurak.lat, hedefEnYakinDurak.lon);
                        taksi.UcretHesapla();
                    }
                }
                else
                {
                    Console.WriteLine("Yak nlarda otob s dura   bulunamad .");
                }
            }
        }

        internal static class main
        {
            [STAThread]
            static void Main()
            {
                Taksi taksi = new Taksi();
                json jsonc = new json();

                ArrayList BusData = new ArrayList();
                List<int> surelist = new List<int>();

                BusData.Add(jsonc.JsonCekme(0, surelist));
                BusData.Add(jsonc.JsonCekme(1, surelist));
                BusData.Add(jsonc.JsonCekme(2, surelist));
                BusData.Add(jsonc.JsonCekme(3, surelist));
                BusData.Add(jsonc.JsonCekme(4, surelist));
                BusData.Add(jsonc.JsonCekme(5, surelist));

                ArrayList TramvayData = new ArrayList();
                TramvayData.Add(jsonc.JsonCekme(6, surelist));
                TramvayData.Add(jsonc.JsonCekme(7, surelist));
                TramvayData.Add(jsonc.JsonCekme(8, surelist));
                TramvayData.Add(jsonc.JsonCekme(9, surelist));

                /*foreach (var elements in BusData)
                {
                    Console.WriteLine(elements);
                }

                foreach (var elements in TramvayData)
                {
                    Console.WriteLine(elements);
                }*/

                List<Arac> otobusDuraklari = new List<Arac>();

                Otobus BusOtogar = OtobusSinifOlustur.OtobusOlustur(BusData[0].ToString());
                otobusDuraklari.Add(BusOtogar);

                Otobus BusSekapark = OtobusSinifOlustur.OtobusOlustur(BusData[1].ToString());
                otobusDuraklari.Add(BusSekapark);

                Otobus BusYahyakaptan = OtobusSinifOlustur.OtobusOlustur(BusData[2].ToString());
                otobusDuraklari.Add(BusYahyakaptan);

                Otobus BusUmuttepe = OtobusSinifOlustur.OtobusOlustur(BusData[3].ToString());
                otobusDuraklari.Add(BusUmuttepe);

                Otobus BusSymbolavm = OtobusSinifOlustur.OtobusOlustur(BusData[4].ToString());
                otobusDuraklari.Add(BusSymbolavm);

                Otobus Bus41Burada = OtobusSinifOlustur.OtobusOlustur(BusData[5].ToString());
                otobusDuraklari.Add(Bus41Burada);

                List<Arac> tramDuraklari = new List<Arac>();

                Tramvay TramOtogar = TramSinifOlustur.TramvayOlustur(TramvayData[0].ToString());
                tramDuraklari.Add(TramOtogar);

                Tramvay TramYahyakaptan = TramSinifOlustur.TramvayOlustur(TramvayData[1].ToString());
                tramDuraklari.Add(TramYahyakaptan);

                Tramvay TramSekapark = TramSinifOlustur.TramvayOlustur(TramvayData[2].ToString());
                tramDuraklari.Add(TramSekapark);

                Tramvay TramHalkevi = TramSinifOlustur.TramvayOlustur(TramvayData[3].ToString());
                tramDuraklari.Add(TramHalkevi);

                //yakinDurakBul.EnYakinDuragiBul(otobusDuraklari, taksi);
                //yakinDurakBul.EnYakinDuragiBul(tramDuraklari, taksi);

                List<Arac> aracDuraklari = new List<Arac>();
                aracDuraklari.AddRange(otobusDuraklari);
                aracDuraklari.AddRange(tramDuraklari);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(aracDuraklari));


                /*foreach (var stop in BusSekapark.NextStops)
                {
                    Console.WriteLine(stop);
                }

                foreach (var stop in TramSekapark.NextStops)
                {
                    Console.WriteLine(stop);
                }*/


                Graph graph = new Graph();

                foreach (var durak in otobusDuraklari)
                {
                    graph.AddNode(durak);
                }

                foreach (var durak in tramDuraklari)
                {
                    graph.AddNode(durak);
                }


                int i = 0;

                var validStops = new List<string>
                    {
                        "bus_otogar",
                        "bus_sekapark",
                        "bus_yahyakaptan",
                        "bus_umuttepe",
                        "bus_symbolavm",
                        "bus_41burda"
                    };

                foreach (var otobus in otobusDuraklari.OfType<Otobus>())
                {
                    foreach (var nextStopRaw in otobus.NextStops)
                    {
                        if (string.IsNullOrEmpty(nextStopRaw) || nextStopRaw.Contains("None"))
                            continue;

                        var nextStop = nextStopRaw.Split('(')[0].Trim();

                        if (!validStops.Contains(nextStop))
                        {
                            continue;
                        }

                        Console.WriteLine($"Geçerli NextStop: {nextStop}");

                        var icerik = nextStopRaw.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string surestr = string.Empty;

                        if (!string.IsNullOrEmpty(nextStop))
                        {
                            var matchingStop = aracDuraklari.FirstOrDefault(d => d.id.Trim() == nextStop.Trim());

                            if (matchingStop != null)
                            {
                                graph.AddEdge(otobus, matchingStop, surelist[i]);
                                i++;
                            }
                            else
                            {
                                Console.WriteLine($"Eşleşen durak bulunamadı: {nextStop}");
                            }
                        }
                    }
                }

                graph.PrintGraph();
                graph.PrintShortestPath(BusOtogar, BusYahyakaptan);


                /*
                foreach (var tramvay in tramDuraklari.OfType<Tramvay>())
                {
                    foreach (var nextStopRaw in tramvay.NextStops)
                    {
                        var nextStop = nextStopRaw.Split('(')[0].Trim();

                        var icerik = nextStopRaw.Split(new char[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string surestr = string.Empty;

                        foreach (var item in icerik)
                        {
                            if (item.Contains("dk"))
                            {
                                surestr = new string(item.Where(char.IsDigit).ToArray());
                                int.TryParse(surestr, out int sure);
                                surelist.Add(sure);
                                i++;
                            }
                        }
                        if (!string.IsNullOrEmpty(nextStop))
                        {
                            graph.AddEdge(tramvay.id, nextStop, surelist[i]);
                        }
                    }
                }
                graph.PrintGraph();*/



            }
        }
    }
}/*
using System;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;

namespace HaritaUygulamasi
{
    public partial class Form1 : Form
    {
        private GMapControl gMapControl;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.gMapControl = new GMapControl();
            this.SuspendLayout();

            // gMapControl  zelliklerini ayarl yoruz
            this.gMapControl.Dock = DockStyle.Fill;
            this.gMapControl.MapProvider = GoogleMapProvider.Instance;  // Google haritas  kullan yoruz
            GMaps.Instance.Mode = AccessMode.ServerOnly;  // Yaln zca sunucu  zerinden harita verisi al nacak
            this.Controls.Add(this.gMapControl);

            // Harita ba lang   ayarlar 
            gMapControl.Position = new GMap.NET.PointLatLng(40.730610, -73.935242);  // Ba lang   konumu (New York)
            gMapControl.MinZoom = 0;   // Minimum zoom seviyesi
            gMapControl.MaxZoom = 20;  // Maksimum zoom seviyesi


            // Form1  zelliklerini ayarl yoruz
            this.ClientSize = new System.Drawing.Size(800, 600);  // Form boyutunu belirliyoruz
            this.Name = "Form1";
            this.Text = "Harita Uygulamas ";
            this.Load += new System.EventHandler(this.Form1_Load);

            this.ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Harita  zerinde marker ekliyoruz
            AddMarkerAtLocation(40.730610, -73.935242, "Burada bir buton ekliyoruz!");

            // Ba ka bir marker ekleyelim
            AddMarkerAtLocation(40.740610, -73.935242, "Ba ka bir konum");
        }

        private void AddMarkerAtLocation(double lat, double lon, string description)
        {
            // GMap  zerinde marker olu turuyoruz
            GMapMarker marker = new GMarkerGoogle(new GMap.NET.PointLatLng(lat, lon), GMarkerGoogleType.green);
            marker.ToolTipText = description;

            // Marker'  eklemek i in GMapOverlay kullan yoruz
            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(marker);
            gMapControl.Overlays.Add(markers);

            // Marker t klama olay n  i lemek i in
            gMapControl.MouseClick += (sender, e) =>
            {
                // Mouse t klama olay  kontrol 
                if (e.Button == MouseButtons.Left)
                {
                    // T klanan yerin koordinatlar n  kontrol ediyoruz
                    GMap.NET.PointLatLng clickPos = gMapControl.FromLocalToLatLng(e.X, e.Y);

                    // E er t klanan koordinat, marker' n koordinat na yak nsa
                    if (clickPos.Lat == lat && clickPos.Lng == lon)
                    {
                        MessageBox.Show("T klanan yerin a  klamas : " + description);
                    }
                }
            };
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
*/
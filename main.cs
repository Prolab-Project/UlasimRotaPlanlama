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
using System.Drawing;

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
    public string JsonCekme(int x, List<double> surelist, List<double> ucretlist)
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

        // Durak bilgilerini yazdır
        Console.WriteLine($"\nDurak: {name} (ID: {id})");
        Console.WriteLine("Next Stops:");

        if (root.GetProperty("duraklar").EnumerateArray().ElementAt(x).TryGetProperty("nextStops", out JsonElement nextStops))
        {
            foreach (JsonElement stop in nextStops.EnumerateArray())
            {
                string stopId = stop.GetProperty("stopId").GetString();
                double mesafe = stop.GetProperty("mesafe").GetDouble();
                int sure = stop.GetProperty("sure").GetInt32();
                double ucret = stop.GetProperty("ucret").GetDouble();

                Console.WriteLine($"  -> {stopId}: {mesafe}km, {sure}dk, {ucret}TL");

                surelist.Add(sure);
                ucretlist.Add(ucret);
                nextStopsList.Add($"{stopId}({mesafe}km, {sure}dk, {ucret}TL)");
            }
        }

        Console.WriteLine("Transfer Bilgileri:");
        if (root.GetProperty("duraklar").EnumerateArray().ElementAt(x).TryGetProperty("transfer", out JsonElement Transfer) &&
            !Transfer.ValueKind.Equals(JsonValueKind.Null))
        {
            string transferStopId = Transfer.GetProperty("transferStopId").GetString();
            double transferSure = Transfer.GetProperty("transferSure").GetDouble();
            double transferUcret = Transfer.GetProperty("transferUcret").GetDouble();

            Console.WriteLine($"  -> Transfer durağı: {transferStopId}");
            Console.WriteLine($"  -> Transfer süresi: {transferSure}dk");
            Console.WriteLine($"  -> Transfer ücreti: {transferUcret}TL");

            surelist.Add((int)transferSure);
            ucretlist.Add(transferUcret);
            nextStopsList.Add($"{transferStopId}({0}km, {transferSure}dk, {transferUcret}TL)");
        }
        else
        {
            Console.WriteLine("  -> Transfer bilgisi yok");
        }
        Console.WriteLine("------------------------");

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
        private Graph graph;

        public Form1(List<Arac> arac, Graph graph = null)
        {
            this.aracListesi = arac;
            this.graph = graph;
            this.Text = "Harita Uygulaması";
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

        public void DrawRoute(List<Arac> path)
        {
            if (path == null || path.Count < 2)
                return;

            GMapOverlay routeOverlay = new GMapOverlay("route");

            List<PointLatLng> points = new List<PointLatLng>();
            foreach (var arac in path)
            {
                points.Add(new PointLatLng(arac.lat, arac.lon));
            }

            GMapRoute route = new GMapRoute(points, "Rota");
            route.Stroke = new Pen(Color.Red, 3);

            routeOverlay.Routes.Add(route);

            AddDirectionArrows(points, routeOverlay);

            gMapControl.Overlays.Add(routeOverlay);

            gMapControl.Refresh();

            if (points.Count > 0)
            {
                gMapControl.ZoomAndCenterRoutes("route");
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

        private void AddDirectionArrows(List<PointLatLng> points, GMapOverlay overlay)
        {
            if (points.Count < 2)
                return;

            for (int i = 0; i < points.Count - 1; i += 1)
            {
                double midLat = (points[i].Lat + points[i + 1].Lat) / 2;
                double midLng = (points[i].Lng + points[i + 1].Lng) / 2;

                GMapMarker arrow = new GMarkerGoogle(
                    new PointLatLng(midLat, midLng),
                    GMarkerGoogleType.arrow);

                overlay.Markers.Add(arrow);
            }
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
            public static void EnYakinDuragiBul(List<Arac> Durak, Taksi taksi, Graph graph)
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
                    Console.WriteLine($"En yakin baslangic duragi: {enYakinDurak.name}, {minMesafe:F2} km uzaklıkta.");
                    Console.WriteLine($"En yakin hedef duragi: {hedefEnYakinDurak.name}, {minMesafeHedef:F2} km uzaklıkta.");

                    if (enYakinDurak is Otobus && hedefEnYakinDurak is Tramvay)
                    {
                        var transferStop = graph.AdjacencyList[enYakinDurak].FirstOrDefault(t => t.Item1.id == "tram_otogar");
                        if (transferStop != default)
                        {
                            Console.WriteLine($"Aktarma durağı: {transferStop.Item1.name}");
                        }
                    }
                    else if (enYakinDurak is Tramvay && hedefEnYakinDurak is Otobus)
                    {
                        var transferStop = graph.AdjacencyList[enYakinDurak].FirstOrDefault(t => t.Item1.id == "bus_otogar");
                        if (transferStop != default)
                        {
                            Console.WriteLine($"Aktarma durağı: {transferStop.Item1.name}");
                        }
                    }

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
                    graph.PrintShortestPath(enYakinDurak, hedefEnYakinDurak);

                }
                else
                {
                    Console.WriteLine("Yakınlarda otobüs durağı bulunamadı.");
                }
            }
        }

        internal static class main
        {
            [STAThread]
            static void Main()
            {
                // Bu metodları en başta çağırın
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Taksi taksi = new Taksi();
                json jsonc = new json();

                ArrayList BusData = new ArrayList();
                List<double> surelist = new List<double>();
                List<double> ucretlist = new List<double>();

                BusData.Add(jsonc.JsonCekme(0, surelist, ucretlist));
                BusData.Add(jsonc.JsonCekme(1, surelist, ucretlist));
                BusData.Add(jsonc.JsonCekme(2, surelist, ucretlist));
                BusData.Add(jsonc.JsonCekme(3, surelist, ucretlist));
                BusData.Add(jsonc.JsonCekme(4, surelist, ucretlist));
                BusData.Add(jsonc.JsonCekme(5, surelist, ucretlist));

                ArrayList TramvayData = new ArrayList();
                TramvayData.Add(jsonc.JsonCekme(6, surelist, ucretlist));
                TramvayData.Add(jsonc.JsonCekme(7, surelist, ucretlist));
                TramvayData.Add(jsonc.JsonCekme(8, surelist, ucretlist));
                TramvayData.Add(jsonc.JsonCekme(9, surelist, ucretlist));

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

                List<Arac> aracDuraklari = new List<Arac>();
                aracDuraklari.AddRange(otobusDuraklari);
                aracDuraklari.AddRange(tramDuraklari);

                Graph graph = new Graph();

                foreach (var durak in otobusDuraklari)
                {
                    graph.AddNode(durak);
                }

                foreach (var durak in tramDuraklari)
                {
                    graph.AddNode(durak);
                }

                var validStops = new List<string>
                    {
                        "bus_otogar",
                        "bus_sekapark",
                        "bus_yahyakaptan",
                        "bus_umuttepe",
                        "bus_symbolavm",
                        "bus_41burda",
                        "tram_otogar",
                        "tram_yahyakaptan",
                        "tram_sekapark",
                        "tram_halkevi"
                    };

                int i = 0;

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

                        var matchingStop = aracDuraklari.FirstOrDefault(d => d.id.Trim() == nextStop.Trim());

                        if (matchingStop != null && i < ucretlist.Count)
                        {
                            graph.AddEdge(otobus, matchingStop, ucretlist[i]);
                            i++;
                        }
                    }
                }

                foreach (var tramvay in tramDuraklari.OfType<Tramvay>())
                {
                    foreach (var nextStopRaw in tramvay.NextStops)
                    {
                        if (string.IsNullOrEmpty(nextStopRaw) || nextStopRaw.Contains("None"))
                            continue;

                        var nextStop = nextStopRaw.Split('(')[0].Trim();

                        if (!validStops.Contains(nextStop))
                        {
                            continue;
                        }

                        var matchingStop = aracDuraklari.FirstOrDefault(d => d.id.Trim() == nextStop.Trim());

                        if (matchingStop != null && i < ucretlist.Count)
                        {
                            graph.AddEdge(tramvay, matchingStop, ucretlist[i]);
                            i++;
                        }
                        else
                        {
                            Console.WriteLine($"Ücret listesi sınırı aşıldı veya eşleşen durak bulunamadı: {nextStop}");
                        }
                    }
                }

                graph.PrintGraph();
                graph.PrintShortestPath(BusUmuttepe, TramOtogar);

                // Form1'i graph ile birlikte oluştur
                Form1 form = new Form1(aracDuraklari, graph);

                // Rota hesapla ve çiz
                List<Arac> shortestPath = graph.GetShortestPath(BusUmuttepe, TramOtogar);
                form.DrawRoute(shortestPath);

                Application.Run(form);
            }
        }
    }
}
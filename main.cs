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
        private RichTextBox terminalBox;
        private TextBox startLatTextBox;
        private TextBox startLonTextBox;
        private TextBox targetLatTextBox;
        private TextBox targetLonTextBox;
        private Button calculateRouteButton;

        public Form1(List<Arac> arac, Graph graph = null)
        {
            this.aracListesi = arac;
            this.graph = graph;
            this.Text = "Harita Uygulaması";
            this.Width = 1000;
            this.Height = 600;

            Panel terminalPanel = new Panel();
            terminalPanel.Dock = DockStyle.Top;
            terminalPanel.Height = 100;
            terminalPanel.BackColor = Color.Black;

            terminalBox = new RichTextBox();
            terminalBox.Dock = DockStyle.Fill;
            terminalBox.BackColor = Color.White;
            terminalBox.ForeColor = Color.Black;
            terminalBox.Font = new Font("Consolas", 10);
            terminalBox.Multiline = true;
            terminalBox.ScrollBars = RichTextBoxScrollBars.Vertical;

            terminalPanel.Controls.Add(terminalBox);

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
            this.Controls.Add(terminalPanel);

            // Kullanıcıdan koordinat girişi için TextBox'lar ekleyelim
            startLatTextBox = new TextBox { PlaceholderText = "Mevcut Enlem (lat)", Dock = DockStyle.Top };
            startLonTextBox = new TextBox { PlaceholderText = "Mevcut Boylam (lon)", Dock = DockStyle.Top };
            targetLatTextBox = new TextBox { PlaceholderText = "Hedef Enlem (lat)", Dock = DockStyle.Top };
            targetLonTextBox = new TextBox { PlaceholderText = "Hedef Boylam (lon)", Dock = DockStyle.Top };
            calculateRouteButton = new Button { Text = "Rota Hesapla", Dock = DockStyle.Top };

            // Butona tıklama olayını ekleyelim
            calculateRouteButton.Click += CalculateRouteButton_Click;

            // Kontrolleri forma ekleyelim
            this.Controls.Add(calculateRouteButton);
            this.Controls.Add(targetLonTextBox);
            this.Controls.Add(targetLatTextBox);
            this.Controls.Add(startLonTextBox);
            this.Controls.Add(startLatTextBox);

            this.Load += new System.EventHandler(this.Form1_Load);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var arac in aracListesi)
            {
                AddMarkerAtLocation(arac.lat, arac.lon, arac.name);
            }

            LogToTerminal("Harita yüklendi.");
        }

        private void CalculateRouteButton_Click(object sender, EventArgs e)
        {
            if (double.TryParse(startLatTextBox.Text, out double startLat) &&
                double.TryParse(startLonTextBox.Text, out double startLon) &&
                double.TryParse(targetLatTextBox.Text, out double targetLat) &&
                double.TryParse(targetLonTextBox.Text, out double targetLon))
            {
                // En yakın durakları bul
                Taksi taksi = new Taksi();
                yakinDurakBul.EnYakinDuragiBul(aracListesi, taksi, graph, startLat, startLon, targetLat, targetLon);
            }
            else
            {
                MessageBox.Show("Lütfen geçerli koordinatlar girin.");
            }
        }

        public void DrawRoutes(List<Arac> shortestPath1, List<Arac> shortestPath2)
        {
            if ((shortestPath1 == null || shortestPath1.Count < 2) &&
                (shortestPath2 == null || shortestPath2.Count < 2))
            {
                LogToTerminal("En kısa yollar bulunamadı.");
                return;
            }

            LogToTerminal("🚏 En Kısa Yol Rotaları:\n");

            GMapOverlay routeOverlay1 = new GMapOverlay("route1");
            GMapOverlay routeOverlay2 = new GMapOverlay("route2");

            List<PointLatLng> points1 = new List<PointLatLng>();
            List<PointLatLng> points2 = new List<PointLatLng>();

            int maxLength = Math.Max(shortestPath1.Count, shortestPath2.Count);

            double totalWeight1 = 0;
            double totalWeight2 = 0;

            int indexWidth = 2;    // "1." için genişlik
            int nameWidth = 22;    // Durak ismi genişliği
            int coordWidth = 25;   // Koordinatlar genişliği
            int typeWidth = 7;    // "Graph" veya "Graph2"
            string separator = " || ";

            for (int i = 0; i < maxLength; i++)
            {
                string stopInfo1 = i < shortestPath1.Count
                    ? $"{(i + 1).ToString().PadRight(indexWidth)} {shortestPath1[i].name.PadRight(nameWidth)} {($"({shortestPath1[i].lat}, {shortestPath1[i].lon})").PadRight(coordWidth)} - {("Graph".PadRight(typeWidth))}"
                    : new string(' ', indexWidth + nameWidth + coordWidth + typeWidth);

                string stopInfo2 = i < shortestPath2.Count
                    ? $"{(i + 1).ToString().PadRight(indexWidth)} {shortestPath2[i].name.PadRight(nameWidth)} {($"({shortestPath2[i].lat}, {shortestPath2[i].lon})").PadRight(coordWidth)} - {("Graph2".PadRight(typeWidth))}"
                    : new string(' ', indexWidth + nameWidth + coordWidth + typeWidth);

                LogToTerminal(stopInfo1 + separator + stopInfo2);

                if (i < shortestPath1.Count)
                {
                    var arac = shortestPath1[i];
                    points1.Add(new PointLatLng(arac.lat, arac.lon));

                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(arac.lat, arac.lon), GMarkerGoogleType.blue);
                    marker.ToolTipText = $"Graph Durak {i + 1}: {arac.name}";
                    routeOverlay1.Markers.Add(marker);

                    if (i > 0)
                    {
                        totalWeight1 += graph.GetEdgeWeight(shortestPath1[i - 1], arac);
                    }
                }

                if (i < shortestPath2.Count)
                {
                    var arac = shortestPath2[i];
                    points2.Add(new PointLatLng(arac.lat, arac.lon));

                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(arac.lat, arac.lon), GMarkerGoogleType.red);
                    marker.ToolTipText = $"Graph2 Durak {i + 1}: {arac.name}";
                    routeOverlay2.Markers.Add(marker);

                    if (i > 0)
                    {
                        totalWeight2 += graph.GetEdgeWeight(shortestPath2[i - 1], arac);
                    }
                }
            }

            GMapRoute route1 = new GMapRoute(points1, "Rota 1");
            route1.Stroke = new Pen(Color.Blue, 3);
            routeOverlay1.Routes.Add(route1);

            GMapRoute route2 = new GMapRoute(points2, "Rota 2");
            route2.Stroke = new Pen(Color.Green, 3);
            routeOverlay2.Routes.Add(route2);

            gMapControl.Overlays.Clear();
            gMapControl.Overlays.Add(routeOverlay1);
            gMapControl.Overlays.Add(routeOverlay2);

            gMapControl.Refresh();

            LogToTerminal(string.Format("Toplam Ağırlık: {0}                                             || Toplam Ağırlık: {1}", totalWeight1, totalWeight2));
            LogToTerminal("📍 Rotalar başarıyla çizildi.");

            if (points1.Count > 0 || points2.Count > 0)
            {
                gMapControl.ZoomAndCenterRoutes("route1");
                gMapControl.ZoomAndCenterRoutes("route2");
            }
        }

        private void LogToTerminal(string message)
        {
            terminalBox.AppendText("> " + message + Environment.NewLine);
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
            public static void EnYakinDuragiBul(List<Arac> Durak, Taksi taksi, Graph graph, double lat_konum, double lon_konum, double hedef_lat, double hedef_lon)
            {
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

                    // Rota çizme işlemi
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

                //yakinDurakBul.EnYakinDuragiBul(otobusDuraklari, taksi);
                //yakinDurakBul.EnYakinDuragiBul(tramDuraklari, taksi);

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
                //yakinDurakBul.EnYakinDuragiBul(otobusDuraklari, taksi, graph);
                //graph.PrintShortestPath(BusSekapark, BusOtogar);

                Form1 form = new Form1(aracDuraklari, graph);

                List<Arac> shortestPath = graph.GetShortestPath(BusUmuttepe, BusOtogar);

                Graph graph2 = new Graph();

                foreach (var durak in otobusDuraklari)
                {
                    graph2.AddNode(durak);
                }

                foreach (var durak in tramDuraklari)
                {
                    graph2.AddNode(durak);
                }

                int a = 0;

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

                        if (!string.IsNullOrEmpty(nextStop))
                        {
                            var matchingStop = aracDuraklari.FirstOrDefault(d => d.id.Trim() == nextStop.Trim());

                            if (matchingStop != null && a < surelist.Count)
                            {
                                graph2.AddEdge(otobus, matchingStop, surelist[a]);
                                a++;
                            }
                            else
                            {
                                Console.WriteLine($"Süre listesi sınırı aşıldı veya eşleşen durak bulunamadı: {nextStop}");
                            }
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

                        Console.WriteLine($"Geçerli NextStop: {nextStop}");

                        if (!string.IsNullOrEmpty(nextStop))
                        {
                            var matchingStop = aracDuraklari.FirstOrDefault(d => d.id.Trim() == nextStop.Trim());

                            if (matchingStop != null && a < surelist.Count)
                            {
                                graph2.AddEdge(tramvay, matchingStop, surelist[a]);
                                a++;
                            }
                            else
                            {
                                Console.WriteLine($"Süre listesi sınırı aşıldı veya eşleşen durak bulunamadı: {nextStop}");
                            }
                        }
                    }
                }

                graph2.PrintGraph();
                List<Arac> shortestPath2 = graph2.GetShortestPath(BusUmuttepe, BusOtogar);
                form.DrawRoutes(shortestPath, shortestPath2);
                Application.Run(form);



                //graph2.PrintShortestPath(BusSekapark, BusOtogar);
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
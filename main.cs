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
using System.Threading;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using UlasimRotaPlanlama.Models.Odeme;

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

        if (root.GetProperty("duraklar").EnumerateArray().ElementAt(x).TryGetProperty("nextStops", out JsonElement nextStops))
        {
            foreach (JsonElement stop in nextStops.EnumerateArray())
            {
                string stopId = stop.GetProperty("stopId").GetString();
                double mesafe = stop.GetProperty("mesafe").GetDouble();
                int sure = stop.GetProperty("sure").GetInt32();
                double ucret = stop.GetProperty("ucret").GetDouble();

                surelist.Add(sure);
                ucretlist.Add(ucret);
                nextStopsList.Add($"{stopId}({mesafe}km, {sure}dk, {ucret}TL)");
            }
        }

        if (root.GetProperty("duraklar").EnumerateArray().ElementAt(x).TryGetProperty("transfer", out JsonElement Transfer) &&
            !Transfer.ValueKind.Equals(JsonValueKind.Null))
        {
            string transferStopId = Transfer.GetProperty("transferStopId").GetString();
            double transferSure = Transfer.GetProperty("transferSure").GetDouble();
            double transferUcret = Transfer.GetProperty("transferUcret").GetDouble();

            surelist.Add((int)transferSure);
            ucretlist.Add(transferUcret);
            nextStopsList.Add($"{transferStopId}({0}km, {transferSure}dk, {transferUcret}TL)");
        }
        else
        {
            Console.WriteLine("  -> Transfer bilgisi yok");
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
        private Graph graph;
        private RichTextBox terminalBox;
        private TextBox startLatTextBox;
        private TextBox startLonTextBox;
        private TextBox targetLatTextBox;
        private TextBox targetLonTextBox;
        private Button calculateRouteButton;
        private Button paymentMethodButton;
        private static bool isNakitSelected = false; 
        private static bool isKentKartSelected = false;
        private static bool isKrediKartiSelected = false; 

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

            startLatTextBox = new TextBox { PlaceholderText = "Mevcut Enlem (lat)", Dock = DockStyle.Top };
            startLonTextBox = new TextBox { PlaceholderText = "Mevcut Boylam (lon)", Dock = DockStyle.Top };
            targetLatTextBox = new TextBox { PlaceholderText = "Hedef Enlem (lat)", Dock = DockStyle.Top };
            targetLonTextBox = new TextBox { PlaceholderText = "Hedef Boylam (lon)", Dock = DockStyle.Top };
            calculateRouteButton = new Button { Text = "Rota Hesapla", Dock = DockStyle.Top };

            calculateRouteButton.Click += CalculateRouteButton_Click;

            this.Controls.Add(calculateRouteButton);
            this.Controls.Add(targetLonTextBox);
            this.Controls.Add(targetLatTextBox);
            this.Controls.Add(startLonTextBox);
            this.Controls.Add(startLatTextBox);

            paymentMethodButton = new Button { Text = "Ödeme Yöntemi Seç", Dock = DockStyle.Top };
            paymentMethodButton.Click += PaymentMethodButton_Click;

            this.Controls.Add(paymentMethodButton);

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
        public string coordinates;
        private void CalculateRouteButton_Click(object sender, EventArgs e)
        {
            if (double.TryParse(startLatTextBox.Text, out double startLat) &&
                double.TryParse(startLonTextBox.Text, out double startLon) &&
                double.TryParse(targetLatTextBox.Text, out double targetLat) &&
                double.TryParse(targetLonTextBox.Text, out double targetLon))
            {
                coordinates = $"{startLat}/{startLon}/{targetLat}/{targetLon}";
            }
            else
            {
                MessageBox.Show("Lütfen geçerli koordinatlar girin.");
            }
        }
        private void PaymentMethodButton_Click(object sender, EventArgs e)
        {
            string message = "💳 Ödeme tipini seçiniz:\n1 - Nakit\n2 - Kentkart (%20 indirim)\n3 - Kredi Kartı (+%1.5 komisyon)";
            string title = "Ödeme Yöntemi Seçimi";
            string input = Microsoft.VisualBasic.Interaction.InputBox(message, title, "1");

            if (int.TryParse(input, out int choice))
            {
                switch (choice)
                {
                    case 1:
                        MessageBox.Show("Nakit ödeme seçildi.");
                        isNakitSelected = true;
                        break;
                    case 2:
                        MessageBox.Show("Kentkart seçildi. %20 indirim uygulanacak.");
                        isKentKartSelected = true;

                        break;
                    case 3:
                        MessageBox.Show("Kredi Kartı seçildi. +%1.5 komisyon uygulanacak.");
                        isKrediKartiSelected = true;
                        break;
                    default:
                        MessageBox.Show("Geçersiz seçim. Lütfen tekrar deneyin.");
                        break;
                }
            }
            else
            {
                MessageBox.Show("Geçersiz giriş. Lütfen bir sayı girin.");
            }
        }
        public static double totalWeight1 = 0;
        public static double totalWeight2 = 0;
        public void DrawRoutes(List<Arac> shortestPath1, List<Arac> shortestPath2 , Taksi taksi, double taksiucreti, double lat_konum , double lon_konum , double lat_hedef , double lon_hedef)
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

            int indexWidth = 2;    
            int nameWidth = 22;    
            int coordWidth = 25;   
            int typeWidth = 7;    
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

                if (shortestPath1.Count > 0)
                {
                    var firstStop = shortestPath1[0];
                    var lastStop = shortestPath1[shortestPath1.Count - 1]; 

                    points1.Insert(0, new PointLatLng(lat_konum, lon_konum));
                    GMapMarker startMarker = new GMarkerGoogle(new PointLatLng(lat_konum, lon_konum), GMarkerGoogleType.yellow);
                    startMarker.ToolTipText = "Başlangıç Konumu";
                    routeOverlay1.Markers.Add(startMarker);

                    GMapRoute startToFirstStop = new GMapRoute(new List<PointLatLng>
                    {
                        new PointLatLng(lat_konum, lon_konum),
                        new PointLatLng(firstStop.lat, firstStop.lon)
                    }, "Başlangıç - İlk Durak");
                    startToFirstStop.Stroke = new Pen(Color.Yellow, 2);
                    routeOverlay1.Routes.Add(startToFirstStop);

                    GMapRoute lastStopToTarget = new GMapRoute(new List<PointLatLng>
                    {
                        new PointLatLng(lastStop.lat, lastStop.lon),
                        new PointLatLng(lat_hedef, lon_hedef)
                    }, "Son Durak - Hedef");
                    lastStopToTarget.Stroke = new Pen(Color.Yellow, 2);
                    routeOverlay1.Routes.Add(lastStopToTarget);

                    GMapMarker targetMarker = new GMarkerGoogle(new PointLatLng(lat_hedef, lon_hedef), GMarkerGoogleType.red);
                    targetMarker.ToolTipText = "Hedef Konumu";
                    routeOverlay1.Markers.Add(targetMarker);
                }

                if (i < shortestPath1.Count)
                {
                    var arac = shortestPath1[i];
                    points1.Add(new PointLatLng(arac.lat, arac.lon));

                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(arac.lat, arac.lon), GMarkerGoogleType.yellow);
                    marker.ToolTipText = $"Graph Durak {i + 1}: {arac.name}";
                    routeOverlay1.Markers.Add(marker);

                    if (i > 0)
                    {
                        totalWeight1 += graph.GetEdgeWeight(shortestPath1[i - 1], arac);
                    }
                }

                if (shortestPath2.Count > 0)
                {
                    var firstStop = shortestPath2[0];
                    var lastStop = shortestPath2[shortestPath2.Count - 1]; 

                    points1.Insert(0, new PointLatLng(lat_konum, lon_konum));
                    GMapMarker startMarker = new GMarkerGoogle(new PointLatLng(lat_konum, lon_konum), GMarkerGoogleType.yellow);
                    startMarker.ToolTipText = "Başlangıç Konumu";
                    routeOverlay2.Markers.Add(startMarker);

                    GMapRoute startToFirstStop = new GMapRoute(new List<PointLatLng>
                    {
                        new PointLatLng(lat_konum, lon_konum),
                        new PointLatLng(firstStop.lat, firstStop.lon)
                    }, "Başlangıç - İlk Durak");
                    startToFirstStop.Stroke = new Pen(Color.Yellow, 2);
                    routeOverlay2.Routes.Add(startToFirstStop);

                    GMapRoute lastStopToTarget = new GMapRoute(new List<PointLatLng>
                    {
                        new PointLatLng(lastStop.lat, lastStop.lon),
                        new PointLatLng(lat_hedef, lon_hedef)
                    }, "Son Durak - Hedef");
                    lastStopToTarget.Stroke = new Pen(Color.Yellow, 2);
                    routeOverlay2.Routes.Add(lastStopToTarget);

                    GMapMarker targetMarker = new GMarkerGoogle(new PointLatLng(lat_hedef, lon_hedef), GMarkerGoogleType.red);
                    targetMarker.ToolTipText = "Hedef Konumu";
                    routeOverlay2.Markers.Add(targetMarker);
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
            route1.Stroke = new Pen(Color.Green, 3);
            routeOverlay1.Routes.Add(route1);

            GMapRoute route2 = new GMapRoute(points2, "Rota 2");
            route2.Stroke = new Pen(Color.Red, 3);
            routeOverlay2.Routes.Add(route2);

            gMapControl.Overlays.Clear();
            gMapControl.Overlays.Add(routeOverlay1);
            gMapControl.Overlays.Add(routeOverlay2);

            gMapControl.Refresh();

            Yolcu ogrenci = new Ogrenci();
            Yolcu yasli = new Yasli();
            Yolcu genel = new Genel();

            double indirimliUcretOgrenci = ogrenci.UcretHesapla(totalWeight1) + taksiucreti;
            double indirimliUcretYasli = yasli.UcretHesapla(totalWeight1) + taksiucreti;
            double indirimliUcretGenel = genel.UcretHesapla(totalWeight1) + taksiucreti;
            double odemeliUcretKentKart = 0;
            double odemeliUcretNakit = 0; 
            double odemeliUcretKrediKarti = 0;

            KrediKarti kredikarti = new KrediKarti();
            Nakit nakit = new Nakit();
            KentKart kentkart = new KentKart();

            if (isKentKartSelected)
            {
                Console.WriteLine($"Kentkart secilmis indirimsiz {totalWeight1}"); 
                odemeliUcretKentKart = kentkart.Hesapla(totalWeight1);
                Console.WriteLine($"Kentkart ucreti hesaplandi: {odemeliUcretKentKart}");
                LogToTerminal("Kentkart seçtiğiniz için indirimli ücret : " + odemeliUcretKentKart);
            }

            if (isNakitSelected)
            {
                Console.WriteLine($"Nakit secilmis indirimsiz {totalWeight1}");
                odemeliUcretNakit = nakit.Hesapla(totalWeight1);
                Console.WriteLine($"Nakit ucreti hesaplandi: {odemeliUcretNakit}");
                LogToTerminal("Nakit seçtiğiniz için indirimli ücret : " + odemeliUcretNakit);
            }

            if (isKrediKartiSelected)
            {
                Console.WriteLine($"krediakrt secilmis indirimsiz {totalWeight1}");
                odemeliUcretKrediKarti = kredikarti.Hesapla(totalWeight1);
                Console.WriteLine($"Kredikart ucreti hesaplandi: {odemeliUcretKrediKarti}");
                LogToTerminal("Kredi kartı seçtiğiniz için indirimli ücret: " + odemeliUcretKrediKarti);
            }

            LogToTerminal(string.Format("Toplam ücret: {0} \n TL Indirimli Ogrenci Ucreti : {2} TL \n Indirimli Yasli Ucreti : {3} TL                                           || Toplam süre: {1}", totalWeight1+taksiucreti , totalWeight2+taksiucreti , indirimliUcretOgrenci, indirimliUcretYasli));
            LogToTerminal("📍 Rotalar başarıyla çizildi.");

            if (points1.Count > 0 || points2.Count > 0)
            {
                gMapControl.ZoomAndCenterRoutes("route1");
                gMapControl.ZoomAndCenterRoutes("route2");
            }

            GMapOverlay taxiRouteOverlay = new GMapOverlay("taxiRoute");
            List<PointLatLng> taxiRoutePoints = new List<PointLatLng>
            {
                new PointLatLng(lat_konum, lon_konum), 
                new PointLatLng(lat_hedef, lon_hedef)  
            };

            GMapRoute taxiRoute = new GMapRoute(taxiRoutePoints, "Taksi Rota");
            taxiRoute.Stroke = new Pen(Color.MediumPurple, 3); 
            taxiRouteOverlay.Routes.Add(taxiRoute);

            gMapControl.Overlays.Add(taxiRouteOverlay);

            taksi.mesafe = mesafeHesapla.MesafeHesapla(lat_konum, lon_konum, lat_hedef, lon_hedef);
            taksiucreti = 0; 
            taksiucreti = taksi.UcretHesapla() + taksiucreti;


            LogToTerminal($"Direkt Taksi Rotası Ücreti: {taksiucreti} TL"); 
        }

        private void LogToTerminal(string message)
        {
            if (terminalBox != null && !terminalBox.IsDisposed)
            {
                terminalBox.AppendText("> " + message + Environment.NewLine);
            }
            else
            {
                Console.WriteLine("Hata: terminalBox nesnesi kapatılmış.");
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
                        MessageBox.Show("Tıklanan yerin açıklaması: " + description);
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
            public static List<Arac> shortestPath ;
            public static List<Arac> shortestPath2;
            public static Graph graph = new Graph();
            public static Graph graph2 = new Graph();

            public static void EnYakinDuragiBul(List<Arac> Durak, Taksi taksi, double lat_konum, double lon_konum, double hedef_lat, double hedef_lon , Form1 form)
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
                taksi.mesafe = mesafeHesapla.MesafeHesapla(lat_konum, lon_konum , enYakinDurak.lat , enYakinDurak.lon);
                double taksiucreti = 0;
                if(taksi.mesafe > 3)
                {
                    taksiucreti = taksi.UcretHesapla() + taksiucreti;
                }

                shortestPath = graph.GetShortestPath(enYakinDurak, hedefEnYakinDurak);
                shortestPath2 = graph2.GetShortestPath(enYakinDurak, hedefEnYakinDurak);

                if (enYakinDurak != null)
                {
                    Console.WriteLine($"En yakin baslangic duragi: {enYakinDurak.name}, {minMesafe:F2} km uzaklıkta.");
                    Console.WriteLine($"En yakin hedef duragi: {hedefEnYakinDurak.name}, {minMesafeHedef:F2} km uzaklıkta.");
                    totalWeight1 = totalWeight1;
                    if (Application.OpenForms["Form1"] == null)
                    {
                        Form1 yeniForm = new Form1(Durak, graph);
                        yeniForm.DrawRoutes(shortestPath, shortestPath2, taksi, taksiucreti,lat_konum, lon_konum , hedef_lat, hedef_lon);
                        Application.Run(yeniForm);
                    }
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

                List<Arac> aracDuraklari = new List<Arac>();

                aracDuraklari.AddRange(otobusDuraklari);
                aracDuraklari.AddRange(tramDuraklari);

                foreach (var durak in otobusDuraklari)
                {
                   yakinDurakBul.graph.AddNode(durak);
                }

                foreach (var durak in tramDuraklari)
                {
                    yakinDurakBul.graph.AddNode(durak);
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
                            yakinDurakBul.graph.AddEdge(otobus, matchingStop, ucretlist[i]);
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
                            yakinDurakBul.graph.AddEdge(tramvay, matchingStop, ucretlist[i]);
                            i++;
                        }
                        else
                        {
                            Console.WriteLine($"Ücret listesi sınırı aşıldı veya eşleşen durak bulunamadı: {nextStop}");
                        }
                    }
                }

                foreach (var durak in otobusDuraklari)
                {
                    yakinDurakBul.graph2.AddNode(durak);
                }

                foreach (var durak in tramDuraklari)
                {
                    yakinDurakBul.graph2.AddNode(durak);
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
                                yakinDurakBul.graph2.AddEdge(otobus, matchingStop, surelist[a]);
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
                                yakinDurakBul.graph2.AddEdge(tramvay, matchingStop, surelist[a]);
                                a++;
                            }
                            else
                            {
                                Console.WriteLine($"Süre listesi sınırı aşıldı veya eşleşen durak bulunamadı: {nextStop}");
                            }
                        }
                    }
                }
                Form1 form = new Form1(aracDuraklari, yakinDurakBul.graph);
                Application.Run(form);

                string[] value = form.coordinates.Split("/");
                double startLat = Convert.ToDouble(value[0]);
                double startLon = Convert.ToDouble(value[1]);
                double targetLat = Convert.ToDouble(value[2]);
                double targetLon = Convert.ToDouble(value[3]);

                yakinDurakBul.EnYakinDuragiBul(aracDuraklari, taksi, startLat, startLon, targetLat, targetLon , form);
            }
        }
    }
}
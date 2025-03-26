using System;
using System.Collections.Generic;
using System.Linq;
using UlasimRotaPlanlama.Models.Arac;

public class Graph
{
    public Dictionary<Arac, List<(Arac, int)>> AdjacencyList { get; set; }
    private Dictionary<Arac, Arac> previousNodes; // previousNodes burada tanımlanmalı

    public Graph()
    {
        AdjacencyList = new Dictionary<Arac, List<(Arac, int)>>();
        previousNodes = new Dictionary<Arac, Arac>(); // previousNodes'i başlatıyoruz
    }

    // Arac nesnesi eklemek
    public void AddNode(Arac arac)
    {
        if (!AdjacencyList.ContainsKey(arac))
            AdjacencyList[arac] = new List<(Arac, int)>();
    }

    // Edge eklemek
    public void AddEdge(Arac from, Arac to, int weight)
    {
        if (!AdjacencyList.ContainsKey(from))
        {
            Console.WriteLine($"Hata: {from.id} düğümü bulunamadı!");
            return;
        }

        AdjacencyList[from].Add((to, weight));
    }

    // Dijkstra algoritması
    public Dictionary<Arac, int> Dijkstra(Arac start)
    {
        var distances = new Dictionary<Arac, int>();
        var priorityQueue = new List<(Arac node, int distance)>();

        // Başlangıç düğümüne sıfır mesafe ver
        foreach (var node in AdjacencyList.Keys)
        {
            if (node.Equals(start))
                distances[node] = 0;
            else
                distances[node] = int.MaxValue;

            priorityQueue.Add((node, distances[node]));
            previousNodes[node] = null; // Başlangıçta önceki düğüm bilinmiyor
        }

        while (priorityQueue.Count > 0)
        {
            // Kuyruğu mesafeye göre sırala ve en düşük mesafeye sahip düğümü al
            var currentNode = priorityQueue.OrderBy(x => x.distance).First().node;
            priorityQueue.RemoveAll(x => x.node.Equals(currentNode));

            // Komşuları kontrol et
            foreach (var neighbor in AdjacencyList[currentNode])
            {
                var altDist = distances[currentNode] + neighbor.Item2;

                // Daha kısa mesafe bulunursa güncelle
                if (altDist < distances[neighbor.Item1])
                {
                    distances[neighbor.Item1] = altDist;
                    previousNodes[neighbor.Item1] = currentNode;
                    priorityQueue.Add((neighbor.Item1, altDist));
                }
            }
        }

        // Sonuçları döndür
        return distances;
    }

    // Dijkstra algoritmasından elde edilen en kısa yolu yazdırmak
    public void PrintShortestPath(Arac start, Arac end)
    {
        var distances = Dijkstra(start);
        var path = new Stack<Arac>();
        var currentNode = end;

        while (currentNode != null)
        {
            path.Push(currentNode);
            currentNode = previousNodes.ContainsKey(currentNode) ? previousNodes[currentNode] : null;
        }

        Console.WriteLine($"En kısa yol {start.id} -> {end.id}:");
        while (path.Count > 0)
        {
            Console.WriteLine(path.Pop().id);
        }
    }

    // Grafiği yazdırmak
    public void PrintGraph()
    {
        foreach (var node in AdjacencyList)
        {
            Console.WriteLine($"Node {node.Key.id}:");
            foreach (var neighbor in node.Value)
            {
                if (neighbor.Item1 != null)
                {
                    Console.WriteLine($"  -> {neighbor.Item1.id} (Ağırlık: {neighbor.Item2})");
                }
            }
        }
    }
}
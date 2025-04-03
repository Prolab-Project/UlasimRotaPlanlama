using System;
using System.Collections.Generic;
using System.Linq;
using UlasimRotaPlanlama.Models.Arac;

public class Graph
{
    public Dictionary<Arac, List<(Arac, double)>> AdjacencyList { get; set; }

    public Graph()
    {
        AdjacencyList = new Dictionary<Arac, List<(Arac, double)>>();
    }

    public void AddNode(Arac arac)
    {
        if (!AdjacencyList.ContainsKey(arac))
            AdjacencyList[arac] = new List<(Arac, double)>();
    }

    public void AddEdge(Arac from, Arac to, double weight, bool isTransfer = false)
    {
        if (!AdjacencyList.ContainsKey(from))
        {
            Console.WriteLine($"Hata: {from.id} düğümü bulunamadı!");
            return;
        }
        if (isTransfer)
        {
            weight += 0.5; // Örnek transfer ücreti ekleniyor
        }
        if (!AdjacencyList.ContainsKey(to))
            AdjacencyList[to] = new List<(Arac, double)>();

        AdjacencyList[from].Add((to, weight));
        AdjacencyList[to].Add((from, weight)); 
    }

    public List<Arac> GetShortestPath(Arac start, Arac end)
    {
        Dictionary<Arac, double> distances = new Dictionary<Arac, double>();
        Dictionary<Arac, Arac> previous = new Dictionary<Arac, Arac>();
        List<Arac> nodes = new List<Arac>();

        foreach (var node in AdjacencyList.Keys)
        {
            distances[node] = double.MaxValue;
            previous[node] = null;
            nodes.Add(node);
        }

        distances[start] = 0;

        while (nodes.Count > 0)
        {
            Arac smallest = nodes.OrderBy(n => distances[n]).First();
            
            if (smallest == end)
                break;
            
            nodes.Remove(smallest);

            foreach (var neighbor in AdjacencyList[smallest])
            {
                double alt = distances[smallest] + neighbor.Item2;
                if (alt < distances[neighbor.Item1])
                {
                    distances[neighbor.Item1] = alt;
                    previous[neighbor.Item1] = smallest;
                }
            }
        }

        List<Arac> path = new List<Arac>();
        Arac current = end;

        if (previous[current] == null && current != start)
        {
            return path;
        }

        while (current != null)
        {
            path.Add(current);
            current = previous[current];
        }

        path.Reverse(); 
        return path;
    }

    public double GetEdgeWeight(Arac from, Arac to)
    {
        if (AdjacencyList.ContainsKey(from))
        {
            var edges = AdjacencyList[from];
            var edge = edges.FirstOrDefault(e => e.Item1 == to);
            if (edge != default)
            {
                return edge.Item2; // Ağırlığı döndür
            }
        }
        throw new Exception($"Hata: {from.id} ile {to.id} arasında bir bağlantı bulunamadı!");
    }
}

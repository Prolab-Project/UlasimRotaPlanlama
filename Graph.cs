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

    public (Dictionary<Arac, double> distances, Dictionary<Arac, Arac> previousNodes) Dijkstra(Arac start)
    {
        var distances = new Dictionary<Arac, double>();
        var previousNodes = new Dictionary<Arac, Arac>();
        var priorityQueue = new SortedSet<(double, Arac)>(Comparer<(double, Arac)>.Create((a, b) =>
            a.Item1 == b.Item1 ? a.Item2.id.CompareTo(b.Item2.id) : a.Item1.CompareTo(b.Item1)
        ));

        foreach (var node in AdjacencyList.Keys)
        {
            distances[node] = int.MaxValue;
            previousNodes[node] = null;
        }

        distances[start] = 0;
        priorityQueue.Add((0, start));

        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Min;
            priorityQueue.Remove(current);

            var currentNode = current.Item2;
            double currentDistance = current.Item1;

            if (distances[currentNode] < currentDistance)
                continue;

            foreach (var (neighbor, weight) in AdjacencyList[currentNode])
            {
                double altDist = distances[currentNode] + weight;

                if (altDist < distances[neighbor])
                {
                    priorityQueue.Remove((distances[neighbor], neighbor));  
                    distances[neighbor] = altDist;
                    previousNodes[neighbor] = currentNode;
                    priorityQueue.Add((altDist, neighbor)); 
                }
            }
        }

        return (distances, previousNodes);
    }

    public void PrintShortestPath(Arac start, Arac end)
    {
        var (distances, previousNodes) = Dijkstra(start); 

        if (!distances.ContainsKey(end) || distances[end] == int.MaxValue)
        {
            Console.WriteLine($"Hata: {start.id} ile {end.id} arasında bir bağlantı bulunamadı!");
            return;
        }

        var path = new Stack<Arac>();
        var currentNode = end;

        while (currentNode != null)
        {
            path.Push(currentNode);
            currentNode = previousNodes.ContainsKey(currentNode) ? previousNodes[currentNode] : null;
        }

        Console.Write($"En kısa yol {start.id} -> {end.id} (Mesafe: {distances[end]}): ");
        while (path.Count > 0)
        {

            Console.Write(path.Pop().id);
            if (path.Count > 0) Console.Write(" -> ");
        }
        Console.WriteLine();
    }

    public void PrintGraph()
    {
        foreach (var node in AdjacencyList)
        {
            Console.WriteLine($"Node {node.Key.id}:");
            foreach (var neighbor in node.Value)
            {
                Console.WriteLine($"  -> {neighbor.Item1.id} (Ağırlık: {neighbor.Item2})");
            }
        }
    }
}

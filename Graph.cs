using System;
using System.Collections.Generic;
using System.Linq;

public class Graph
{
    public Dictionary<string, List<(string, int)>> AdjacencyList { get; set; }

    public Graph()
    {
        AdjacencyList = new Dictionary<string, List<(string, int)>>();
    }

    public void AddNode(string id)
    {
        if (!AdjacencyList.ContainsKey(id))
            AdjacencyList[id] = new List<(string, int)>();
    }

    public void AddEdge(string from, string to, int weight)
    {
        if (to == "bus_otogar" || to  == "bus_sekapark" || to == "bus_yahyakaptan" || to == "bus_umuttepe" || to == "bus_symbolavm" || to == "bus_41burda" || to == "tram_otogar" || to == "tram_yahyakaptan" || to == "tram_sekapark" || to == "tram_halkevi") 
        {
            if (AdjacencyList.ContainsKey(from))
                AdjacencyList[from].Add((to, weight));
            else
            {
                Console.WriteLine($"Hata: {from} düğümü bulunamadı!");
            }
            //Console.WriteLine(weight);
        }
    }

    public List<string> Dijkstra(Graph graph, string start, string goal)
    {
        var distances = new Dictionary<string, int>();
        var previous = new Dictionary<string, string>();
        var queue = new SortedSet<(int, string)>();

        if (!graph.AdjacencyList.ContainsKey(start))
        {
            Console.WriteLine($"Hata: Başlangıç düğümü {start} bulunamadı!");
            return new List<string>();
        }

        if (!graph.AdjacencyList.ContainsKey(goal))
        {
            Console.WriteLine($"Hata: Hedef düğüm {goal} bulunamadı!");
            return new List<string>();
        }

        foreach (var node in graph.AdjacencyList.Keys)
        {
            distances[node] = int.MaxValue;
            previous[node] = null;
        }
        distances[start] = 0;
        queue.Add((0, start));

        while (queue.Count > 0)
        {
            var (currentDistance, currentNode) = queue.First();
            queue.Remove(queue.First());

            if (currentNode == goal)
                break;

            foreach (var (neighbor, weight) in graph.AdjacencyList[currentNode])
            {
                int newDistance = currentDistance + weight;

                if (newDistance < distances[neighbor])
                {
                    queue.Remove((distances[neighbor], neighbor));
                    distances[neighbor] = newDistance;
                    previous[neighbor] = currentNode;
                    queue.Add((newDistance, neighbor));
                }
            }
        }

        return ReconstructPath(previous, start, goal);
    }


    public void PrintGraph()
    {
        foreach (var node in AdjacencyList)
        {
            Console.WriteLine($"Node {node.Key}:");
            foreach (var neighbor in node.Value)
            {
                Console.WriteLine($"  -> {neighbor}");
            }
        }
    }
    private List<string> ReconstructPath(Dictionary<string, string> previous, string start, string goal)
    {
        var path = new List<string>();
        var current = goal;
        while (current != null)
        {
            path.Insert(0, current);
            current = previous[current];
        }

        // Eğer path sadece başlangıç düğümünü içeriyorsa, yol bulunamadı demektir
        if (path.Count == 1 && path[0] != start)
        {
            return new List<string>(); // Boş yol, yani ulaşılabilir bir hedef yok
        }

        return path;
    }

}

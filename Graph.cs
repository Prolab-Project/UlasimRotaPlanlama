using System;
using System.Collections.Generic;
using System.Linq;
using UlasimRotaPlanlama.Models.Arac;

public class Graph
{
    public Dictionary<Arac, List<(Arac, int)>> AdjacencyList { get; set; }
    private Dictionary<Arac, Arac> previousNodes; 
    public Graph()
    {
        AdjacencyList = new Dictionary<Arac, List<(Arac, int)>>();
        previousNodes = new Dictionary<Arac, Arac>();
    }

    public void AddNode(Arac arac)
    {
        if (!AdjacencyList.ContainsKey(arac))
            AdjacencyList[arac] = new List<(Arac, int)>();
    }

    public void AddEdge(Arac from, Arac to, int weight)
    {
        if (!AdjacencyList.ContainsKey(from))
        {
            Console.WriteLine($"Hata: {from.id} düğümü bulunamadı!");
            return;
        }

        AdjacencyList[from].Add((to, weight));
    }

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
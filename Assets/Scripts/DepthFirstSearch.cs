using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DepthFirstSearch
{
    // Main DFS function that initializes the visited array
    // and calls DfsRec
    public List<int> DFS(List<int>[] adj)
    {
        bool[] visited = new bool[adj.Length];
        List<int> res = new List<int>();
        DfsRec(adj, visited, 0, res);
        return res;
    }

    public void DfsRec(List<int>[] adj, bool[] visited, int s, List<int> res)
    {
        visited[s] = true;
        res.Add(s);

        // Recursively visit all adjacent vertices that are
        // not visited yet
        foreach (int i in adj[s])
        {
            if (!visited[i])
            {
                DfsRec(adj, visited, i, res);
            }
        }
    }

    public void TraverseGraph()
    {

    }
}

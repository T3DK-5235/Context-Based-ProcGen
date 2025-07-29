using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using System.Linq;
using System;

public class Graph
{
    public Node initialNode;
    //TODO check if a dict of total nodes would be better. Or an additional dict assigning an id, or the grid pos to the node
    public List<Node> totalNodeList;
    private int idIncrementer;
    private SO_MapGenerationValues genValues;

    public Graph(int initialNodeID, Vector2Int initialNodePos, SO_MapGenerationValues genValues)
    {
        initialNode = new Node(initialNodeID, initialNodePos, genValues);
        totalNodeList = new List<Node>();
        // This value will increase whenever a new node is created.
        // Starts at 1, as 0 is reserved for the root node.
        idIncrementer = 1;
        this.genValues = genValues;
    }

    // Used to clear out the existing graph if mapgen fails
    public void ClearGraph()
    {
        for (int i = 0; i < totalNodeList.Count(); i++)
        {
            totalNodeList[i].ClearConnections();
        }
        totalNodeList.Clear();
        idIncrementer = 1;
    }

    public Node AddNode(Vector2Int gridPos)
    {
        Node newNode = new Node(idIncrementer++, gridPos, genValues);
        totalNodeList.Add(newNode);
        return newNode;
    }

    public void AddNodeConnection(Node originNode, Node destinationNode)
    {
        originNode.AddConnection(destinationNode);
    }

    // use nullable values to fill in empty squares
    public int?[,] CreateAdjacencyMatrix()
    {
        int?[,] adjMatrix = new int?[totalNodeList.Count, totalNodeList.Count];

        for (int i = 0; i < totalNodeList.Count; i++)
        {
            Node originTestNode = totalNodeList[i];

            for (int j = 0; j < totalNodeList.Count; j++)
            {
                Node destinationTestNode = totalNodeList[j];

                Connection connectionCheck = originTestNode.connections.FirstOrDefault(x => x.child == destinationTestNode);

                if (connectionCheck != null)
                {
                    //TODO figure out if I want edge weighting instead of just having a "1" to denote a present adjacency/connection
                    adjMatrix[i, j] = 1;
                }
            }
        }

        PrintAdjMatrix(adjMatrix);

        return adjMatrix;
    }


    public void PrintAdjMatrix(int?[,] adjMatrix)
    {
        int Count = totalNodeList.Count;
        string topbar = "";
        for (int i = 0; i < Count; i++)
        {
            topbar += (totalNodeList[i].id + "  ");
        }
        Debug.Log(topbar);

        for (int i = 0; i < Count; i++)
        {
            //Debug.Log((char)('A' + i) + " | [ ");
            String outputRow = totalNodeList[i].id + " | [ ";
            for (int j = 0; j < Count; j++)
            {
                if (i == j)
                {
                    outputRow += ("  &,");
                }
                else if (adjMatrix[i, j] == null || adjMatrix[i, j] == 0)
                {
                    outputRow += ("  .,");
                }
                else
                {
                    outputRow += ("  " + adjMatrix[i, j] + ",");
                }

            }
            Debug.Log(outputRow + " ]\r\n");
        }
        //Debug.Log("\r\n");
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayoutManager : MonoBehaviour
{
    private List<Node> unvisitedNodes;
    private List<Node> visitedNodes;
    private Node startingNode;

    //private SO_LayoutParameters layoutParameters;

    void Start()
    {
        unvisitedNodes = new List<Node>();
        visitedNodes = new List<Node>();

        InitNodes();
        InitGraph();
    }

    //Return the initial starting node for the graph
    private Node InitNodes()
    {
        //TODO generate all nodes, number of nodes is gotten from generation size from within layout parameters
        //TODO entryway node needs to be assigned to starting node and added to visited Nodes

        return null;
    }

    private void InitGraph()
    {
        //GraphBase initialGraph = new GraphBase(startingNode);
    }
}

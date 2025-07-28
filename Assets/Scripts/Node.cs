using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class Connection
{
    // int weight;
    public Node parent { get; set; }
    public Node child { get; set; }
    public Connection(Node parent, Node child)
    {
        this.parent = parent;
        this.child = child;
    }

    //public Node getParent() { return parent; }
    //public Node getChild() { return child; }
}

public class Node
{
    public int id { get; }
    public Vector2Int gridPos { get; }
    public List<Connection> connections { get; }

    public GameObject directionalRoomPrefab { get; set; }
    public E_CardinalDirections[] occupiedCardinalDirections;

    public Node(int id, Vector2Int gridPos)
    {
        this.id = id;
        this.gridPos = gridPos;
        connections = new List<Connection>();
        occupiedCardinalDirections = new E_CardinalDirections[4];
    }

    public Node AddConnection(Node childNode)
    {
        Connection newConnection = new Connection(this, childNode);
        connections.Add(newConnection);

        // Checks if a connection from the childnode to this node exists yet, and adds it if not.
        List<Connection> childConnections = childNode.connections;
        if (!(childConnections.Exists(x => (x.parent == childNode) && (x.child == this)))) { childNode.AddConnection(this); }

        return this;
    }

    public void ClearConnections()
    {
        connections.Clear();
    }

    public void AddCardinalNeighbour(E_CardinalDirections occupiedDirection)
    {
        occupiedCardinalDirections[(int)occupiedDirection] = occupiedDirection;
    }

    //public List<Connection> GetConnections() { return connections; }

}

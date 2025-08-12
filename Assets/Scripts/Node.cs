using System;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject basicMapPrefab { get; set; }
    public int[] occupiedCardinalDirections;

    //TODO need to pass this in from MapManager
    private SO_MapGenerationValues genValues;

    public (int, GameObject) basicRoomData;
    // 
    // private SO_RoomType relevantRoomType;
    // // The current tags for the room
    // List<E_RoomTags> roomTags { get; set; }

    // List<GameObject> relevantRoomTags { get; set; }

    public SO_RoomType roomType { get; set; }
    public List<GameObject> relevantRoomPrefabs { get; set; }

    // Room Orientation Handling 
    // Dead End
    public static int[] defaultDeadEndConnections = { 0, 0, 1, 0 };
    // 2 Entrances
    public static int[] defaultCornerConnections = { 0, 0, 1, 1 };
    public static int[] defaultHallwayConnections = { 1, 0, 1, 0 };
    // 3 Entrances
    public static int[] defaultTShapeConnections = { 0, 1, 1, 1 };

    public Node(int id, Vector2Int gridPos, SO_MapGenerationValues genValues)
    {
        this.id = id;
        this.gridPos = gridPos;
        this.genValues = genValues;
        connections = new List<Connection>();
        // N/E/S/W
        occupiedCardinalDirections = new int[4];
        relevantRoomPrefabs = new List<GameObject>();
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

    public void AddCardinalNeighbourSet(int[] occupiedDirection)
    {
        this.occupiedCardinalDirections = occupiedDirection;
    }

    public int[] GetCardinalNeighbourSet()
    {
        return occupiedCardinalDirections;
    }

    public void SetupBasicRoom(int neighbourCount)
    {
        //! this currently decides based on neighbours, even if those neighbours shouldn't be connected
        //! could be better to decide based on connections?
        int rotationDegree = 0;
        GameObject relevantPrefab = null;
        switch (neighbourCount)
        {
            case 1:
                relevantPrefab = genValues.basicDeadEnd;
                if (occupiedCardinalDirections.SequenceEqual(defaultDeadEndConnections)) { break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] == 1) { rotationDegree = 90; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] == 1) { rotationDegree = 270; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] == 1) { rotationDegree = 180; break; }

                break;
            case 2:
                int[] swappedHallwayConnections = new int[defaultHallwayConnections.Length];
                Array.Copy(defaultHallwayConnections, swappedHallwayConnections, defaultHallwayConnections.Length);
                // Reversing the array creates the same array as a East to West hallway
                Array.Reverse(swappedHallwayConnections);

                //TODO remove this after testing
                string providedCardinalDirections = "";
                for (int i = 0; i < occupiedCardinalDirections.Length; i++)
                {
                    providedCardinalDirections += ", " + occupiedCardinalDirections[i];
                }
                //Debug.Log("Node ID: " + id + " --- providedCardinalDirections: " + providedCardinalDirections);

                // This will be true for straight line segments heading either north to south OR west to east
                //if (occupiedCardinalDirections == defaultHallwayConnections || occupiedCardinalDirections == swappedHallwayConnections)
                if (occupiedCardinalDirections.SequenceEqual(defaultHallwayConnections) || occupiedCardinalDirections.SequenceEqual(swappedHallwayConnections))
                {
                    relevantPrefab = genValues.basicHallway;

                    if (occupiedCardinalDirections.SequenceEqual(defaultHallwayConnections)) { break; }
                    else { rotationDegree = 90; break; }
                }
                else
                {
                    relevantPrefab = genValues.basicCorner;

                    if (occupiedCardinalDirections.SequenceEqual(defaultCornerConnections)) { rotationDegree = 270; break; }
                    // If the default (South and West) isn't correct, but there is still a south connection, the corner must face right (East)
                    if (occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] == 1) { break; }
                    // If the previous two are incorrect, but there is still an east connection, the corner must connect the North and East sides
                    if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] == 1) { rotationDegree = 90; break; }
                    if (occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] == 1) { rotationDegree = 180; break; }
                }

                break;
            case 3:
                relevantPrefab = genValues.basicTJunction;
                // If the occupied directions are the same as the default for the room, do nothing
                if (occupiedCardinalDirections.SequenceEqual(defaultTShapeConnections)) { break; }
                // If there is a not a southern connection (default entryway direction) return 180, as none of the 3 connections can be south
                if (occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] != 1) { rotationDegree = 180; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] != 1) { rotationDegree = 270; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] != 1) { rotationDegree = 90; break; }
                break;
            default:
                // If 4 exits are found, no rotation is needed as all cardinal directions are occupied
                relevantPrefab = genValues.basicCrossJunction;
                break;
        }
        basicRoomData = (rotationDegree, relevantPrefab);
    }

    //public List<Connection> GetConnections() { return connections; }

}

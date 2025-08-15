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
    public E_CardinalDirections travelDirection { get; set; }
    //travel direction is from parent (the node this is stored in) to child
    public Connection(Node parent, Node child, E_CardinalDirections travelDirection)
    {
        this.parent = parent;
        this.child = child;
        this.travelDirection = travelDirection;
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
    private int[] occupiedCardinalDirections;

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
    // Default: Open area on the north
    public static int[] defaultDeadEndConnections = { 1, 0, 0, 0 };
    // 2 Entrances
    // Default: Open areas on the north and east
    public static int[] defaultCornerConnections = { 1, 0, 1, 0 };
    public static int[] defaultHallwayConnections = { 1, 1, 0, 0 };
    // 3 Entrances
    public static int[] defaultTShapeConnections = { 1, 0, 1, 1 };

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

    public Node AddConnection(Node childNode, E_CardinalDirections expansionDirection)
    {
        Debug.Log("Adding new connection from NodeID: " + id + " to NodeID:" + childNode.id + " from the parent the direction is: " + expansionDirection.ToString());

        Connection newConnection = new Connection(this, childNode, expansionDirection);
        connections.Add(newConnection);

        // Checks if a connection from the childnode to this node exists yet, and adds it if not.
        List<Connection> childConnections = childNode.connections;

        E_CardinalDirections inverseDirection;

        // Positive directions (+x, +y etc) are stored as odd numbers and the reverse are stored as even numbers
        if ((int)expansionDirection % 2 == 0) { inverseDirection = (E_CardinalDirections)((int)expansionDirection + 1); }
        else { inverseDirection = (E_CardinalDirections)((int)expansionDirection - 1); }

        if (!(childConnections.Exists(x => (x.parent == childNode) && (x.child == this)))) { childNode.AddConnection(this, inverseDirection); }

        return this;
    }

    public void ClearConnections()
    {
        connections.Clear();
    }

    public void AddCardinalNeighbourSet(int[] occupiedDirection)
    {
        // Instead of checking the nearby nodes, it checks the direction of any connections it has

        // string test = "NodeID: " + id + "------------ Initial occupiedDirection: ";
        // for (int i = 0; i < occupiedDirection.Count(); i++) { test += occupiedDirection[i] + "."; }
        // Debug.Log(test);

        // string test3 = "NodeID: " + id + "------------ connections directions: ";
        // for (int i = 0; i < connections.Count(); i++) { test3 += connections[i].travelDirection.ToString() + "."; }
        // Debug.Log(test3);

        // Array.Clear(occupiedDirection, 0, occupiedDirection.Length);

        int[] newOccupiedDirection = new int[4];

        for (int i = 0; i < occupiedCardinalDirections.Count(); i++)
        {
            Debug.Log("occupiedCardinalDirections[i]: " + occupiedCardinalDirections[i]);
            newOccupiedDirection[i] = occupiedCardinalDirections[i];
            Debug.Log("newOccupiedDirection[i]: " + newOccupiedDirection[i]);
        }
        Debug.Log("Connection count: " + connections.Count);

        for (int i = 0; i < connections.Count(); i++)
        {
            //This node instance acts as the parent, so all travel directions will be towards the occupied children
            if (connections[i].travelDirection == E_CardinalDirections.NORTH) { newOccupiedDirection[(int)E_CardinalDirections.NORTH] = 1; }
            else if (connections[i].travelDirection == E_CardinalDirections.SOUTH) { newOccupiedDirection[(int)E_CardinalDirections.SOUTH] = 1; }
            else if (connections[i].travelDirection == E_CardinalDirections.EAST) { newOccupiedDirection[(int)E_CardinalDirections.EAST] = 1; }
            else if (connections[i].travelDirection == E_CardinalDirections.WEST) { newOccupiedDirection[(int)E_CardinalDirections.WEST] = 1; }
        }

        string test2 = "NodeID: " + id + "------------ Edited occupiedDirection: ";
        for (int i = 0; i < newOccupiedDirection.Count(); i++) { test2 += newOccupiedDirection[i] + "."; }
        Debug.Log(test2);

        occupiedCardinalDirections = newOccupiedDirection;

        //Array.Copy(newOccupiedDirection, occupiedCardinalDirections, newOccupiedDirection.Length - 1);

        //int[] testoccupiedCardinalDirections = this.occupiedCardinalDirections;

        string test3 = "NodeID: " + id + "------------ Edited occupiedDirection Stored: ";
        for (int i = 0; i < occupiedCardinalDirections.Count(); i++) { test2 += occupiedCardinalDirections[i] + "."; }
        Debug.Log(test3);
    }

    public int[] GetCardinalNeighbourSet()
    {
        return occupiedCardinalDirections;
    }

    public void SetCardinalNeighbourSet(int newOccupiedIndex)
    {
        this.occupiedCardinalDirections[newOccupiedIndex] = 1;
    }
    
    //! temp for debugging

    // public void SetupBasicRoom(int neighbourCount)
    // {
    //     //! this currently decides based on neighbours, even if those neighbours shouldn't be connected
    //     //! could be better to decide based on connections?
    //     int rotationDegree = 0;
    //     GameObject relevantPrefab = null;
    //     switch (neighbourCount)
    //     {
    //         case 1:
    //             relevantPrefab = genValues.basicDeadEnd;
    //             if (occupiedCardinalDirections.SequenceEqual(defaultDeadEndConnections)) { break; }
    //             if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] == 1) { rotationDegree = 90; break; }
    //             if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] == 1) { rotationDegree = 270; break; }
    //             if (occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] == 1) { rotationDegree = 180; break; }

    //             break;
    //         case 2:
    //             int[] swappedHallwayConnections = new int[defaultHallwayConnections.Length];
    //             Array.Copy(defaultHallwayConnections, swappedHallwayConnections, defaultHallwayConnections.Length);
    //             // Reversing the array creates the same array as a East to West hallway
    //             Array.Reverse(swappedHallwayConnections);

    //             //TODO remove this after testing
    //             string providedCardinalDirections = "";
    //             for (int i = 0; i < occupiedCardinalDirections.Length; i++)
    //             {
    //                 providedCardinalDirections += ", " + occupiedCardinalDirections[i];
    //             }
    //             //Debug.Log("Node ID: " + id + " --- providedCardinalDirections: " + providedCardinalDirections);

    //             // This will be true for straight line segments heading either north to south OR west to east
    //             //if (occupiedCardinalDirections == defaultHallwayConnections || occupiedCardinalDirections == swappedHallwayConnections)
    //             if (occupiedCardinalDirections.SequenceEqual(defaultHallwayConnections) || occupiedCardinalDirections.SequenceEqual(swappedHallwayConnections))
    //             {
    //                 relevantPrefab = genValues.basicHallway;

    //                 if (occupiedCardinalDirections.SequenceEqual(defaultHallwayConnections)) { break; }
    //                 else { rotationDegree = 90; break; }
    //             }
    //             else
    //             {
    //                 relevantPrefab = genValues.basicCorner;

    //                 if (occupiedCardinalDirections.SequenceEqual(defaultCornerConnections)) { rotationDegree = 270; break; }
    //                 // If the default (South and West) isn't correct, but there is still a south connection, the corner must face right (East)
    //                 if (occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] == 1) { break; }
    //                 // If the previous two are incorrect, but there is still an east connection, the corner must connect the North and East sides
    //                 if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] == 1) { rotationDegree = 90; break; }
    //                 if (occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] == 1) { rotationDegree = 180; break; }
    //             }

    //             break;
    //         case 3:
    //             relevantPrefab = genValues.basicTJunction;
    //             // If the occupied directions are the same as the default for the room, do nothing
    //             if (occupiedCardinalDirections.SequenceEqual(defaultTShapeConnections)) { break; }
    //             // If there is a not a southern connection (default entryway direction) return 180, as none of the 3 connections can be south
    //             if (occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] != 1) { rotationDegree = 180; break; }
    //             if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] != 1) { rotationDegree = 270; break; }
    //             if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] != 1) { rotationDegree = 90; break; }
    //             break;
    //         default:
    //             // If 4 exits are found, no rotation is needed as all cardinal directions are occupied
    //             relevantPrefab = genValues.basicCrossJunction;
    //             break;
    //     }
    //     basicRoomData = (rotationDegree, relevantPrefab);
    // }

    public void SetupBasicRoom()
    {
        int rotationDegree = 0;
        GameObject relevantPrefab = null;

        string test2 = "Test Node ID" + id + " ------------ Edited occupiedDirection: ";
        for (int i = 0; i < occupiedCardinalDirections.Count(); i++) { test2 += occupiedCardinalDirections[i] + "."; }
        Debug.Log(test2);

        switch (connections.Count)
        {

            case 1:
                relevantPrefab = genValues.basicDeadEnd;
                if (occupiedCardinalDirections.SequenceEqual(defaultDeadEndConnections)) { rotationDegree = 0; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] == 1) { rotationDegree = 270; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] == 1) { rotationDegree = 90; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] == 1) { rotationDegree = 180; break; }

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
                    // If the default (North and East) isn't correct, but there is still a north connection, the corner must face left (West)
                    if (occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] == 1) { rotationDegree = 0; break; }
                    // If the previous two are incorrect, but there is still an west connection, the corner must connect the South and West sides
                    if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] == 1) { rotationDegree = 90; break; }
                    if (occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] == 1) { rotationDegree = 180; break; }
                }

                break;
            case 3:
                relevantPrefab = genValues.basicTJunction;
                // If the occupied directions are the same as the default for the room (north east west), do nothing
                if (occupiedCardinalDirections.SequenceEqual(defaultTShapeConnections)) { rotationDegree = 0; break; }
                // If there is a not a northern connection (default entryway direction) return 180, as none of the 3 connections can be west
                if (occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] != 1) { rotationDegree = 180; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.EAST] != 1) { rotationDegree = 90; break; }
                if (occupiedCardinalDirections[(int)E_CardinalDirections.WEST] != 1) { rotationDegree = 270; break; }
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

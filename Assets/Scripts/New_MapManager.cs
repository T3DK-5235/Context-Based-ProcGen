using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;


public class New_MapManager : MonoBehaviour
{
    private int[,] mapArray;

    private int mapArrayCount;
    private int minNodes;
    private int maxNodes;
    private List<Vector2Int> endRooms;

    public int gridSizeX;
    public int gridSizeY;


    public New_TempCell cellPrefab;
    private float cellSize;
    private Queue<Vector2Int> cellQueue;
    private List<Node> spawnedNodes;
    List<GameObject> spawnedRooms;

    private Graph nodeGraph;

    public int initialCellX = 5;
    public int initialCellY = 6;


    [SerializeField] SO_MapGenerationValues mapGenValues;
    [SerializeField] GameObject newRoom;

    void Start()
    {
        minNodes = 16;
        maxNodes = 24;

        cellSize = 0.5f;
        spawnedNodes = new List<Node>();
        spawnedRooms = new List<GameObject>();

        nodeGraph = new Graph(0, new Vector2Int(initialCellX, initialCellY), mapGenValues);

        SetupMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetupMap();
        }
    }

    //TODO need to bring in graph from Layout Manager?

    void SetupMap()
    {
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Destroy(spawnedNodes[i].directionalRoomPrefab);
        }

        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            Destroy(spawnedRooms[i]);
        }

        nodeGraph.ClearGraph();
        spawnedNodes.Clear();
        spawnedRooms.Clear();

        mapArray = new int[gridSizeX, gridSizeY];
        mapArrayCount = default;
        cellQueue = new Queue<Vector2Int>();
        endRooms = new List<Vector2Int>();

        Vector2Int initialPosition = new Vector2Int(initialCellX, initialCellY);
        VisitCell(initialPosition, initialPosition, E_CardinalDirections.NONE);

        GenerateMap();
    }

    void GenerateMap()
    {
        while (cellQueue.Count > 0)
        {
            Vector2Int cellPos = cellQueue.Dequeue();

            bool created = false;

            // Currently hardcoded to be a certain size
            // if (cellPos.x > 1) { created |= VisitCell(new Vector2Int(xPos - 1, cellPos.y)); }
            // if (cellPos.x < 9) { created |= VisitCell(new Vector2Int(xPos + 1, yPos)); }
            // if (cellPos.y > 2) { created |= VisitCell(new Vector2Int(xPos, yPos - 1)); }
            // if (cellPos.y < 7) { created |= VisitCell(new Vector2Int(xPos, yPos + 1)); }

            // -1 is up, as it goes backwards through in values to prior cells of the array
            // Vector2Int north = new Vector2Int(-1, 0) + cellPos;
            // Vector2Int south = new Vector2Int(1, 0) + cellPos;
            // Vector2Int east = new Vector2Int(0, -1) + cellPos;
            // Vector2Int west = new Vector2Int(0, 1) + cellPos;

            // int EXPANSION_DIRECTIONS = 4;
            // int[] expansionChances = new int[EXPANSION_DIRECTIONS];

            // for (int i = 0; i < EXPANSION_DIRECTIONS; i++)
            // {
            //     int random = Random.Range(0, 10);

            // }

            //Debug.Log("x position: " + cellPos.x + " --- y position: " + cellPos.y);

            // Cascades through the if statements and attempts to create new cells
            if (cellPos.x > 1)
            {
                created |= VisitCell(new Vector2Int(cellPos.x - 1, cellPos.y), cellPos, E_CardinalDirections.NORTH);
            }
            if (cellPos.x < 8)
            {
                created |= VisitCell(new Vector2Int(cellPos.x + 1, cellPos.y), cellPos, E_CardinalDirections.EAST);
            }
            if (cellPos.y > 2)
            {
                created |= VisitCell(new Vector2Int(cellPos.x, cellPos.y - 1), cellPos, E_CardinalDirections.WEST);
            }
            if (cellPos.y < 7)
            {
                created |= VisitCell(new Vector2Int(cellPos.x, cellPos.y + 1), cellPos, E_CardinalDirections.SOUTH);
            }

            // if a new cell is not created, add the existing index to the endrooms list
            if (created == false)
            {
                endRooms.Add(cellPos);
            }

        }

        // Not enough rooms
        Debug.Log("mapArrayCount: " + mapArrayCount);
        if (mapArrayCount < minNodes)
        {
            SetupMap();
            nodeGraph.ClearGraph();
            return;
        }

        Debug.Log("Finished Physical Map Gen");
        Debug.Log("End Room Number: " + endRooms.Count);
        nodeGraph.CreateAdjacencyMatrix();

        //TODO figure out if to turn "4" into a const up top to avoid magic numbers
        // N/E/S/W
        int[] occupiedCardinalDirections = new int[4];
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Array.Clear(occupiedCardinalDirections, 0, occupiedCardinalDirections.Length);

            int cellPosX = spawnedNodes[i].gridPos.x;
            int cellPosY = spawnedNodes[i].gridPos.y;

            // Check which directions from the node are occupied by another node
            if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] = 1; }
            if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] = 1; }
            if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.EAST] = 1; }
            if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.WEST] = 1; }

            spawnedNodes[i].AddCardinalNeighbourSet(occupiedCardinalDirections);
            (int, GameObject) basicRoomData = spawnedNodes[i].SetupBasicRoom(GetNeighbourCount(spawnedNodes[i].gridPos.x, spawnedNodes[i].gridPos.y));

            Vector3 roomPhysicalPosition = new Vector3(cellPosX * (cellSize * 30), 0, -cellPosY * (cellSize * 30));
            Debug.Log("Node ID: " + spawnedNodes[i].id + " --- Rotation amount = " + basicRoomData.Item1);
            Quaternion roomRotation = Quaternion.Euler(0, basicRoomData.Item1, 0);

            GameObject newRoom = Instantiate(basicRoomData.Item2, roomPhysicalPosition, roomRotation);
            spawnedRooms.Add(newRoom);
            //TODO BIG TODO - DELETE OLD PREFABS WHEN CREATING NEW MAP
        }

        //TODO loop through node array, check number of neighbours and cardinal directions, use these values to determine the basic room shape
        //TODO use bool isEqual = Enumerable.SequenceEqual(target1, target2); to check patterns of room directions for rotation needs
    }

    int GetRandomEndRoom()
    {
        return -1;
    }

    private int GetNeighbourCount(int cellPosX, int cellPosY)
    {
        int occupiedNeighbours = 0;
        if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedNeighbours++; }
        if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedNeighbours++; }
        if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedNeighbours++; }
        if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedNeighbours++; }
        return occupiedNeighbours;
    }

    // private int[] GetNeighbouringCellDirections(Node centralNode)
    // {
    //     int cellPosX = centralNode.gridPos.x;
    //     int cellPosY = centralNode.gridPos.y;

    //     int[] occupiedCardinalDirections = new int[4];

    //     if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] = 1; }
    //     if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] = 1; }
    //     if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.EAST] = 1; }
    //     if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.WEST] = 1; }
    //     return occupiedCardinalDirections;
    // }

    private bool VisitCell(Vector2Int cellPos, Vector2Int previousCellPos, E_CardinalDirections traversalDirection)
    {
        if ((mapArray[cellPos.x, cellPos.y] != 0) || (GetNeighbourCount(cellPos.x, cellPos.y) > 1) || (mapArrayCount > maxNodes) || (UnityEngine.Random.value < 0.4))
        {
            return false;
        }

        cellQueue.Enqueue(cellPos);
        mapArray[cellPos.x, cellPos.y] = 1;
        mapArrayCount++;

        SpawnRoom(cellPos, previousCellPos, traversalDirection);

        return true;
    }

    private void SpawnRoom(Vector2Int cellPos, Vector2Int previousCellPos, E_CardinalDirections traversalDirection)
    {
        Vector2 position = new Vector2(cellPos.x * cellSize, -cellPos.y * cellSize);
        GameObject newRoomObj = Instantiate(newRoom, position, Quaternion.identity);

        //TODO remove this text section once debugging is finished
        TMP_Text cellText = newRoomObj.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();

        Node newNode = nodeGraph.AddNode(cellPos);

        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            // This is used to get the neighbouring node from which this new node is being created. (via the cell object intermediary)
            // The previous and current cell pos should only ever be the same for the root cell, in which case no connection is needed
            if (spawnedNodes[i].gridPos == previousCellPos && cellPos != previousCellPos)
            {
                newNode.AddConnection(spawnedNodes[i]);
                break;
            }
        }

        //TODO add whatever direction this new cell is in, into a list of occupied cardinal directions in the node, for room rotation needs.

        newNode.directionalRoomPrefab = newRoomObj;

        //TODO check if an edge weight is needed
        int EDGE_WEIGHT = 1;

        // Replaces the "1" used in previous function that was used to show the cell was occupied, with the int id of the Node/Cell
        mapArray[cellPos.x, cellPos.y] = newNode.id;

        cellText.SetText("ID: " + newNode.id);

        spawnedNodes.Add(newNode);

    }
}

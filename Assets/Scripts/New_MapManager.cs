using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


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
    private List<New_TempCell> spawnedCells;

    private Graph nodeGraph;

    public int initialCellX = 5;
    public int initialCellY = 6;

    void Start()
    {
        minNodes = 16;
        maxNodes = 24;

        cellSize = 0.5f;
        spawnedCells = new List<New_TempCell>();

        nodeGraph = new Graph(0, new Vector2Int(initialCellX, initialCellY));

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
        for (int i = 0; i < spawnedCells.Count; i++)
        {
            Destroy(spawnedCells[i].gameObject);
        }

        nodeGraph.ClearGraph();
        spawnedCells.Clear();

        mapArray = new int[gridSizeX, gridSizeY];
        mapArrayCount = default;
        cellQueue = new Queue<Vector2Int>();
        endRooms = new List<Vector2Int>();

        Vector2Int initialPosition = new Vector2Int(initialCellX, initialCellY);
        VisitCell(initialPosition, initialPosition);

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
                created |= VisitCell(new Vector2Int(cellPos.x - 1, cellPos.y), cellPos);
            }
            if (cellPos.x < 8)
            {
                created |= VisitCell(new Vector2Int(cellPos.x + 1, cellPos.y), cellPos);
            }
            if (cellPos.y > 2)
            {
                created |= VisitCell(new Vector2Int(cellPos.x, cellPos.y - 1), cellPos);
            }
            if (cellPos.y < 7)
            {
                created |= VisitCell(new Vector2Int(cellPos.x, cellPos.y + 1), cellPos);
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
    }

    int GetRandomEndRoom()
    {
        return -1;
    }

    private int GetNeighbourCount(int cellPosX, int cellPosY)
    {
        //return mapArray[index - 10] + mapArray[index - 1] + mapArray[index + 1] + mapArray[index + 10];
        int occupiedNeighbours = 0;
        if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedNeighbours++; }
        if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedNeighbours++; }
        if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedNeighbours++; }
        if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedNeighbours++; }
        return occupiedNeighbours;
    }

    private bool VisitCell(Vector2Int cellPos, Vector2Int previousCellPos)
    {
        if ((mapArray[cellPos.x, cellPos.y] != 0) || (GetNeighbourCount(cellPos.x, cellPos.y) > 1) || (mapArrayCount > maxNodes) || (Random.value < 0.4))
        {
            return false;
        }

        cellQueue.Enqueue(cellPos);
        mapArray[cellPos.x, cellPos.y] = 1;
        mapArrayCount++;

        SpawnRoom(cellPos, previousCellPos);

        return true;
    }

    private void SpawnRoom(Vector2Int cellPos, Vector2Int previousCellPos)
    {
        Vector2 position = new Vector2(cellPos.x * cellSize, -cellPos.y * cellSize);

        //TODO swap this to instantiating a Node? Then add connection to previous node
        //TODO maybe have node and cell seperate, to allow purely cell based features in additon to
        New_TempCell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        TMP_Text cellText = newCell.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        Node newNode = nodeGraph.AddNode(cellPos);

        for (int i = 0; i < spawnedCells.Count; i++)
        {
            // This is used to get the neighbouring node from which this new node is being created. (via the cell object intermediary)
            // The previous and current cell pos should only ever be the same for the root cell, in which case no connection is needed
            if (spawnedCells[i].mapPos == previousCellPos && cellPos != previousCellPos)
            {
                newNode.AddConnection(spawnedCells[i].node);
                break;
            }
        }

        // //TODO make this a const up top
        int EDGE_WEIGHT = 1;

        // Some of these values are set both in the cell and node to allow for location specific data based on grid position, vs purely node data that could use the cell data
        // This could lead to certain rooms being more humid due to water conditions
        newCell.id = newNode.id;
        newCell.node = newNode;
        newCell.value = EDGE_WEIGHT;
        newCell.mapPos = cellPos;
        // Replaces the "1" used in previous function that was used to show the cell was occupied, with the int id of the Node/Cell
        mapArray[cellPos.x, cellPos.y] = newNode.id;

        cellText.SetText("ID: " + newCell.id);

        spawnedCells.Add(newCell);

    }
}

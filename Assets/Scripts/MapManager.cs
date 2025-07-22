using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MapManager : MonoBehaviour
{
    //private int[,] mapArray;
    private int[] mapArray;

    private int mapArrayCount;
    private int minNodes;
    private int maxNodes;
    private List<int> endRooms;


    public TempCell cellPrefab;
    private float cellSize;
    private Queue<int> cellQueue;
    private List<TempCell> spawnedCells;

    //[SerializedField] private int initialCellX = 5;
    //[SerializedField] private int initialCellY = 6;

    // void Start()
    // {
    //     minNodes = 16;
    //     maxNodes = 24;

    //     cellSize = 0.5f;
    //     spawnedCells = new List<TempCell>();

    //     SetupMap();
    // }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         SetupMap();
    //     }
    // }

    void SetupMap()
    {
        for (int i = 0; i < spawnedCells.Count; i++)
        {
            Destroy(spawnedCells[i].gameObject);
        }

        spawnedCells.Clear();

        //mapArray = new int[,];
        mapArray = new int[100];
        mapArrayCount = default;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();

        //VisitCell(initialCellX, initialCellY);
        VisitCell(45);

        GenerateMap();

        
        // for (int i = 0; i < 100; i++)
        // {
        //     Debug.Log(mapArray[i]);
        // }
    }

    void GenerateMap()
    {
        while (cellQueue.Count > 0)
        {
            int index = cellQueue.Dequeue();
            int x = index % 10;

            bool created = false;

            if (x > 1) created |= VisitCell(index - 1);
            if (x < 9) created |= VisitCell(index + 1);
            if (index > 20) created |= VisitCell(index - 10);
            if (index < 70) created |= VisitCell(index + 10);

            if (created == false)
            {
                endRooms.Add(index);
            }

        }

        // Not enough rooms
        if (mapArrayCount < minNodes)
        {
            SetupMap();
            return;
        }
    }

    int GetRandomEndRoom()
    {
        return -1;
    }

    //private int GetNeighbourCount(int cellPosX, int cellPosY)
    private int GetNeighbourCount(int index)
    {
        return mapArray[index - 10] + mapArray[index - 1] + mapArray[index + 1] + mapArray[index + 10];
    }

    //private bool VisitCell(int cellPosX, int cellPosY)
    private bool VisitCell(int index)
    {
        // if ((mapArray[cellPosX, cellPosY] != 0) || (GetNeighbourCount(cellPosX, cellPosY) > 1) || (mapArrayCount > maxNodes) || (Random.value < 0.4))
        // {
        //     return false;
        // }

        // int index = cellPosX + (cellPosY * 10);
        // cellQueue.enqueue();

        if ((mapArray[index] != 0) || (GetNeighbourCount(index) > 1) || (mapArrayCount > maxNodes) || (UnityEngine.Random.value < 0.4))
        {
            return false;
        }

        cellQueue.Enqueue(index);
        mapArray[index] = 1;
        mapArrayCount++;

        SpawnRoom(index);

        return true;
    }

    private void SpawnRoom(int index)
    {
        int x = index % 10;
        int y = index / 10;

        Vector2 position = new Vector2(x * cellSize, -y * cellSize);

        TempCell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        newCell.value = 1;
        newCell.index = index;

        spawnedCells.Add(newCell);

    }
}

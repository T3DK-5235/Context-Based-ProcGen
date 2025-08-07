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
    private List<Node> endRooms;

    public int gridSizeX;
    public int gridSizeY;


    public New_TempCell cellPrefab;
    private float cellSize;
    private Queue<Node> nodeQueue;
    private List<Node> spawnedNodes;
    List<GameObject> spawnedRooms;

    private Graph nodeGraph;

    public int initialCellX = 5;
    public int initialCellY = 6;

    // Stores a list of nodes hat contain features that can spread to nearby rooms so they can be revisited after complete map creation
    private List<Node> featureOriginNodes;

    [SerializeField] SO_MapGenerationValues mapGenValues;
    [SerializeField] SO_RoomTypeContainer allRoomTypes;

    //Currently stores 2d image placeholder for testing
    [SerializeField] GameObject newRoom;
    // A tempt object for visually showing info when debugging
    [SerializeField] GameObject roomInfoPrefab;

    void Start()
    {
        minNodes = 16;
        maxNodes = 24;

        cellSize = 0.5f;
        spawnedNodes = new List<Node>();
        spawnedRooms = new List<GameObject>();

        featureOriginNodes = new List<Node>();

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
            for (int j = 0; j < spawnedRooms[i].transform.childCount; j++)
            {
                Destroy(spawnedRooms[i].transform.GetChild(j).gameObject);
            }
            Destroy(spawnedRooms[i]);
        }

        nodeGraph.ClearGraph();
        spawnedNodes.Clear();
        spawnedRooms.Clear();

        mapArray = new int[gridSizeX, gridSizeY];
        mapArrayCount = default;
        nodeQueue = new Queue<Node>();
        endRooms = new List<Node>();

        Vector2Int initialPosition = new Vector2Int(initialCellX, initialCellY);

        VisitCell(initialPosition, null, E_CardinalDirections.NONE);

        GenerateMap();
    }

    void GenerateMap()
    {

        while (nodeQueue.Count > 0)
        {
            Node previousNode = nodeQueue.Dequeue();
            Vector2Int previousCellPos = previousNode.gridPos;

            bool created = false;
            // Cascades through the if statements and attempts to create new cells
            if (previousCellPos.x > 1)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x - 1, previousCellPos.y), previousNode, E_CardinalDirections.NORTH);
            }
            if (previousCellPos.x < 8)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x + 1, previousCellPos.y), previousNode, E_CardinalDirections.EAST);
            }
            if (previousCellPos.y > 2)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x, previousCellPos.y - 1), previousNode, E_CardinalDirections.WEST);
            }
            if (previousCellPos.y < 7)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x, previousCellPos.y + 1), previousNode, E_CardinalDirections.SOUTH);
            }

            // if a new cell is not created, add the existing index to the endrooms list
            if (created == false)
            {
                endRooms.Add(previousNode);
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

        Debug.Log("Finished Initial Physical Map Gen");
        Debug.Log("End Room Number: " + endRooms.Count);
        nodeGraph.CreateAdjacencyMatrix();

        // GetNames is significantly faster than GetValues I believe due to it checking for duplicated values?
        int numOfRoomTypes = Enum.GetNames(typeof(E_RoomTypes)).Length;

        //TODO figure out if to turn "4" into a const up top to avoid magic numbers
        //TODO move this to a seperate function?
        // N/E/S/W
        int[] occupiedCardinalDirections = new int[4];
        // Loops through all nodes regardless of their relation to each other
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            //=================================================================================
            //                         Handles initial room prefab
            //=================================================================================
            Array.Clear(occupiedCardinalDirections, 0, occupiedCardinalDirections.Length);

            int cellPosX = spawnedNodes[i].gridPos.x;
            int cellPosY = spawnedNodes[i].gridPos.y;

            // Check which directions from the node are occupied by another node
            if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] = 1; }
            if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] = 1; }
            if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.EAST] = 1; }
            if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.WEST] = 1; }

            spawnedNodes[i].AddCardinalNeighbourSet(occupiedCardinalDirections);

            // Get a random room type from the initial possible room types list
            List<SO_RoomType> possibleRoomTypeList = allRoomTypes.initialRoomTypes;
            SO_RoomType chosenRoomType = possibleRoomTypeList[UnityEngine.Random.Range(0, possibleRoomTypeList.Count)];
            spawnedNodes[i].roomType = chosenRoomType;

            // If the room has any feature prefabs that can be shared add it to a list that can be looped through later to spread those prefabs
            if (chosenRoomType.featurePrefabs.Count > 0) { featureOriginNodes.Add(spawnedNodes[i]); }

            // Prompts node to store rotation and basic room outer structure (a prefab with the correct number of doors, currently only one of each type exists)
            spawnedNodes[i].SetupBasicRoom(GetNeighbourCount(spawnedNodes[i].gridPos.x, spawnedNodes[i].gridPos.y));
        }

        Debug.Log("Number of origin nodes: " + featureOriginNodes.Count);

        //TODO do graph rewriting here, moving prefab spawning til later
        //TODO feature spreading by looping through featureOriginNodes

        // Loops through all the stored nodes that have possible features to be spread to nearby rooms
        for (int i = 0; i < featureOriginNodes.Count; i++)
        {
            List<SO_RoomFeature> featureList = featureOriginNodes[i].roomType.featurePrefabs;
            for (int j = 0; j < featureList.Count; j++)
            {
                SpreadFeature(featureOriginNodes[i], featureList[j]);
            }
        }


        


        //===============================================================================================================
        //                         Handles the creation of the prefabs for each room
        //===============================================================================================================
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            int cellPosX = spawnedNodes[i].gridPos.x;
            int cellPosY = spawnedNodes[i].gridPos.y;

            (int, GameObject) basicRoomData = spawnedNodes[i].basicRoomData;
            Vector3 roomPhysicalPosition = new Vector3(cellPosX * (cellSize * 30), 0, -cellPosY * (cellSize * 30));
            Debug.Log("Node ID: " + spawnedNodes[i].id + " --- Rotation amount = " + basicRoomData.Item1);
            Quaternion roomRotation = Quaternion.Euler(0, basicRoomData.Item1, 0);

            //TODO move this Instantiation til a later point so all room features are instantiated at the same time?
            //TODO if I do this, store the instantiation data in the node, as otherwise the physical position and rotation will be lost after this current loop
            GameObject newRoom = Instantiate(basicRoomData.Item2, roomPhysicalPosition, roomRotation);

            GameObject initialRoomContentPrefab = spawnedNodes[i].roomType.baseRoomPrefab;
            GameObject newRoomTypeContent = Instantiate(initialRoomContentPrefab, roomPhysicalPosition, roomRotation);
            newRoomTypeContent.gameObject.transform.SetParent(newRoom.transform);

            // Debugging text
            GameObject newRoomInfo = Instantiate(roomInfoPrefab, roomPhysicalPosition, roomRotation);
            TMP_Text roomInfo = newRoomInfo.gameObject.transform.GetComponent<TMP_Text>();
            roomInfo.SetText("ID: " + spawnedNodes[i].id + " --- RoomType: " + spawnedNodes[i].roomType);
            newRoomInfo.gameObject.transform.SetParent(newRoom.transform);
            newRoomInfo.transform.Translate(0.0f, -1.0f, 0.0f, Space.Self);
            newRoomInfo.transform.Rotate(-90.0f, 0.0f, 180.0f, Space.Self);

            spawnedRooms.Add(newRoom);
        }
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

    private bool VisitCell(Vector2Int newCellPos, Node previousNode, E_CardinalDirections traversalDirection)
    {
        if ((mapArray[newCellPos.x, newCellPos.y] != 0) || (GetNeighbourCount(newCellPos.x, newCellPos.y) > 1) || (mapArrayCount > maxNodes) || (UnityEngine.Random.value < 0.4))
        {
            return false;
        }
        
        Node newNode = nodeGraph.AddNode(newCellPos);

        nodeQueue.Enqueue(newNode);
        mapArray[newCellPos.x, newCellPos.y] = 1;
        mapArrayCount++;

        SpawnRoom(newNode, previousNode, traversalDirection);

        return true;
    }

    private void SpawnRoom(Node newNode, Node previousNode, E_CardinalDirections traversalDirection)
    {
        Vector2 position = new Vector2(newNode.gridPos.x * cellSize, -newNode.gridPos.y * cellSize);
        GameObject newRoomObj = Instantiate(newRoom, position, Quaternion.identity);

        //TODO remove this text section once debugging is finished
        TMP_Text cellText = newRoomObj.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        cellText.SetText("ID: " + newNode.id);


        // for (int i = 0; i < spawnedNodes.Count; i++)
        // {
        //     // This is used to get the neighbouring node from which this new node is being created. (via the cell object intermediary)
        //     // The previous and current cell pos should only ever be the same for the root cell, in which case no connection is needed
        //     if (spawnedNodes[i].gridPos == previousCellPos && cellPos != previousCellPos)
        //     {
        //         newNode.AddConnection(spawnedNodes[i]);
        //         //nodeGraph.AddNodeConnection()
        //         break;
        //     }
        // }

        // If statement is used to filter out the root node, as it has no previous node to connect to
        if (previousNode != null) { nodeGraph.AddNodeConnection(newNode, previousNode); }

        //TODO rename this, as it's just a simplified map view
        newNode.directionalRoomPrefab = newRoomObj;

        // Replaces the "1" used in previous function that was used to show the cell was occupied, with the int id of the Node/Cell
        mapArray[newNode.gridPos.x, newNode.gridPos.y] = newNode.id;

        spawnedNodes.Add(newNode);

    }

    private void SpreadFeature(Node rootNode, SO_RoomFeature roomFeature)
    {
        List<int> visitedIDs = new List<int>();
        Queue<int> nextToVisit = new Queue<int>();

        //nextToVisit.Enqueue()
    }
}

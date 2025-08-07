using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using Unity.VisualScripting;


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
    //private List<Node> spawnedNodes;
    List<GameObject> spawnedRooms;
    List<GameObject> spawnedMapCells;

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
        //spawnedNodes = new List<Node>();
        spawnedRooms = new List<GameObject>();
        spawnedMapCells = new List<GameObject>();

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
        for (int i = 0; i < spawnedMapCells.Count; i++)
        {
            Destroy(spawnedMapCells[i]);
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
        //Debug.Log("End Room Number: " + endRooms.Count);
        nodeGraph.CreateAdjacencyMatrix();

        SetupBasicRoomInfo();


        //Debug.Log("Number of origin nodes: " + featureOriginNodes.Count);

        //TODO do graph rewriting here, moving prefab spawning til later


        //TODO feature spreading by looping through featureOriginNodes
        //TODO move this to its own function
        // Loops through all the stored nodes that have possible features to be spread to nearby rooms
        Debug.Log("Origin Nodes Count: " + featureOriginNodes.Count);
        for (int i = 0; i < featureOriginNodes.Count; i++)
        {
            List<SO_RoomFeature> featureList = featureOriginNodes[i].roomType.featurePrefabs;
            for (int j = 0; j < featureList.Count; j++)
            {
                SpreadFeature(featureOriginNodes[i], featureList[j]);
            }
        }

        BuildRoom();
    }

    private void SetupBasicRoomInfo()
    {
        List<Node> spawnedNodes = nodeGraph.totalNodeList;

        //TODO figure out if to turn "4" into a const up top to avoid magic numbers
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
    }

    private void BuildRoom()
    {
        List<Node> spawnedNodes = nodeGraph.totalNodeList;

        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Node nodeToBuild = spawnedNodes[i];
            int cellPosX = spawnedNodes[i].gridPos.x;
            int cellPosY = spawnedNodes[i].gridPos.y;

            (int, GameObject) basicRoomData = spawnedNodes[i].basicRoomData;
            Vector3 roomPhysicalPosition = new Vector3(cellPosX * (cellSize * 30), 0, -cellPosY * (cellSize * 30));
            //Debug.Log("Node ID: " + spawnedNodes[i].id + " --- Rotation amount = " + basicRoomData.Item1);
            Quaternion roomRotation = Quaternion.Euler(0, basicRoomData.Item1, 0);

            //TODO move this Instantiation til a later point so all room features are instantiated at the same time?
            //TODO if I do this, store the instantiation data in the node, as otherwise the physical position and rotation will be lost after this current loop
            GameObject newRoom = Instantiate(basicRoomData.Item2, roomPhysicalPosition, roomRotation);

            GameObject newRoomTypeContent = Instantiate(spawnedNodes[i].roomType.baseRoomPrefab, roomPhysicalPosition, roomRotation);
            newRoomTypeContent.gameObject.transform.SetParent(newRoom.transform);

            // Loop through all of the extra prefabs to spawn
            for (int j = 0; j < spawnedNodes[i].relevantRoomPrefabs.Count; j++)
            {
                GameObject extraRoomPrefab = Instantiate(spawnedNodes[i].relevantRoomPrefabs[j], roomPhysicalPosition, roomRotation);
                extraRoomPrefab.gameObject.transform.SetParent(newRoom.transform);
            }

            // Debugging text
            GameObject newRoomInfo = Instantiate(roomInfoPrefab, roomPhysicalPosition, roomRotation);
            TMP_Text roomInfo = newRoomInfo.gameObject.transform.GetComponent<TMP_Text>();
            roomInfo.SetText("ID: " + spawnedNodes[i].id + " --- RoomType: " + spawnedNodes[i].roomType);
            newRoomInfo.gameObject.transform.SetParent(newRoom.transform);
            newRoomInfo.transform.Translate(0.0f, -1.0f, 0.0f, Space.Self);

            //newRoomInfo.transform.Rotate(-90, 0.0f, 180.0f, Space.Self);
            newRoomInfo.transform.rotation = Quaternion.LookRotation(new Vector3(0,1,0));

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

        InitRoom(newNode, previousNode, traversalDirection);

        return true;
    }

    private void InitRoom(Node newNode, Node previousNode, E_CardinalDirections traversalDirection)
    {
        Vector2 position = new Vector2(newNode.gridPos.x * cellSize, -newNode.gridPos.y * cellSize);
        GameObject newRoomObj = Instantiate(newRoom, position, Quaternion.identity);

        //TODO remove this text section once debugging is finished
        TMP_Text cellText = newRoomObj.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        cellText.SetText("ID: " + newNode.id);

        // If statement is used to filter out the root node, as it has no previous node to connect to
        if (previousNode != null) { nodeGraph.AddNodeConnection(newNode, previousNode); }

        newNode.basicMapPrefab = newRoomObj;
        spawnedMapCells.Add(newRoomObj);

        // Replaces the "1" used in previous function that was used to show the cell was occupied, with the int id of the Node/Cell
        mapArray[newNode.gridPos.x, newNode.gridPos.y] = newNode.id;
    }

    // Based on a breadth first search, so changes evenly radiate out from a room
    private void SpreadFeature(Node rootNode, SO_RoomFeature roomFeature)
    {
        int?[,] adjMatrix = nodeGraph.adjMatrix;
        List<Node> totalNodeList = nodeGraph.totalNodeList;

        List<Node> visitedNodes = new List<Node>();
        Queue<Node> nextToVisit = new Queue<Node>();

        visitedNodes.Clear();
        nextToVisit.Clear();

        nextToVisit.Enqueue(rootNode);

        // while (nextToVisit.Count > 0)
        // {
        //     Node visitingNode = nextToVisit.Dequeue();

        //     // If the node has not been visited or if the node already has the relevant feature prefab
        //     if (!visitedNodes.Contains(visitingNode) || visitingNode.relevantRoomPrefabs.Contains(roomFeature.featurePrefab))
        //     {
        //         //TODO have chance of spawning feature in that node
        //         // Generate a percentage value
        //         int prefabSpawnChance = UnityEngine.Random.Range(0, 100);
        //         if (roomFeature.linkChance <= prefabSpawnChance)
        //         {
        //             visitingNode.relevantRoomPrefabs.Add(roomFeature.featurePrefab);
        //         }

        //         // Add neighbours to the queue by checking that node's column in the adjacency matrix
        //         Debug.Log("adjMatrix depth: " + adjMatrix.GetLength(1));
        //         for (int i = 0; i < adjMatrix.GetLength(1) - 1; i++)
        //         {
        //             Debug.Log("visiting Node ID: " + visitingNode.id);
        //             // If there is a connection found, and the connection is not to itself
        //             if ((adjMatrix[visitingNode.id - 1, i] != null || adjMatrix[visitingNode.id - 1, i] != 0)
        //             && visitingNode.id != i)
        //             {
        //                 Debug.Log("Connected Node index" + totalNodeList[i]);
        //                 nextToVisit.Enqueue(totalNodeList[i]);
        //             }
        //         }
        //     }
        // }

        //for (int i = 0; i < 10; i++)
        while (nextToVisit.Count > 0)
        {

            Node visitingNode = nextToVisit.Dequeue();
            List<Connection> nodeNeighbours = visitingNode.connections;

            Debug.Log("Got here with node ID: " + visitingNode.id);

            if (visitedNodes.Contains(visitingNode) == false || visitingNode.relevantRoomPrefabs.Contains(roomFeature.featurePrefab))
            {
                visitedNodes.Add(visitingNode);
                //TODO have chance of spawning feature in that node
                // Generate a percentage value
                int prefabSpawnChance = UnityEngine.Random.Range(0, 100);
                //Debug.Log("prefab spawn chance: " + roomFeature.linkChance + " --- " + "rolled chance: " + prefabSpawnChance);
                //TODO improve bandage fix of preventing same prefab from spawning. Maybe return the visitedNodes and prefab type so it can be reused?
                if (roomFeature.linkChance >= prefabSpawnChance && !visitingNode.relevantRoomPrefabs.Contains(roomFeature.featurePrefab))
                {
                    visitingNode.relevantRoomPrefabs.Add(roomFeature.featurePrefab);

                    Debug.Log("Node ID: " + visitingNode.id + " --- Neighbour Count: " + nodeNeighbours.Count);
                    for (int j = 0; j < nodeNeighbours.Count; j++)
                    {
                        if (visitedNodes.Contains(nodeNeighbours[j].child) == false)
                        {
                            Debug.Log("Child Node ID: " + nodeNeighbours[j].child.id);
                            nextToVisit.Enqueue(nodeNeighbours[j].child);
                        }
                    }
                }

                // Add neighbours to the queue by checking that node's column in the adjacency matrix
                // Debug.Log("adjMatrix depth: " + adjMatrix.GetLength(1));
                // for (int j = 1; j < adjMatrix.GetLength(1); j++)
                // {
                //     Debug.Log("current Node ID: " + visitingNode.id + " and j is: " + j + " --- Value at [node_id, i] position is: " + adjMatrix[visitingNode.id, j]);
                //     // If there is a connection found, and the connection is not to itself
                //     if (adjMatrix[visitingNode.id, j] != null || adjMatrix[visitingNode.id, j] != 0)
                //     {
                //         Debug.Log("Next node to visit" + totalNodeList[j].id);
                //         if (visitingNode.id != j) { nextToVisit.Enqueue(totalNodeList[j]); }
                //     }
                // }
            }
        }

        string remainingNodes = "";
        for (int x = 0; x < nextToVisit.Count; x++)
        {
            Node nextUp = nextToVisit.Peek();
            remainingNodes += ", " + nextUp.id;
            nextToVisit.Dequeue();
        }
        Debug.Log("The remaining nodes are: " + remainingNodes);

    }
}

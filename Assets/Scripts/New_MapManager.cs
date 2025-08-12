using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using Unity.VisualScripting;
using System.ComponentModel.Design;


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
    [SerializeField] SO_GraphGrammarContainer allGrammars;

    //Currently stores 2d image placeholder for testing
    [SerializeField] GameObject newRoom;
    // A tempt object for visually showing info when debugging
    [SerializeField] GameObject roomInfoPrefab;

    private int generationAttempts;

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

        generationAttempts = 0;

        int idIncrementer = 0;
        idIncrementer = allRoomTypes.AllocateTagIDs(idIncrementer);

        allGrammars.CreateGrammarLookup();

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

        VisitCell(initialPosition, null);

        GenerateMap();
    }

    void GenerateMap()
    {
        generationAttempts++;
        while (nodeQueue.Count > 0)
        {
            Node previousNode = nodeQueue.Dequeue();
            Vector2Int previousCellPos = previousNode.gridPos;

            bool created = false;
            // Cascades through the if statements and attempts to create new cells
            if (previousCellPos.x > 1)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x - 1, previousCellPos.y), previousNode);
            }
            if (previousCellPos.x < 8)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x + 1, previousCellPos.y), previousNode);
            }
            if (previousCellPos.y > 2)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x, previousCellPos.y - 1), previousNode);
            }
            if (previousCellPos.y < 7)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x, previousCellPos.y + 1), previousNode);
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
            nodeQueue.Clear();
            return;
        }

        Debug.Log("Finished Initial Physical Map Gen");
        //Debug.Log("End Room Number: " + endRooms.Count);
        nodeGraph.CreateAdjacencyMatrix();



        List<Node> spawnedNodes = nodeGraph.totalNodeList;
        //TODO figure out if to turn "4" into a const up top to avoid magic numbers
        //? Think about integrating this section with "initCell" as that will remove a for loop
        // N/E/S/W
        int[] occupiedCardinalDirections = new int[4];
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Array.Clear(occupiedCardinalDirections, 0, occupiedCardinalDirections.Length);
            SetupRoom(occupiedCardinalDirections, spawnedNodes[i]);
        }

        CheckGrammars();

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

        Debug.Log("Total Generation Attempts: " + generationAttempts);
    }

    // This function is seperate from the initial
    private void SetupRoom(int[] occupiedCardinalDirections, Node nodeToSetup)
    {
        int cellPosX = nodeToSetup.gridPos.x;
        int cellPosY = nodeToSetup.gridPos.y;

        // Check which directions from the node are occupied by another node
        if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] = 1; }
        if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] = 1; }
        if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.EAST] = 1; }
        if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.WEST] = 1; }

        nodeToSetup.AddCardinalNeighbourSet(occupiedCardinalDirections);

        // Get a random room type from the initial possible room types list
        List<SO_RoomType> possibleRoomTypeList = allRoomTypes.initialRoomTypes;
        SO_RoomType chosenRoomType = possibleRoomTypeList[UnityEngine.Random.Range(0, possibleRoomTypeList.Count)];
        nodeToSetup.roomType = chosenRoomType;

        // If the room has any feature prefabs that can be shared add it to a list that can be looped through later to spread those prefabs
        if (chosenRoomType.featurePrefabs.Count > 0) { featureOriginNodes.Add(nodeToSetup); }

        // Prompts node to store rotation and basic room outer structure (a prefab with the correct number of doors, currently only one of each type exists)
        nodeToSetup.SetupBasicRoom(GetNeighbourCount(nodeToSetup.gridPos.x, nodeToSetup.gridPos.y));
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
            newRoomInfo.transform.rotation = Quaternion.LookRotation(new Vector3(0, 1, 0));

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

    private bool VisitCell(Vector2Int newCellPos, Node previousNode)
    {
        if ((mapArray[newCellPos.x, newCellPos.y] != 0) || (GetNeighbourCount(newCellPos.x, newCellPos.y) > 1) || (mapArrayCount > maxNodes) || (UnityEngine.Random.value < 0.4))
        {
            return false;
        }

        Node newNode = nodeGraph.AddNode(newCellPos);

        nodeQueue.Enqueue(newNode);
        mapArray[newCellPos.x, newCellPos.y] = 1;
        mapArrayCount++;

        InitRoom(newNode, previousNode);

        return true;
    }

    private void InitRoom(Node newNode, Node previousNode)
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
    // in the future this could be used to make expansion less likely the further the away from origin node
    private void SpreadFeature(Node rootNode, SO_RoomFeature roomFeature)
    {
        //int?[,] adjMatrix = nodeGraph.adjMatrix;
        //List<Node> totalNodeList = nodeGraph.totalNodeList;

        List<Node> visitedNodes = new List<Node>();
        Queue<Node> nextToVisit = new Queue<Node>();

        visitedNodes.Clear();
        nextToVisit.Clear();

        nextToVisit.Enqueue(rootNode);

        while (nextToVisit.Count > 0)
        {

            Node visitingNode = nextToVisit.Dequeue();
            List<Connection> nodeNeighbours = visitingNode.connections;

            //Debug.Log("Got here with node ID: " + visitingNode.id);

            // If the room hasn't been visited AND the room doesnt already have the prefab 
            //TODO improve bandage fix of preventing same prefab from spawning. Maybe return the visitedNodes and prefab type so it can be reused if needed?
            if (!visitedNodes.Contains(visitingNode) && !visitingNode.relevantRoomPrefabs.Contains(roomFeature.featurePrefab))
            {
                visitedNodes.Add(visitingNode);
                //TODO have chance of spawning feature in that node
                // Generate a percentage value
                int prefabSpawnChance = UnityEngine.Random.Range(0, 100);
                //Debug.Log("prefab spawn chance: " + roomFeature.linkChance + " --- " + "rolled chance: " + prefabSpawnChance);
                if (roomFeature.linkChance >= prefabSpawnChance)
                {
                    visitingNode.relevantRoomPrefabs.Add(roomFeature.featurePrefab);

                    //Debug.Log("Node ID: " + visitingNode.id + " --- Neighbour Count: " + nodeNeighbours.Count);
                    for (int j = 0; j < nodeNeighbours.Count; j++)
                    {
                        //TODO figure out if theres a more efficient way of doing this
                        //TODO this is caused by there being  both a connection from one node to another, and one in the opposite direction. This is currently used to figure out where doors need to be
                        if (!visitedNodes.Contains(nodeNeighbours[j].child))
                        {
                            //Debug.Log("Child Node ID: " + nodeNeighbours[j].child.id);
                            nextToVisit.Enqueue(nodeNeighbours[j].child);
                        }
                    }
                }
            }
        }

        // string remainingNodes = "";
        // for (int x = 0; x < nextToVisit.Count; x++)
        // {
        //     Node nextUp = nextToVisit.Peek();
        //     remainingNodes += ", " + nextUp.id;
        //     nextToVisit.Dequeue();
        // }
        // Debug.Log("The remaining nodes are: " + remainingNodes);

    }


    // Depth first search is used to look for grammar checking to look for long chains of rooms that head away from the rootnode
    // This could allow easy integration with lock and key puzzles, or for making it more likely to create altered/unique rooms further from the entrance
    private void CheckGrammars()
    {
        // Depth first search of graph to get all possible connections that exist in the graph
        // // Whilst doing this, add connection types to a dictionary of (SO_RoomType,SO_RoomType) key to (Node, Node) value

        List<Node> visitedNodes = new List<Node>();
        Stack<Node> nextToVisit = new Stack<Node>();

        visitedNodes.Clear();
        nextToVisit.Clear();

        nextToVisit.Push(nodeGraph.totalNodeList[0]);

        while (nextToVisit.Count > 0)
        {
            Node visitingNode = nextToVisit.Pop();
            //Debug.Log("Popped Visiting Node: " + visitingNode.id);

            //! using the connections may break, as connections back to it from the child are also stored
            List<Connection> nodeNeighbours = visitingNode.connections;

            if (visitedNodes.Contains(visitingNode) == false)
            {
                visitedNodes.Add(visitingNode);

                // Currently unused, but a chance for each grammar to apply could be added later
                // int grammarApplicationChance = UnityEngine.Random.Range(0, 100);

                List<int> nodePattern = new List<int>();
                nodePattern.Clear();
                nodePattern.Add(visitingNode.roomType.tagID);

                for (int i = 0; i < nodeNeighbours.Count; i++)
                {
                    //TODO either loop through all graph grammars here to check which one is linked.

                    // // The first element is the current node, so that shouldn't be removed, 
                    // // but the child of the node changes as we loop, so the rest of the list should be cleared
                    // // Remove range is used to allow multiple children to be stored which could be expanded upon in the future
                    // // Currently though, the setup of this DFS would only really allow grammars with two elements
                    // nodePattern.RemoveRange(1, nodePattern.Count - 1);
                    // // Add the id of the roomtype of the child to the nodePattern to compare to the grammars
                    // nodePattern.Add(nodeNeighbours[i].child.roomType.tagID);

                    // //! Sorting it screwed with the Remove range above! Maybe can create a new List that is sorted
                    // nodePattern.Sort();

                    // List<int> sortedNodePattern = new List<int>();
                    // sortedNodePattern.AddRange(nodePattern);
                    // sortedNodePattern.Sort();

                    nodePattern.Clear();
                    nodePattern.Add(visitingNode.roomType.tagID);
                    nodePattern.Add(nodeNeighbours[i].child.roomType.tagID);
                    nodePattern.Sort();

                    // List<Node> nodeReferenceList = new List<Node>() {visitingNode, nodeNeighbours[i].child};
                    // string nodePatternString = "NodePattern: ";
                    // for (int d = 0; d < nodePattern.Count; d++) { nodePatternString += nodePattern[d] + "."; }
                    // Debug.Log(nodePatternString);

                    //SO_GraphGrammar foundGrammar = null;

                    List<SO_GraphGrammar> potentialGrammars;
                    // Search for the smallest tagID value found from the nodes in the pattern
                    // For tagIDs 4,2,1,6, it will try find the tagID 1 in the dict, then check if the remaining values are correct
                    Debug.Log("Trying to get value: " + nodePattern[0]);
                    if (allGrammars.patternGrammarLink.TryGetValue(nodePattern[0], out potentialGrammars))
                    {
                        Debug.Log("Got the value from the dict --- potentialGrammars.Count: " + potentialGrammars.Count);
                        // // Starts at 1 as we know the first value is correct, as it's what we used as the value in the dict as a lookup
                        // for (int j = 1; j < relevantGrammar.relevantGrammarPattern.Count; j++)
                        // {
                        //     if (relevantGrammar.relevantGrammarPattern[j] == nodePattern[j])
                        //     {

                        //     }
                        // }
                        // Debug.Log("Found Related Grammar");
                        for (int j = 0; j < potentialGrammars.Count; j++)
                        {
                            //TODO remove debug log lines
                            Debug.Log("Node IDs: 1:" + visitingNode.id + " --- 2: " + nodeNeighbours[i].child.id);

                            Debug.Log("Grammar ID Counts: " + potentialGrammars[j].relevantGrammarPatternIDs.Count);
                            string grammarIdString = "Grammar ID String: ";
                            for (int d = 0; d < potentialGrammars[j].relevantGrammarPatternIDs.Count; d++)
                            {
                                grammarIdString += potentialGrammars[j].relevantGrammarPatternIDs[d] + ", ";
                            }
                            Debug.Log(grammarIdString);

                            string nodePatternStrings = "nodePattern String: ";
                            for (int d = 0; d < nodePattern.Count; d++) { nodePatternStrings += nodePattern[d] + ", "; }
                            Debug.Log(nodePatternStrings);

                            // if the pattern given by the nodes is equal to one of the patterns stored
                            if (nodePattern.SequenceEqual(potentialGrammars[j].relevantGrammarPatternIDs))
                            {
                                //applicableGrammar = potentialGrammars[j];
                                //TODO work on checking whether to replace existing node (and which node to replace), or adding a new node
                                //visitingNode.roomType = potentialGrammars[j].resultantRoomType;

                                Debug.Log("Visiting Node ID: " + visitingNode.id);
                                List<Node> nodeList = new List<Node>() { visitingNode, nodeNeighbours[i].child };
                                ApplyGrammar(nodeList, potentialGrammars[j]);
                                Debug.Log("Found the grammar!");
                                break;
                            }
                        }

                    }

                    // if (foundGrammar != null)
                    // {
                    //     visitingNode.roomType = foundGrammar.resultantRoomType;
                    // }



                    if (!visitedNodes.Contains(nodeNeighbours[i].child))
                    {
                        nextToVisit.Push(nodeNeighbours[i].child);
                    }
                }
            }
        }
    }

    private void ApplyGrammar(List<Node> nodeReferenceList, SO_GraphGrammar relevantGrammar)
    {
        if (relevantGrammar.replace == true)
        {
            for (int i = 0; i < nodeReferenceList.Count; i++)
            {
                // Debug.Log("Node initial Type: " + nodeReferenceList[i].roomType.tagID +
                // "--- Node expected Type: " + relevantGrammar.replaceType.tagID +
                // " --- Replacement Type: " + relevantGrammar.resultantRoomType.tagID +
                // "--- NodeReferenceList Size: " + nodeReferenceList.Count);

                if (nodeReferenceList[i].roomType.tagID == relevantGrammar.replaceType.tagID)
                {
                    nodeReferenceList[i].roomType = relevantGrammar.resultantRoomType;
                }
            }
        }
        else
        {
            for (int i = 0; i < nodeReferenceList.Count; i++)
            {

                //TODO check the neighbours of each node and see if another room can be placed, if so place it with the new prefab as it's main
                int cellPosX = nodeReferenceList[i].gridPos.x;
                int cellPosY = nodeReferenceList[i].gridPos.y;

                // Checks the physical neighbours of the node to see if another node can be placed
                if (GetNeighbourCount(nodeReferenceList[i].gridPos.x, nodeReferenceList[i].gridPos.y) < 4)
                {
                    //TODO improve the way directions for the new room are chosen, currently it just goes N/S/E/W
                    Vector2Int newCellPos = new Vector2Int();
                    if (mapArray[cellPosX - 1, cellPosY] == 0) { newCellPos.Set(cellPosX - 1, cellPosY); }
                    else if (mapArray[cellPosX + 1, cellPosY] == 0) { newCellPos.Set(cellPosX + 1, cellPosY); }
                    else if (mapArray[cellPosX, cellPosY + 1] == 0) { newCellPos.Set(cellPosX, cellPosY + 1); }
                    else if (mapArray[cellPosX, cellPosY - 1] == 0) { newCellPos.Set(cellPosX, cellPosY - 1); }
                    else { Debug.LogError(" If the code has made it here, it somehow has less than 4 neighbours but none in any cardinal direction... somehow "); }

                    //TODO figure out how the node that is added to is chosen, currently just picks the first node in the list
                    VisitCell(newCellPos, nodeReferenceList[0]);

                    //SetupRoom(new int[4], );
                }
            }
        }
    }
}

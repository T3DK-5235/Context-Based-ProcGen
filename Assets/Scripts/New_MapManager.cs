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

    private int gridSizeX;
    private int gridSizeY;


    public New_TempCell cellPrefab;
    private float cellSize;
    private float nodeSize;
    private Queue<Node> nodeQueue;
    //private List<Node> spawnedNodes;
    List<GameObject> spawnedRooms;
    List<GameObject> spawnedMapCells;

    private Graph nodeGraph;

    private int initialCellX;
    private int initialCellY;

    // Stores a list of nodes hat contain features that can spread to nearby rooms so they can be revisited after complete map creation
    private List<Node> featureOriginNodes;

    private const int ROOM_EXIT_DIRECTIONS = 4;

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
        minNodes = mapGenValues._minNodeCount;
        maxNodes = mapGenValues._maxNodeCount;

        cellSize = mapGenValues._cellSize;
        nodeSize = mapGenValues._nodeSize;

        gridSizeX = mapGenValues._gridSizeX;
        gridSizeY = mapGenValues._gridSizeY;

        initialCellX = mapGenValues._initialCellX;
        initialCellY = mapGenValues._initialCellY;

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

        VisitCell(initialPosition, null, E_CardinalDirections.NONE);

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
            
            //! east and west, and north and south *should* technically be swapped, as -1 actually goes up, back through the array
            if (previousCellPos.x > 1)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x - 1, previousCellPos.y), previousNode, E_CardinalDirections.WEST);
            }
            if (previousCellPos.x < 8)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x + 1, previousCellPos.y), previousNode, E_CardinalDirections.EAST);
            }
            if (previousCellPos.y > 2)
            {
                created |= VisitCell(new Vector2Int(previousCellPos.x, previousCellPos.y - 1), previousNode, E_CardinalDirections.NORTH);
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
        Debug.Log("mapArrayCount: " + mapArrayCount + "====================================================================================================================================================================================================================================================================================================================");
        if (mapArrayCount < minNodes)
        {
            SetupMap();
            nodeGraph.ClearGraph();
            nodeQueue.Clear();
            return;
        }

        //Debug.Log("Finished Initial Physical Map Gen");
        //Debug.Log("End Room Number: " + endRooms.Count);
        nodeGraph.CreateAdjacencyMatrix();

        List<Node> spawnedNodes = nodeGraph.totalNodeList;

        //? Think about integrating this section with "initCell" as that will remove a for loop
        //? Can't currently, as an adjacency matrix needs to be created before rooms can be setup
        // N/E/S/W
        int[] occupiedCardinalDirections = new int[ROOM_EXIT_DIRECTIONS];
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Array.Clear(occupiedCardinalDirections, 0, occupiedCardinalDirections.Length);
            SetupRoom(occupiedCardinalDirections, spawnedNodes[i]);
        }

        CheckGrammars();

        // Loops through all the stored nodes that have possible features to be spread to nearby rooms
        //Debug.Log("Origin Nodes Count: " + featureOriginNodes.Count);
        for (int i = 0; i < featureOriginNodes.Count; i++)
        {
            List<SO_RoomFeature> featureList = featureOriginNodes[i].roomType.featurePrefabs;
            for (int j = 0; j < featureList.Count; j++)
            {
                SpreadFeature(featureOriginNodes[i], featureList[j]);
            }
        }

        BuildRoom();

        //Debug.Log("Total Generation Attempts: " + generationAttempts);
    }

    private void BuildRoom()
    {
        List<Node> spawnedNodes = nodeGraph.totalNodeList;
        //Debug.Log("Now building rooms. There are " + spawnedNodes.Count + " Nodes to spawn");

        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Node nodeToBuild = spawnedNodes[i];
            int cellPosY = spawnedNodes[i].gridPos.x;
            int cellPosX = spawnedNodes[i].gridPos.y;

            (int, GameObject) basicRoomData = spawnedNodes[i].basicRoomData;
            Vector3 roomPhysicalPosition = new Vector3(cellPosX * nodeSize, 0, -cellPosY * nodeSize);
            //Debug.Log("Node ID: " + spawnedNodes[i].id + " --- Rotation amount = " + basicRoomData.Item1);
            Quaternion roomRotation = Quaternion.Euler(0, basicRoomData.Item1, 0);
            
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
            roomInfo.SetText("ID: " + spawnedNodes[i].id + " --- RoomType: " + spawnedNodes[i].roomType  + " --- GridPos: " + spawnedNodes[i].gridPos.ToString());
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

    private bool VisitCell(Vector2Int newCellPos, Node previousNode, E_CardinalDirections expansionDirection, bool additionalGrammarCells = false)
    {
        Debug.Log("Got here on load");
     
        // The next check within the if statement is only performed during normal generation and is ignored if the cell is provided by a grammar
        if (!additionalGrammarCells == true)
        {
            // Checks the cell isnt occupied, the cell isnt too near other cells (additional grammar cells ignore this rule), there aren't too many cells, and if a random value fails
            if ((mapArray[newCellPos.x, newCellPos.y] != 0) || (GetNeighbourCount(newCellPos.x, newCellPos.y) > 1) || (mapArrayCount > maxNodes) || (UnityEngine.Random.value < 0.4))
            {
                return false;
            }
        }

        Node newNode = nodeGraph.AddNode(newCellPos);

        if (!(previousNode == null))
        {
            Debug.Log("newCellPos: " + newCellPos.ToString() + " --- previousNodeID: " + previousNode.id + " --- newNodeID: " + newNode.id + " --- expansionDirection: " + (int)expansionDirection);
        }
        

        nodeQueue.Enqueue(newNode);
        mapArray[newCellPos.x, newCellPos.y] = 1;
        mapArrayCount++;

        InitRoom(newNode, previousNode, expansionDirection);

        return true;
    }

    private void InitRoom(Node newNode, Node previousNode, E_CardinalDirections expansionDirection)
    {
        Vector2 position = new Vector2(newNode.gridPos.x * cellSize, -newNode.gridPos.y * cellSize);
        GameObject newRoomObj = Instantiate(newRoom, position, Quaternion.identity);

        TMP_Text cellText = newRoomObj.gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        cellText.SetText("ID: " + newNode.id);

        // If statement is used to filter out the root node, as it has no previous node to connect to
        if (previousNode != null) { nodeGraph.AddNodeConnection(newNode, previousNode, expansionDirection); }

        newNode.basicMapPrefab = newRoomObj;
        spawnedMapCells.Add(newRoomObj);

        // Replaces the "1" used in previous function that was used to show the cell was occupied, with the int id of the Node/Cell
        mapArray[newNode.gridPos.x, newNode.gridPos.y] = newNode.id;
    }

    // Can optionally tell it to override a andom
    private void SetupRoom(int[] occupiedCardinalDirections, Node nodeToSetup, bool overrideRoomType = false, SO_RoomType chosenRoomType = null)
    {
        int cellPosX = nodeToSetup.gridPos.x;
        int cellPosY = nodeToSetup.gridPos.y;

        // Check which directions from the node are occupied by another node
        // Also check that those directions are within bounds, as out of bounds cannot be occupied anyway, and not catching this will cause an array bounds error
        // Each statement needs two ifs as checking the second statement in the first if could cause an out of bounds error anyway
        if (cellPosX - 1 > 0) { if (mapArray[cellPosX - 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.WEST] = 1; } }
        if (cellPosX + 1 < gridSizeX) { if (mapArray[cellPosX + 1, cellPosY] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.EAST] = 1; } }
        if (cellPosY - 1 < gridSizeY) { if (mapArray[cellPosX, cellPosY - 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.NORTH] = 1; } }
        if (cellPosY + 1 > 0) { if (mapArray[cellPosX, cellPosY + 1] != 0) { occupiedCardinalDirections[(int)E_CardinalDirections.SOUTH] = 1; } }

        nodeToSetup.AddCardinalNeighbourSet(occupiedCardinalDirections);

        // If a type is not provided get a random room type from the initial possible room types list
        if (overrideRoomType == false)
        {
            //TODO improve selection via weighted random?
            //TODO include a max amount of certain rooms in the calculation
            List<SO_RoomType> possibleRoomTypeList = allRoomTypes.initialRoomTypes;
            chosenRoomType = possibleRoomTypeList[UnityEngine.Random.Range(0, possibleRoomTypeList.Count)];
        }
        
        nodeToSetup.roomType = chosenRoomType;

        // If the room has any feature prefabs that can be shared add it to a list that can be looped through later to spread those prefabs
        if (chosenRoomType.featurePrefabs.Count > 0) { featureOriginNodes.Add(nodeToSetup); }

        // Prompts node to store rotation and basic room outer structure (a prefab with the correct number of doors, currently only one of each type exists)
        nodeToSetup.SetupBasicRoom();
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
                        //TODO this is caused by there being  both a connection from one node to another, and one in the opposite direction. The dual connection is being used to figure out where doors need to be
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

        List<Node> visitedNodes = new List<Node>();
        Stack<Node> nextToVisit = new Stack<Node>();

        visitedNodes.Clear();
        nextToVisit.Clear();

        nextToVisit.Push(nodeGraph.totalNodeList[0]);

        while (nextToVisit.Count > 0)
        {
            Node visitingNode = nextToVisit.Pop();

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

                    nodePattern.Clear();
                    nodePattern.Add(visitingNode.roomType.tagID);
                    nodePattern.Add(nodeNeighbours[i].child.roomType.tagID);
                    //nodePattern.Sort();

                    // List<Node> nodeReferenceList = new List<Node>() {visitingNode, nodeNeighbours[i].child};
                    // string nodePatternString = "NodePattern: ";
                    // for (int d = 0; d < nodePattern.Count; d++) { nodePatternString += nodePattern[d] + "."; }
                    // Debug.Log(nodePatternString);

                    List<SO_GraphGrammar> potentialGrammars;

                    if (allGrammars.patternGrammarLink.TryGetValue(nodePattern[0], out potentialGrammars))
                    {
                        Debug.Log("Got the value from the dict --- potentialGrammars.Count: " + potentialGrammars.Count);

                        for (int j = 0; j < potentialGrammars.Count; j++)
                        {
                            Debug.Log("Node IDs: 1:" + visitingNode.id + " --- 2: " + nodeNeighbours[i].child.id);

                            string grammarIdString = "Grammar roomID Counts: " + potentialGrammars[j].relevantGrammarPatternIDs.Count + " Grammar ID String: ";
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
                                Debug.Log("Found the grammar!" + " Visiting Node ID: " + visitingNode.id);

                                List<Node> nodeList = new List<Node>() { visitingNode, nodeNeighbours[i].child };
                                ApplyGrammar(nodeList, potentialGrammars[j]);

                                // The for loop continues, as the checked node could be involved in another grammar.
                            }
                        }

                    }

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
                if (nodeReferenceList[i].roomType.tagID == relevantGrammar.placeBesideType.tagID)
                {

                    //TODO check the neighbours of each node and see if another room can be placed, if so place it with the new prefab as it's main
                    int prevCellPosX = nodeReferenceList[i].gridPos.x;
                    int prevCellPosY = nodeReferenceList[i].gridPos.y;

                    // Checks the physical neighbours of the node to see if another node can be placed
                    int neighbourCount = GetNeighbourCount(nodeReferenceList[i].gridPos.x, nodeReferenceList[i].gridPos.y);
                    if (neighbourCount < 4)
                    {
                        Debug.Log(i + "NodeID: " + nodeReferenceList[i].id + " and it's neighbour count: " + neighbourCount);
                        //TODO improve the way directions for the new room are chosen, currently it just goes N/S/E/W
                        Vector2Int newCellPos = new Vector2Int();

                        //! double check that the chosen position is within array bounds
                        E_CardinalDirections expansionDirection = E_CardinalDirections.NONE;
                        if (mapArray[prevCellPosX - 1, prevCellPosY] == 0) //New node is West of parent
                        {
                            newCellPos.Set(prevCellPosX - 1, prevCellPosY);
                            expansionDirection = E_CardinalDirections.WEST;
                        }
                        else if (mapArray[prevCellPosX + 1, prevCellPosY] == 0) // New node is East of parent
                        {
                            newCellPos.Set(prevCellPosX + 1, prevCellPosY);
                            expansionDirection = E_CardinalDirections.EAST;
                        }
                        else if (mapArray[prevCellPosX, prevCellPosY - 1] == 0) // New node is North of parent
                        {
                            newCellPos.Set(prevCellPosX, prevCellPosY - 1);
                            expansionDirection = E_CardinalDirections.NORTH;
                        }
                        else if (mapArray[prevCellPosX, prevCellPosY + 1] == 0) // New node is South of parent
                        {
                            newCellPos.Set(prevCellPosX, prevCellPosY + 1);
                            expansionDirection = E_CardinalDirections.SOUTH;
                        }

                        else { Debug.LogError(" If the code has made it here, it somehow has less than 4 neighbours but none in any cardinal direction... somehow "); }
                        Debug.Log(i + " -------------------------------------");

                        Debug.Log(i + "Parent ID for new node: " + nodeReferenceList[i].id + " ------ And it's cell pos: " + newCellPos.ToString());

                        int initialRotation = nodeReferenceList[i].basicRoomData.Item1;
                        VisitCell(newCellPos, nodeReferenceList[i], expansionDirection, true);

                        //? Currently gets the most recent node (Which the new node will be as it's created just before this)
                        //? This will become more complicated if multiple nodes are created due to a grammar, so will have to check how to handle that.
                        Debug.Log(i + "Checking new node values: Grid Pos:" + nodeGraph.totalNodeList[nodeGraph.totalNodeList.Count - 1].gridPos.ToString() +
                        " --- NodeID: " + nodeGraph.totalNodeList[nodeGraph.totalNodeList.Count - 1].id);

                        string debugString = "Prior to force adding new connection, checking current amount: " + nodeReferenceList[i].connections.Count + " And the actual directions: ";
                        for (int d = 0; d < nodeReferenceList[i].connections.Count; d++)
                        {
                            debugString += nodeReferenceList[i].connections[d].child.id + "<-" + nodeReferenceList[i].connections[d].travelDirection.ToString() + " -----";
                        }
                        Debug.Log(debugString);


                        //? Due to unity being unable to update the occupied directions array during runtime for some reason, it is instead manually done here by directly
                        //? altering the node's array from outside the class which does work... for some reason.
                        E_CardinalDirections inverseDirection;
                        if ((int)expansionDirection % 2 == 0) { inverseDirection = (E_CardinalDirections)((int)expansionDirection + 1); }
                        else { inverseDirection = (E_CardinalDirections)((int)expansionDirection - 1); }
                        nodeReferenceList[i].SetCardinalNeighbourSet((int)inverseDirection);

                        // Connection testConnection = new Connection(nodeReferenceList[i], nodeGraph.totalNodeList[nodeGraph.totalNodeList.Count - 1], expansionDirection);
                        // nodeReferenceList[i].connections.Add(testConnection);
                        //Passes 
                        SetupRoom(new int[4], nodeGraph.totalNodeList[nodeGraph.totalNodeList.Count - 1], true, relevantGrammar.resultantRoomType);

                        // Re set up parent node, as the number of connections, thus the prefab required, will have changed.
                        nodeReferenceList[i].SetupBasicRoom();
                        // If the room has already spawned, no need to try again 
                        return;
                    }
                }
            }
        }
    }
}

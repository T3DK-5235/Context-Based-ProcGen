using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapGenerationValues", menuName = "Scriptable Objects/MapGenerationValues")]
public class SO_MapGenerationValues : ScriptableObject
{
    [Header("Grid Size")]
    [SerializeField] public int _gridSizeX = 10;
    [SerializeField] public int _gridSizeY = 10;

    [Header("Cell Count")]
    [SerializeField] public int _minNodeCount = 16;
    [SerializeField] public int _maxNodeCount = 24;

    [Header("Initial Cell Location")]
    [SerializeField] public int _initialCellX = 5;
    [SerializeField] public int _initialCellY = 6;

    [Header("Prefab spacing")]
    [SerializeField] public float _cellSize = 0.5f;
    [SerializeField] public float _nodeSize = 0.5f;

    [Header("Basic Room Types")]
    [SerializeField] public GameObject basicDeadEnd;
    [SerializeField] public GameObject basicCorner;
    [SerializeField] public GameObject basicHallway;
    [SerializeField] public GameObject basicTJunction;
    [SerializeField] public GameObject basicCrossJunction;

    [Header("Room Prefabs and Layout")]
    [SerializeField] List<SO_RoomType> totalRoomTypes;
}

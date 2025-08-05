using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapGenerationValues", menuName = "Scriptable Objects/MapGenerationValues")]
public class SO_MapGenerationValues : ScriptableObject
{
    [Header("Grid Size")]
    [SerializeField] public int gridSizeX;
    [SerializeField] public int gridSizeY;

    [Header("Cell Count")]
    [SerializeField] public int minCellCount;
    [SerializeField] public int maxCellCount;

    [Header("Initial Cell Location")]
    [SerializeField] public int initialCellX;
    [SerializeField] public int initialCellY;

    [Header("Basic Room Types")]
    [SerializeField] public GameObject basicDeadEnd;
    [SerializeField] public GameObject basicCorner;
    [SerializeField] public GameObject basicHallway;
    [SerializeField] public GameObject basicTJunction;
    [SerializeField] public GameObject basicCrossJunction;

    [Header("Room Prefabs and Layout")]
    [SerializeField] List<SO_RoomType> totalRoomTypes;
    // [SerializeField] public SO_RoomTypeContainer roomTypeStorage;
    // [SerializeField] public SO_RoomVisualTypes roomContextTypeStorage;
    // [SerializeField] public SO_RoomGameplayTypes roomVisualTypeStorage;
}

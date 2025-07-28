using UnityEngine;

[CreateAssetMenu(fileName = "MapGenerationValues", menuName = "Scriptable Objects/MapGenerationValues")]
public class SO_MapGenerationValues : ScriptableObject
{
    [Header("Grid Size")]
    [SerializeField] int gridSizeX;
    [SerializeField] int gridSizeY;

    [Header("Cell Count")]
    [SerializeField] int minCellCount;
    [SerializeField] int maxCellCount;

    [Header("Initial Cell Location")]
    [SerializeField] int initialCellX;
    [SerializeField] int initialCellY;

    [Header("Basic Room Types")]
    [SerializeField] GameObject basicDeadEnd;
    [SerializeField] GameObject basicCorner;
    [SerializeField] GameObject basicHallway;
    [SerializeField] GameObject basicTJunction;
    [SerializeField] GameObject basicCrossJunction;

    [Header("Room Prefabs and Layout")]
    [SerializeField] SO_RoomTypeContainer roomTypeStorage;
    [SerializeField] SO_RoomVisualTypes roomContextTypeStorage;
    [SerializeField] SO_RoomGameplayTypes roomVisualTypeStorage;
}

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

    [Header("Room Prefabs and Layout")]
    [SerializeField] SO_RoomTypes roomTypeStorage;
    [SerializeField] SO_RoomTypes roomContextTypeStorage;
    [SerializeField] SO_RoomTypes roomVisualTypeStorage;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomTypeContainer", menuName = "Scriptable Objects/RoomTypeContainer")]
public class SO_RoomTypeContainer : ScriptableObject
{
    // Used to add all the SO room types via the inspector 
    [SerializeField] List<SO_RoomType> totalRoomTypes;
    public Dictionary<E_RoomTypes, SO_RoomType> enumToObjectDict;

    // To avoid repeatedly searching through a list, I create a dictionary linking the Enum to the related Scriptable Object
    // This allows me to use the enum for comparison and creating the room, and can then get additional data from the SO when needed.
    public void LinkEnumToObject()
    {
        enumToObjectDict = new Dictionary<E_RoomTypes, SO_RoomType>();
        
        for (int i = 0; i < totalRoomTypes.Count; i++)
        {
            Debug.Log("Room Type: " + totalRoomTypes[i].roomType);
            enumToObjectDict.Add(totalRoomTypes[i].roomType, totalRoomTypes[i]);
        }
    }
}

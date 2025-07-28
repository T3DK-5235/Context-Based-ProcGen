using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomTypeContainer", menuName = "Scriptable Objects/RoomTypeContainer")]
public class SO_RoomTypeContainer : ScriptableObject
{
    [SerializeField] List<E_RoomTypes> totalRoomTypes; 
}

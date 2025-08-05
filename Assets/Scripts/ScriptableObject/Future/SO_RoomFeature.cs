using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomFeature", menuName = "Scriptable Objects/RoomFeature")]
public class SO_RoomFeature : ScriptableObject
{
    // The name/type of the room (could allow for multiple rooms of the same type in the future for variety)
    [SerializeField] E_RoomTypes roomType;
    // For normal rooms, this is the chance of the room appearing,
    // For altered rooms, this is used to determine if this altered room is picked over another
    // Priority for altered rooms is not currently considered, and a room will always become an altered room if it can
    [SerializeField] int roomPriority = 1;
    // The tags from this room that can be applied to nearby rooms. 
    // Notably these are not the total tags for the room, as others can be added based on context
    // If this is an altered room, these tags are the ones required to transition to the altered state
    [SerializeField] List<E_RoomTags> inherentRoomTags;

    [Header("Altered Versions")]
    // Room Types that this room can become based on tags
    [SerializeField] List<SO_RoomType> relatedAlteredRooms;

    [Header("Initial Spawning")]
    // Is it a room that can spawn on start, or is it created by a room's tags being altered
    [SerializeField] bool alteredRoom;

    [Header("Room Object")]
    [SerializeField] GameObject baseRoomPrefab;
}

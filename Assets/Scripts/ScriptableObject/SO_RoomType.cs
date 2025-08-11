using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomType", menuName = "Scriptable Objects/RoomType")]
public class SO_RoomType : ScriptableObject
{
    // The name/type of the room (could allow for multiple rooms of the same type in the future for variety)
    [SerializeField] public E_RoomTypes roomType;
    [SerializeField] public int tagID;
    // For normal rooms, this is the chance of the room appearing,
    [SerializeField] int roomPriority = 1;
    // The amount of these rooms that can spawn per map (0 is used if there is no limit)
    [SerializeField] int maxInstances = 1;

    [Header("Room Object")]
    [SerializeField] public GameObject baseRoomPrefab;

    [Header("Room Spawning")]
    // If the room can only spawn in a dead end room (not currently used)
    [SerializeField] bool isEndRoom = false;

    // If another room is near this one, give it one/multiple of these prefabs
    [Header("Context Provided Features")]
    [SerializeField] public List<SO_RoomFeature> featurePrefabs;

    //public void initTagID(int id) { tagID = id; }
}

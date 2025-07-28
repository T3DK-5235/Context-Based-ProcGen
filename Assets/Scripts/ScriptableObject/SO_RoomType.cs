using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomType", menuName = "Scriptable Objects/RoomType")]
public class SO_RoomType : ScriptableObject
{
    [SerializeField] E_RoomTypes roomType;
    [SerializeField] int roomRarity;
    // The tags from this room that can be applied to nearby rooms. 
    // Notably these are not the total tags for the room, as others can be added based on context
    [SerializeField] E_RoomGameplayTypes inherentVisualTags;
    [SerializeField] E_RoomVisualTypes inherentGameplayTags;

    [Header("Room Object")]
    [SerializeField] GameObject roomPrefab;
}

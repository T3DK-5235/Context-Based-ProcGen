using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomFeature", menuName = "Scriptable Objects/RoomFeature")]
public class SO_RoomFeature : ScriptableObject
{
    [Header("Feature Object")]
    [SerializeField] public GameObject featurePrefab;

    // The maximum distance away from the source room the feature will spread
    [SerializeField] public int linkMaxDistance = 3;
    // Percentage chance out of 100 to spread
    [SerializeField] public int linkChance = 50;
}

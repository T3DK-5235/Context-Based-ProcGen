using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomFeature", menuName = "Scriptable Objects/RoomFeature")]
public class SO_RoomFeature : ScriptableObject
{
    [Header("Feature Object")]
    [SerializeField] GameObject featurePrefab;

    // The maximum distance away from the source room the feature will spread
    [SerializeField] int linkMaxDistance = 3;
    // Percentage chance out of 100 to spread
    [SerializeField] int linkChance = 50;
}

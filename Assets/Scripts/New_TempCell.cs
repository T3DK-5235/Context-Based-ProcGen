using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEditor.MemoryProfiler;
using UnityEngine;


public class New_TempCell : MonoBehaviour
{
    //Currently just acts as an interface between the graph node and the grid based generation
    public int id;
    public Vector2Int mapPos;
    public int value;
    public Node node;
}

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GraphGrammar", menuName = "Scriptable Objects/GraphGrammar")]
public class SO_GraphGrammar : ScriptableObject
{
    [Header("Pattern to look for")]
    [SerializeField] public List<SO_RoomType> relevantGrammarPattern;
    [Header("For replacing an existing node")]
    // Whether the result from the grammar replaces 
    [SerializeField] bool replace;
    // Which of the nodes to replace
    [SerializeField] SO_RoomType replaceType;

    [Header("Grammar Result")]
    [SerializeField] GameObject resultantPrefab;
    

}

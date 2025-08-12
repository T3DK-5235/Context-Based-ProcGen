using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GraphGrammar", menuName = "Scriptable Objects/GraphGrammar")]
public class SO_GraphGrammar : ScriptableObject
{
    [Header("Pattern to look for")]
    [SerializeField] public List<SO_RoomType> relevantGrammarPattern;
    public List<int> relevantGrammarPatternIDs;

    //? Maybe turn all of the below into lists, to allow more complex room replacing/adding
    [Header("For replacing an existing node")]
    // Whether the result from the grammar replaces 
    [SerializeField] public bool replace;
    // Which of the nodes to replace
    [SerializeField] public SO_RoomType replaceType;

    [Header("Grammar Result")]
    [SerializeField] public SO_RoomType resultantRoomType;
    

}

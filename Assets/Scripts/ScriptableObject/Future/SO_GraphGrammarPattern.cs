using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GraphGrammarPattern", menuName = "Scriptable Objects/GraphGrammarPattern")]
public class SO_GraphGrammarPattern : ScriptableObject
{
    // This has it's own scriptable object, as it is not possible to serialize list
    [SerializeField] List<SO_RoomType> roomTypes;
}

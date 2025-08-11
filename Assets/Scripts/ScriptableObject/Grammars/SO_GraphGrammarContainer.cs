using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GraphGrammarContainer", menuName = "Scriptable Objects/GraphGrammarContainer")]
public class SO_GraphGrammarContainer : ScriptableObject
{
    [SerializeField] public List<SO_GraphGrammar> graphGrammarList;

    // A dict containing the ids of each tagid required as a value, and the resulting grammar
    public Dictionary<List<int>, SO_GraphGrammar> patternGrammarLink;

    public void CreateGrammarLookup()
    {
        List<int> tagIDList = new List<int>();
        for (int i = 0; i < graphGrammarList.Count; i++)
        {
            // Gets the ids for the relevant tags used in the comparison to improve comparison and allow easy sorting with ints rather than scriptable objects
            for (int j = 0; j < graphGrammarList[i].relevantGrammarPattern.Count; i++)
            {
                tagIDList.Add(graphGrammarList[i].relevantGrammarPattern[j].tagID);
            }

            //Sorts the tags to be in ascending order, this will be compared to sorted tags to prevent issues where x after y is detected by y after x is not.
            tagIDList.Sort();

            // Adds the int values of the tags in as the key, and the related graphGrammar as the value.
            patternGrammarLink.Add(tagIDList, graphGrammarList[i]);

            tagIDList.Clear();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GraphGrammarContainer", menuName = "Scriptable Objects/GraphGrammarContainer")]
public class SO_GraphGrammarContainer : ScriptableObject
{
    [SerializeField] public List<SO_GraphGrammar> graphGrammarList;

    // A dict containing the smallest tagid of a grammar as a value, and the resulting grammar to allow fast lookup avoiding long list checking
    public Dictionary<int, List<SO_GraphGrammar>> patternGrammarLink;

    public void CreateGrammarLookup()
    {
        //TODO maybe move this out of the scriptable object
        List<int> tagIDList = new List<int>();
        patternGrammarLink = new Dictionary<int, List<SO_GraphGrammar>>();

        // For each given graph grammar
        for (int i = 0; i < graphGrammarList.Count; i++)
        {
            // Gets the ids for the relevant tags used in the comparison to improve comparison and allow easy sorting with ints rather than scriptable objects
            for (int j = 0; j < graphGrammarList[i].relevantGrammarPattern.Count; j++)
            {
                tagIDList.Add(graphGrammarList[i].relevantGrammarPattern[j].tagID);
            }

            // Sorts the tags to be in ascending order, this will be used to get the smallest tagID value 
            // This is to prevent issues where x after y is detected but y after x is not.
            tagIDList.Sort();

            string tagIDListValues = "IDs Found: ";
            for (int d = 0; d < tagIDList.Count; d++) { tagIDListValues += tagIDList[d] + "."; }
            Debug.Log(tagIDListValues);

            // Remove existing grammar patterns. This *Should* only be an issue due to Scriptable Objects keeping data when in the unity editor?
            graphGrammarList[i].relevantGrammarPatternIDs.Clear();
            //graphGrammarList[i].relevantGrammarPatternIDs = tagIDList;
            graphGrammarList[i].relevantGrammarPatternIDs.AddRange(tagIDList);

            // If the key (smallest tagID) doesnt already exist, add it in along with a list
            if (!patternGrammarLink.ContainsKey(tagIDList[0]))
            {
                Debug.Log("Adding to dict with key: " + tagIDList[0]);
                patternGrammarLink.Add(tagIDList[0], new List<SO_GraphGrammar>());
            }
            patternGrammarLink[tagIDList[0]].Add(graphGrammarList[i]);

            tagIDList.Clear();
        }
    }
}

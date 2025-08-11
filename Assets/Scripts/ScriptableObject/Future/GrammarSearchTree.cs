using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

// //TODO look into using namespaces to seperate this from the main map
// //TODO either that or merge the two somehow to prevent some duplicated code
// public class GrammarNode
// {
//     int roomTypeID;
//     SO_GraphGrammar graphGrammar;
//     public List<GrammarConnection> connections { get; }
//     // Not all nodes will have a related grammar, so they are set to null
//     public GrammarNode(int roomTypeID, SO_GraphGrammar graphGrammar = null)
//     {
//         this.roomTypeID = roomTypeID;
//         this.graphGrammar = graphGrammar;
//     }
//     public GrammarNode AddConnection(GrammarNode childNode)
//     {
//         GrammarConnection newGrammarConnection = new GrammarConnection(this, childNode);
//         connections.Add(newGrammarConnection);

//         return this;
//     }
// }

// public class GrammarConnection
// {
//     public GrammarNode parent;
//     public GrammarNode child { get; }
//     public GrammarConnection(GrammarNode parent, GrammarNode child)
//     {
//         this.parent = parent;
//         this.child = child;
//     }
// }

// public class GrammarSearchTree
// {
//     public GrammarSearchTree(int roomTypeID)
//     {
//         GrammarNode rootNode = new GrammarNode(roomTypeID);
//     }

//     public void TraverseOrContruct(List<int> roomTypeIDList, SO_GraphGrammar graphGrammar = null)
//     {
//         for (int i = 0; i < roomTypeIDList.Count - 1; i++)
//         {

//         }
//     }

// }
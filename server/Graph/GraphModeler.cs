using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Napack.Server.Graph
{
    /// <summary>
    /// Models the Napacks as a series of trees.
    /// </summary>
    /// <remarks>
    /// Technically, I'm modeling this as a tree, but heuristically attempting to keep individual leaf nodes
    ///  next to the tree, splitting on leaf nodes that cause the greatest amount of tree division (if necessary).
    /// </remarks>
    public class GraphModeler
    {
        /// <summary>
        /// Maps a node name into its current graph blob and index in that blob
        /// </summary>
        private Dictionary<string, Tuple<int, int>> nameMap;
        
        /// <summary>
        /// The blobs of graphs that exist.
        /// </summary>
        private List<Node> graphs;

        /// <summary>
        /// If a node refers to blobs in other graphs, the other graph dependencies are listed here.
        /// </summary>
        private Dictionary<int, List<int>> graphDependencies;

        public GraphModeler()
        {
            this.nameMap = new Dictionary<string, Tuple<int, int>>();
            this.graphs = new List<Node>();
        }

        /// <summary>
        /// Adds a node into the currently-modeled graph.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(GraphNode node)
        {
            if (nameMap.ContainsKey(node.NodeName))
            {
                // There may be new dependencies.
            }
        }

        /// <summary>
        /// Retrieves the auto-computed layer count.
        /// </summary>
        /// <returns>The total number of layers. </returns>
        public int GetLayerCount()
        {
            return -1;
        }

        private class Node
        {
            public int Name { get; set; }
            public List<int> Dependencies { get; set; }
        }
    }
}

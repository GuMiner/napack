using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Napack.Server.Graph
{
    public class GraphNode
    {
        public string NodeName { get; set; }

        public string[] Dependencies { get; set; }
    }
}

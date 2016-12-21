using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// Graph of a parsed message
    /// </summary>
    public class FileGraph
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileId">ID of the file.</param>
        public FileGraph(uint fileId)
        {
            m_fileId = fileId;
        }

        /// <summary>
        /// Get/set the root node of the graph
        /// </summary>
        public GraphNode RootNode
        {
            get { return m_rootNode; }
            set { m_rootNode = value; }
        }

        /// <summary>
        /// Recurses through the nodes and updates each nodes end position.
        /// </summary>
        public void UpdateEndPositionOfNodes()
        {
            RootNode.SetEndPosition(0);
        }

        /// <summary>
        /// Root node of the graph;
        /// </summary>
        private GraphNode m_rootNode;

        /// <summary>
        /// The ID of the file that this graph applies to.
        /// </summary>
        private readonly uint m_fileId;
    }

    /// <summary>
    /// A single node which is used in a file graph.
    /// </summary>
    public class GraphNode
    {
        public GraphNode(string name, NodeType type, int filePositionStart)
        {
            m_name = name;
            m_nodeType = type;
            m_filePositionStart = filePositionStart;
            m_filePositionEnd = -1;
            m_subNodes = new List<GraphNode>();
            switch (type)
            {
                case NodeType.Complex:
                case NodeType.Empty:
                case NodeType.Vector:
                    m_nodeValue = null;
                    break;
                case NodeType.Primitive_single:
                    m_nodeValue = new Single();
                    break;
                case NodeType.Primitive_uint32:
                    m_nodeValue = new UInt32();
                    break;
                case NodeType.Primitive_uint16:
                    m_nodeValue = new UInt16();
                    break;
                default:
                    throw new ArgumentException("Unknown node type " + type);
            }
        }

        public GraphNode(string name, NodeType type, int filePositionStart, object nodeValue)
        {
            m_name = name;
            m_nodeType = type;
            m_subNodes = new List<GraphNode>();
            m_nodeValue = nodeValue;
            m_filePositionStart = filePositionStart;
            m_filePositionEnd = -1;
        }

        public int SetEndPosition(int offset)
        {
            int totalSize = 0;
            switch (NodeType)
            {
                case NodeType.Empty:
                    totalSize += (int) Value;
                    break;
                case NodeType.Complex:
                case NodeType.Vector:
                    int subNodeEndPos = offset;
                    foreach (GraphNode node in SubNodes)
                    {
                        subNodeEndPos = node.SetEndPosition(subNodeEndPos);
                    }

                    totalSize += subNodeEndPos - offset;
                    break;
                case NodeType.Primitive_single:
                    totalSize += sizeof(Single);
                    break;
                case NodeType.Primitive_uint16:
                    totalSize += sizeof(UInt16);
                    break;
                case NodeType.Primitive_uint32:
                    totalSize += sizeof(UInt32);
                    break;
                default:
                    throw new ArgumentException("Unknown node type " + NodeType);
            }

            m_filePositionEnd = totalSize + m_filePositionStart;
            return m_filePositionEnd;
        }

        /// <summary>
        /// Adds a subnode below this node
        /// </summary>
        /// <param name="subNode">The node that is to be added</param>
        /// <returns>Returns the node that was added.</returns>
        public GraphNode Add(GraphNode subNode)
        {
            if (subNode == null)
            {
                throw new ArgumentException();
            }
            m_subNodes.Add(subNode);
            return subNode;
        }

        /// <summary>
        /// Get/set which nodes belong to this node
        /// </summary>
        public List<GraphNode> SubNodes
        {
            get { return m_subNodes; }
        }

        /// <summary>
        /// Get/set the name of the node
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Get/set the type of the node
        /// </summary>
        public NodeType NodeType
        {
            get { return m_nodeType; }
            set { m_nodeType = value; }
        }

        /// <summary>
        /// Get/set the value of the node
        /// </summary>
        public object Value
        {
            get { return m_nodeValue; }
            set { m_nodeValue = value; }
        }

        /// <summary>
        /// Get/set the position at which the file starts
        /// </summary>
        public int FilePositionStart
        {
            get { return m_filePositionStart; }
            set { m_filePositionStart = value; }
        }

        /// <summary>
        /// Get/set at which the object ends. This value represents a position just after the file
        /// </summary>
        public int FilePositionEnd
        {
            get { return m_filePositionEnd; }
            set { m_filePositionEnd = value; }
        }

        /// <summary>
        /// Gets the length of the structure, which is known as soon as start & end of the structure
        /// have been determined
        /// </summary>
        public int Size
        {
            get
            {
                return m_filePositionEnd - m_filePositionStart;
            }
        }

        public GraphNode GetNamedField(string name)
        {
            // search at the current level
            return SubNodes.Where(p => p.Name == name).SingleOrDefault();

            // todo: search parent levels
        }

        private List<GraphNode> m_subNodes;
        private string m_name;
        private NodeType m_nodeType;
        private object m_nodeValue;
        private int m_filePositionStart;
        private int m_filePositionEnd;
    }
}

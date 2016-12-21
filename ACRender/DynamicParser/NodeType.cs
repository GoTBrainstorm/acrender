namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// available types of nodes
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// The node is a complex node, meaning it contains zero or more subnodes
        /// </summary>
        Complex,
        /// <summary>
        /// The node is a vector, meaning it holds a number of subobjects that are all of the same type.
        /// </summary>
        Vector,
        /// <summary>
        /// Empty node,used for alignements, for example
        /// </summary>
        Empty,
        /// <summary>
        /// This node represents an uint16
        /// </summary>
        Primitive_uint16,
        /// <summary>
        /// This node represents an uint32
        /// </summary>
        Primitive_uint32,
        /// <summary>
        /// This node represents a float
        /// </summary>
        Primitive_single
    }
}
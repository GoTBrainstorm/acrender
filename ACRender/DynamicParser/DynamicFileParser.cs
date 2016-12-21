using System;
using Gawa.ACRender.DynamicParser.DataStructures;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// Logic class for controlling the dynamic file parser
    /// </summary>
    public class DynamicFileParser
    {
        /// <summary>
        /// Type mapping class
        /// </summary>
        private readonly Mapping m_mapping;

        /// <summary>
        /// Constructor
        /// </summary>
        public DynamicFileParser()
        {
            m_mapping = new Mapping();
        }

        /// <summary>
        /// Get/set the type mapping control
        /// </summary>
        public Mapping Mapping
        {
            get { return m_mapping; }
        }

        /// <summary>
        /// Attempts to refresh the type mappings.
        /// </summary>
        /// <returns>Returns whether the refresh was successful (true)
        /// or not (false).</returns>
        public bool RefreshTypeMappings()
        {
            lock (m_mapping)
            {
                try
                {
                    m_mapping.ReadMapping();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }

        public FileGraph ApplyFilterToFile(EmbeddedFile file, string filterName,
                                           IParserOutput output)
        {
            if (file == null || filterName == null || output == null)
            {
                throw new ArgumentException("Invalid parameters specified.");
            }

            // lookup the filter with the specified name
            DataFileDef filter = m_mapping.GetFilterByName(filterName);
            if (filter == null)
            {
                output.HandleFatalError("The specified filter does not exist.");
            }

            FileGraph graph = new FileGraph(file.FileId);
            try
            {
                // open the input file
                file.PrepareFileForReading();

                // create a root node in the graph
                graph.RootNode = new GraphNode("Root node", NodeType.Complex, file.Position);
                foreach (object filterObject in filter.Items)
                {
                    DecodeEntry(graph.RootNode, filterObject, output, file);
                }
                graph.RootNode.FilePositionEnd = file.Position;

                graph.UpdateEndPositionOfNodes();
            }
            catch (Exception ex)
            {
                output.HandleFatalError("Unexpected exception: " + ex.Message);
            }
            finally
            {
                file.FileReadingComplete();
            }
            return graph;
        }

        private void DecodeEntry(GraphNode parentNode, object newSubObject, IParserOutput output, EmbeddedFile file)
        {
            // decode the node and add it to the parent:

            // first determine the exact type of node
            if (newSubObject is Field)
            {
                parentNode.Add(ParseField(newSubObject as Field, file));
            }
            else if (newSubObject is Align)
            {
                // align to the specified boundary
                parentNode.Add(ParseAlignment(newSubObject as Align, file));
            }
            else if (newSubObject is NamedType)
            {
                GraphNode complexNode = parentNode.Add(ParseNamedType(newSubObject as NamedType, file));

                // look up the specified type
                TypeDef definition = m_mapping.GetTypeDefByName((newSubObject as NamedType).TypeName);
                foreach (object subEbtry in definition.Items)
                {
                    DecodeEntry(complexNode, subEbtry, output, file);
                }
            }
            else if (newSubObject is Vector)
            {
                GraphNode vectorNode = parentNode.Add(ParseVector(newSubObject as Vector, file));

                // determine the length of the vector
                // this is done by examining the contents of the 'length' field
                // for now, we assume that all lengths are stored in a sister field of the vector, that
                // has appeared previously
                string indexFieldName = (newSubObject as Vector).Length;
                GraphNode amountNode = parentNode.GetNamedField(indexFieldName);
                if (amountNode == null)
                {
                    throw new ArgumentException(string.Format("Unable to find named field {0}", indexFieldName));
                }

                int length = Convert.ToInt32(amountNode.Value);

                // parse for the specified amount of times
                for (int i = 0; i < length; i++)
                {
                    DecodeEntry(vectorNode, (newSubObject as Vector).Item, output, file);
                }
            }
            else
            {
                throw new Exception("Unknown type of filter object");
            }
        }

        private GraphNode ParseField(Field field, EmbeddedFile file)
        {
            switch (field.Type)
            {
                case FieldType.uint32:
                    {
                        uint fieldValue = file.ReadUInt32();
                        return new GraphNode(field.Name, NodeType.Primitive_uint32, file.Position - sizeof (UInt32),
                                             fieldValue);
                    }
                case FieldType.uint16:
                    {
                        UInt16 fieldValue = file.ReadUInt16();
                        return new GraphNode(field.Name, NodeType.Primitive_uint16, file.Position - sizeof (UInt16),
                                             fieldValue);
                    }
                case FieldType.single:
                    {
                        Single fieldValueSingle = file.ReadSingle();
                        return new GraphNode(field.Name, NodeType.Primitive_single, file.Position - sizeof (Single),
                                             fieldValueSingle);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private GraphNode ParseAlignment(Align align, EmbeddedFile file)
        {
            if (align.boundary == 4)
            {
                file.AlignToDwordBoundary();
                return new GraphNode("DWORD align", NodeType.Empty, file.Position - align.boundary, align.boundary);
            }

            throw new NotImplementedException();
        }

        private GraphNode ParseNamedType(NamedType type, EmbeddedFile file)
        {
            return new GraphNode(type.DisplayName, NodeType.Complex, file.Position);
        }

        private GraphNode ParseVector(Vector vector, EmbeddedFile file)
        {
            return new GraphNode(vector.Name, NodeType.Vector, file.Position);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Gawa.ACRender.DynamicParser.DataStructures;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// Class that takes care of the type mappings
    /// </summary>
    public class Mapping
    {
        public Mapping()
        {
            m_typeDefs = new Dictionary<string, TypeDef>();
            m_dataFileDefs = new Dictionary<string, DataFileDef>();
        }

        /// <summary>
        /// Reads the active mapping into memory
        /// </summary>
        public void ReadMapping()
        {
            // open the XML file
            //string xmlFileLocation = @"G:\gawa\code\ACRender\ACRender\DynamicParser\DataStructures.xml";
            string xmlFileLocation = @"E:\gawa\code\ACRender_ToD\ACRender\DynamicParser\DataStructures.xml";

            ObjectFromXMLDeserializer<Structures> deser = new ObjectFromXMLDeserializer<Structures>();
            XmlDeserializationResult<Structures> deserResult = deser.Deserialize(xmlFileLocation, XSDType.DataStructures);
            if (!deserResult.Success)
            {
                throw new Exception("Failed to load the data structure definition");
            }

            // clear the lookup tables
            m_typeDefs.Clear();
            
            // we've read the structure into memory.. put them into the lookup tables
            Structures structures = deserResult.Value;
            foreach (TypeDef typeDef in structures.TypeDef)
            {
                m_typeDefs.Add(typeDef.Name, typeDef);
            }
            foreach (DataFileDef dataFileDef in structures.DataFileDef)
            {
                m_dataFileDefs.Add(dataFileDef.Name, dataFileDef);
            }
        }

        /// <summary>
        /// Gets a list of filter names
        /// </summary>
        /// <returns>List of filter names</returns>
        public string[] GetFilterList()
        {
            string[] names = new string[m_dataFileDefs.Count];
            int i = 0;
            foreach (string name in m_dataFileDefs.Keys)
            {
                names[i] = name;
                i++;
            }

            Array.Sort(names);
            return names;
        }

        /// <summary>
        /// Looks up a filter by its name.
        /// </summary>
        /// <param name="filterName">The name of the filter that we're looking for.</param>
        /// <returns>Returns the filter, or null of the specified filter name did not exist.</returns>
        public DataFileDef GetFilterByName(string filterName)
        {
            DataFileDef returnValue = null;
            m_dataFileDefs.TryGetValue(filterName, out returnValue);
            return returnValue;
        }

        /// <summary>
        /// Looks up a type definition by its name.
        /// </summary>
        /// <param name="defName">The name of the typedef that we're looking for.</param>
        /// <returns>Returns the typedef, or null of the specified typedef name did not exist.</returns>
        public TypeDef GetTypeDefByName(string defName)
        {
            TypeDef returnValue = null;
            m_typeDefs.TryGetValue(defName, out returnValue);
            return returnValue;
        }

        /// <summary>
        /// Lookup list for type definitions
        /// </summary>
        private readonly Dictionary<string, TypeDef> m_typeDefs;
        /// <summary>
        /// Lookup list for data file definitions
        /// </summary>
        private readonly Dictionary<string, DataFileDef> m_dataFileDefs;
    }
}

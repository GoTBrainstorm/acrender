using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// The types of XSD files that are used in the gateway & plugin host
    /// </summary>
    internal enum XSDType
    {
        Undefined = 0,
        DataStructures,
        CachedFileList
    }

    /// <summary>
    /// Provides functions to load the contents of an XSD file from a DLL
    /// </summary>
    internal sealed class XSDLoader
    {
        /// <summary>
        /// Static constructor, to initialize some static members
        /// </summary>
        static XSDLoader()
        {
            m_XSDFiles = new Dictionary<XSDType, string>();
        }

        /// <summary>
        /// Finds the resource name for a specific XSD
        /// </summary>
        /// <param name="type">The type of XSD that we'r elooking for</param>
        /// <returns>A string containing the filename</returns>
        private static string FindFileNameByXSDType(XSDType type)
        {
            switch (type)
            {
                case XSDType.DataStructures:
                    return BASE_LOCATION + "DataStructures.xsd";
                case XSDType.CachedFileList:
                    return BASE_LOCATION + "CachedFileList.xsd";
                default:
                    throw new ApplicationException("Could not find requested XSD");
            }
        }

        /// <summary>
        /// Returns a stream that contains a specific XSD
        /// </summary>
        /// <param name="type">The type of the XSD that we're looking for</param>
        /// <returns>A stream which contains the XSD, or null if we failed to
        /// create it</returns>
        public static Stream GetXSDAsStream(XSDType type)
        {
            if (type == XSDType.Undefined)
                throw new Exception("Invalid XSD type");

            string fileName = FindFileNameByXSDType(type);

            Stream stream = null;
            try
            {
                stream = typeof(XSDLoader).Assembly.GetManifestResourceStream(fileName);
                return stream;
            }
            catch (Exception)
            {
                if (stream != null)
                    stream.Close();
                return null;
            }
        }

        /// <summary>
        /// Returns the contents of an XSD file as a string
        /// </summary>
        /// <param name="type">The type of XSD to load</param>
        /// <returns>The contents of the XSD, or null if the contents could not be loaded</returns>
        public static string GetXSDContents(XSDType type)
        {
            if (type == XSDType.Undefined)
            {
                throw new Exception("Invalid XSD type");
            }

            if (m_XSDFiles[type] != null)
            {
                return m_XSDFiles[type];
            }

            string fileName = FindFileNameByXSDType(type);

            Stream stream = null;
            StreamReader reader = null;
            try
            {
                stream = typeof(XSDLoader).Assembly.GetManifestResourceStream(fileName);
                reader = new StreamReader(stream);
                m_XSDFiles[type] = reader.ReadToEnd();
                return m_XSDFiles[type];
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();
            }
        }

        private const string BASE_LOCATION = "Gawa.ACRender.DynamicParser.";

        /// <summary>
        /// The cached XSD files
        /// </summary>
        private static Dictionary<XSDType, string> m_XSDFiles;
    }
}

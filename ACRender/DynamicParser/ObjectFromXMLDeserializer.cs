using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// This class provides templated means of deserializing an object from an
    /// XML document. Validation can occur using an XSD scheme.
    /// </summary>
    internal sealed class ObjectFromXMLDeserializer<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectFromXMLDeserializer()
        {
        }

        public XmlDeserializationResult<T> Deserialize(string xmlFile, XSDType xsdType)
        {
            FileStream fs = null;
            Stream xsdStream = null;
            try 
	        {
                xsdStream = XSDLoader.GetXSDAsStream(xsdType);
                fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read);
                return Deserialize(fs, xsdStream);
	        }
	        finally
            {
                if (xsdStream != null)
                {
                    xsdStream.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Deserializes an object from an XML string using the specified XSD stream
        /// </summary>
        /// <param name="xmlString">The string that contains the object</param>
        /// <param name="xsdStream">The stream that contains the XSD for the XML</param>
        /// <param name="dontLogErrors">When set, there will be no errors written to the logfiles</param>
        /// <returns>Returns a deserialized object, or null if anything goed wrong</returns>
        public XmlDeserializationResult<T> Deserialize(string xmlString, Stream xsdStream)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                byte[] b = Encoding.UTF8.GetBytes(xmlString);
                ms.Write(b, 0, b.Length);
                b = null;
                ms.Seek(0, SeekOrigin.Begin);
                return Deserialize(ms, xsdStream);
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize an object from the specified xml stream, using
        /// the xsd stream as a means of validating the contents
        /// </summary>
        /// <param name="xmlStream">The stream containing the XML that we wish to deserialize</param>
        /// <param name="xsdStream">The stream containing the XSD file for this XML</param>
        /// <param name="dontLogErrors">When set, there will be no errors written to the logfiles</param>
        /// <returns>Returns the result of the deserialization operation</returns>
        public XmlDeserializationResult<T> Deserialize(Stream xmlStream, Stream xsdStream)
        {
            m_result = new XmlDeserializationResult<T>();
            XmlReader reader = null;
            try
            {
                // create a reader for the XSD
                XmlReader xsdReader = XmlReader.Create(xsdStream);

                // create a schema set with that XSD
                XmlSchemaSet xss = new XmlSchemaSet();
                xss.Add(null, xsdReader);

                // create serialization options
                XmlReaderSettings xrs = new XmlReaderSettings();
                xrs.ValidationType = ValidationType.Schema;
                xrs.Schemas = xss;
                xrs.ValidationEventHandler += new ValidationEventHandler(xrs_ValidationEventHandler);

                // create the reader
                reader = XmlReader.Create(xmlStream, xrs);

                // create the serializer and deserialize the object
                XmlSerializer xs = new XmlSerializer(typeof(T));
                m_result.Value = (T)xs.Deserialize(reader);
            }
            catch (Exception ex)
            {
                m_result.Errors.Add(ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return m_result;
        }

        /// <summary>
        /// Handles validation errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xrs_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            m_result.Errors.Add(e.Message);
        }

        /// <summary>
        /// Contains the result of the deserialization
        /// </summary>
        private XmlDeserializationResult<T> m_result;
    }
}

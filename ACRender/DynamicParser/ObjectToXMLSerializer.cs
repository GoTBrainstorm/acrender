using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// This class provides means to easily serialize an object to a
    /// XML document
    /// </summary>
    public class ObjectToXMLSerializer
    {
        /// <summary>
        /// Serialize an object to a string. The string will contain a
        /// XML representation of the object after serialization. If any
        /// errors occur, the return stirng will be null.
        /// </summary>
        /// <param name="obj">The object that must be serialized.</param>
        /// <returns>Returns an XML representation of the object.</returns>
        public static string SerializeObject(object obj)
        {
            if (obj == null)
                throw new ArgumentException("Object cannot be null");

            MemoryStream ms = null;
            XmlSerializer xs = null;
            try
            {
                ms = new MemoryStream();
                xs = new XmlSerializer(obj.GetType());

                xs.Serialize(ms, obj);

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// This class contains the result of a deserializationoperation performed
    /// by the ObjectFromXMLDeserializer class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XmlDeserializationResult<T>
    {
        /// <summary>
        /// Constructs an empty result object
        /// </summary>
        public XmlDeserializationResult()
        {
            m_value = default(T);
            m_errors = new List<string>();
        }

        /// <summary>
        /// The result of the deserialization operation
        /// </summary>
        private T m_value;

        /// <summary>
        /// Gets/sets the result of the deserialization operation
        /// </summary>
        public T Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        /// <summary>
        /// Get whether the deserialization was a success
        /// </summary>
        public bool Success
        {
            get { return m_errors.Count == 0; }
        }

        /// <summary>
        /// The list of errors that occured during deserialization
        /// </summary>
        private List<string> m_errors;

        /// <summary>
        /// Gets/sets the list of errors that occured during deserialization
        /// </summary>
        public List<string> Errors
        {
            get { return m_errors; }
            set { m_errors = value; }
        }
    }
}

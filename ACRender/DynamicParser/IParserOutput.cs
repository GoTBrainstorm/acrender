using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// Interface defining the methods that should be implemented when
    /// handling output from the file parser.
    /// </summary>
    public interface IParserOutput
    {
        /// <summary>
        /// Handles any fatal errors that might occur.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        void HandleFatalError(string message);

        void WriteDebug(string message, params object[] args);
    }
}

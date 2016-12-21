using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Gawa.ACRender
{
    /// <summary>
    /// Structure of a complex mesh. (0x02nnnnnn file). A complex mesh
    /// is a composition of one or more simple meshes.
    /// </summary>
    public class ComplexMesh
    {
        public ComplexMesh(EmbeddedFile sourceFile)
        {
            m_sourceFile = sourceFile;
        }

        /// <summary>
        /// Reads & parses the complex mesh
        /// </summary>
        public void ReadComplexMesh()
        {
            try
            {
                m_sourceFile.PrepareFileForReading();

                uint id = m_sourceFile.ReadUInt32();
                Debug.Assert(id == m_sourceFile.FileId);

                m_flags = m_sourceFile.ReadUInt32();
                if (m_flags == 0x04)
                {
                    //Debugger.Break();
                }
                m_simpleMeshCount = m_sourceFile.ReadUInt32();
                m_simpleMeshIDs = new UInt32[m_simpleMeshCount];
                for (int i=0; i < m_simpleMeshCount; i++)
                {
                    m_simpleMeshIDs[i] = m_sourceFile.ReadUInt32();
                }

                //m_indexCount = SourceFile.ReadUInt32();
                ////Debug.Assert(m_indexCount < m_simpleMeshCount);
                //m_indices = new uint[m_indexCount];
                //for (int i = 0; i < m_indexCount; i++)
                //{
                //    m_indices[i] = SourceFile.ReadUInt32();
                //    //Debug.Assert(m_indices[i] < m_simpleMeshCount);
                //}


                if (m_simpleMeshCount == 2)
                {
                    //Debugger.Break();
                }
                //Debugger.Break();
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
        }

        public uint Flags
        {
            get { return m_flags; }
        }

        public uint SimpleMeshCount
        {
            get { return m_simpleMeshCount; }
        }

        public uint[] SimpleMeshIDs
        {
            get { return m_simpleMeshIDs; }
        }

        /// <summary>
        /// The source file.
        /// </summary>
        private EmbeddedFile m_sourceFile;

        private uint m_flags;
        private uint m_simpleMeshCount;
        private uint[] m_simpleMeshIDs;

        //private uint m_indexCount;
        //private uint[] m_indices;
    }
}

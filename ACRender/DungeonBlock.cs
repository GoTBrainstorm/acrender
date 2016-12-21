using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Gawa.ACRender
{
    /// <summary>
    /// Represents a dungeon block, also known as a 0x0Dnnnnnn object.
    /// </summary>
    public class DungeonBlock
    {
        public DungeonBlock(EmbeddedFile sourceFile)
            : this(sourceFile, 0)
        {
        }

        public DungeonBlock(EmbeddedFile sourceFile, int startPosition)
        {
            m_sourceFile = sourceFile;
            m_myStartPos = startPosition;
        }

        /// <summary>
        /// Reads the dungeon block
        /// </summary>
        public uint ReadDungeonBlock()
        {
            uint lastFilePos = (uint)m_sourceFile.Position;
            try
            {
                m_sourceFile.PrepareFileForReading();

                uint fileId;
                if (m_myStartPos == 0)
                {
                    // file id, assert that it matches the file
                    fileId = m_sourceFile.ReadUInt32();
                    Debug.Assert(fileId == m_sourceFile.FileId);

                    // flags & texture IDs
                    m_flags = m_sourceFile.ReadUInt32();
                }
                else
                {
                    m_sourceFile.Seek((uint)m_myStartPos);
                    m_sourceFile.AlignToDwordBoundary();
                    m_flags = 0xFFFFFFFF;
                    fileId = 0xFFFFFFFF;
                }

                //Debug.Assert(m_flags == 0x01);   // check known flag values
                m_textureCount = m_sourceFile.ReadUInt32();     // index, perhaps?

                // unknown values
                m_t2TrifanCount = m_sourceFile.ReadUInt32();
                m_t1TrifanCount = m_sourceFile.ReadUInt32();
                m_unkBytesAfterTrifans = m_sourceFile.ReadUInt32();
                m_unk4 = m_sourceFile.ReadUInt32();

                // vertex data
                m_vertexCount = m_sourceFile.ReadUInt32();
                m_vertexInfo = new VertexInfo[m_vertexCount];
                for (int i = 0; i < m_vertexCount; i++)
                {
                    m_vertexInfo[i].VertexIndex = m_sourceFile.ReadUInt16();
                    Debug.Assert(i == m_vertexInfo[i].VertexIndex);
                    m_vertexInfo[i].CUVDataLength = m_sourceFile.ReadUInt16();
                    m_vertexInfo[i].X = m_sourceFile.ReadSingle();
                    m_vertexInfo[i].Y = m_sourceFile.ReadSingle();
                    m_vertexInfo[i].Z = m_sourceFile.ReadSingle();
                    m_vertexInfo[i].NX = m_sourceFile.ReadSingle();
                    m_vertexInfo[i].NY = m_sourceFile.ReadSingle();
                    m_vertexInfo[i].NZ = m_sourceFile.ReadSingle();
                    m_vertexInfo[i].CUVData = new UV[m_vertexInfo[i].CUVDataLength];
                    for (int j = 0; j < m_vertexInfo[i].CUVDataLength; j++)
                    {
                        m_vertexInfo[i].CUVData[j].U = m_sourceFile.ReadSingle();
                        m_vertexInfo[i].CUVData[j].V = m_sourceFile.ReadSingle();
                    }
                }

                if (fileId == 0x0d0000ca)
                {
                    Debugger.Break();
                }

                MeshParser meshParser = new MeshParser(m_sourceFile);

                T2TrifanSet =
                    meshParser.ParseTrifanSet(true, m_t2TrifanCount, TrifanType.T2, (int)m_unkBytesAfterTrifans);

                //TrifanInfo[] trifans = new TrifanInfo[(int)m_t1TrifanCount];
                //for (int i = 0; i < trifans.Length; i++)
                //{
                //    trifans[i] = meshParser.ParseTrifanInfo();
                //}

                T1TrifanSet = meshParser.ParseTrifanSet(true, m_t1TrifanCount, TrifanType.T1);

                if (!m_sourceFile.HasReachedEnd)
                {
                    uint nextVal = m_sourceFile.ReadUInt32();
                    if (nextVal == 1)
                    {
                        BP tmpBP2 =  meshParser.ParseTaggedRecord(TrifanType.T0);
                        m_sourceFile.AlignToDwordBoundary();
                    }
                    else
                    {
                        if (!m_sourceFile.HasReachedEnd)
                        {
                            // break, unknown condition
                            Debugger.Break();
                        }
                    }
                }

                // read tagged record, if needed
                if (!m_sourceFile.HasReachedEnd)
                {
                    m_taggedElementStart = m_sourceFile.Position;
                    m_taggedElement = new DungeonBlock(m_sourceFile.CreateCopy(), m_taggedElementStart);
                    uint newPosition = m_taggedElement.ReadDungeonBlock();
                    m_sourceFile.Seek(newPosition);
                }

                Debug.Assert(m_sourceFile.HasReachedEnd);
            }
            finally
            {
                lastFilePos =  (uint)m_sourceFile.Position;
                m_sourceFile.FileReadingComplete();
            }
            return lastFilePos;
        }

        public TrifanSet T2TrifanSet
        {
            get { return m_T2TrifanSet; }
            set { m_T2TrifanSet = value; }
        }

        public TrifanSet T1TrifanSet
        {
            get { return m_T1TrifanSet; }
            set { m_T1TrifanSet = value; }
        }

        public uint VertexCount
        {
            get { return m_vertexCount; }
            set { m_vertexCount = value; }
        }

        public VertexInfo[] VertexInfo
        {
            get { return m_vertexInfo; }
            set { m_vertexInfo = value; }
        }

        public EmbeddedFile SourceFile
        {
            get { return m_sourceFile; }
        }

        public uint Flags
        {
            get { return m_flags; }
        }

        /// <summary>
        /// The source file
        /// </summary>
        private EmbeddedFile m_sourceFile;

        private uint m_flags;
        private uint m_textureCount;
        private uint m_vertexCount;
        private VertexInfo[] m_vertexInfo;
        private uint m_t2TrifanCount;
        private TrifanSet m_T2TrifanSet;
        private TrifanSet m_T1TrifanSet;

        private uint m_t1TrifanCount;
        private uint m_unkBytesAfterTrifans;
        private uint m_unk4;

        private DungeonBlock m_taggedElement;
        private int m_taggedElementStart;
        private int m_myStartPos;
    }
}
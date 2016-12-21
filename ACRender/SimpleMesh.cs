using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace Gawa.ACRender
{
    /// <summary>
    /// Defines a single, simple mesh. Same as a 0x01nnnnnn object from the
    /// portal.dat
    /// </summary>
    public class SimpleMesh
    {
        public SimpleMesh(EmbeddedFile sourceFile)
        {
            m_sourceFile = sourceFile;
        }

        public SimpleMesh(DungeonBlock dungeonBlock)
        {
            m_sourceFile = dungeonBlock.SourceFile;
            m_firstTrifanSet = null;
            m_flags = 0;
            m_secondTrifanSet = dungeonBlock.T2TrifanSet;
            m_textureCount = 0;
            m_textureInfo = null;
            m_vertexCount = dungeonBlock.VertexCount;
            m_vertexInfo = dungeonBlock.VertexInfo;
        }

        /// <summary>
        /// reads the mesh from file into memory.
        /// </summary>
        public void ReadSimpleMesh()
        {
            bool breakAfterParsing = false;
            try
            {
                m_sourceFile.PrepareFileForReading();

                // file id, assert that it matches the file
                uint fileId = m_sourceFile.ReadUInt32();
                Debug.Assert(fileId == m_sourceFile.FileId);

                // flags & texture IDs
                m_flags = m_sourceFile.ReadUInt32();
                Debug.Assert(m_flags == 0x02 || m_flags == 0x03 || m_flags == 0x0a || m_flags == 0x0b);   // check known flag values

                if (fileId == 0x01000007)
                {
                    //Debugger.Break();
                }

                if (m_sourceFile.DatType == DatType.Portal_ToD)
                {
                    m_textureCount = m_sourceFile.ReadByte();
                }
                else
                {
                    m_textureCount = m_sourceFile.ReadUInt32();
                }
                m_textureInfo = new uint[m_textureCount];
                for (int i = 0; i < m_textureCount; i++)
                {
                    m_textureInfo[i] = m_sourceFile.ReadUInt32();
                }

                // unknown value, always 0x01
                uint unknown1 = m_sourceFile.ReadUInt32();
                Debug.Assert(unknown1 == 0x01);

                // vertex data
                m_vertexCount = m_sourceFile.ReadUInt32();
                m_vertexInfo = new VertexInfo[m_vertexCount];
                for (int i = 0; i < m_vertexCount; i++)
                {
                    m_vertexInfo[i].VertexIndex = m_sourceFile.ReadUInt16();
                    Debug.Assert(i == m_vertexInfo[i].VertexIndex);
                    m_vertexInfo[i].CUVDataLength = m_sourceFile.ReadUInt16();
                    if (m_vertexInfo[i].CUVDataLength < m_textureCount)
                    {
                        breakAfterParsing = true;
                    }
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

                // look at the flags and decide what comes next. The two least significant
                // bits decide on how many substructures will follow (one or two).
                bool hasSetOne = ((m_flags & 0x01) == 0x01);
                bool hasSetTwo = ((m_flags & 0x02) == 0x02);
                MeshParser meshParser = new MeshParser(m_sourceFile);

                if (hasSetOne)
                {
                    m_firstTrifanSet = meshParser.ParseTrifanSet(true, m_sourceFile.ReadUInt32(), TrifanType.T1);
                }
                if (hasSetTwo)
                {
                    meshParser.ReadThreeFloats(out m_unknown3_a, out m_unknown3_b, out m_unknown3_c);

                    uint trifanCount;

                    if (m_sourceFile.DatType == DatType.Portal_ToD)
                    {
                        trifanCount = m_sourceFile.ReadByte();
                    }
                    else
                    {
                        trifanCount = m_sourceFile.ReadUInt32();
                    }
                    m_secondTrifanSet = meshParser.ParseTrifanSet(true, trifanCount, TrifanType.T0);
                }

                if (m_sourceFile.DatType == DatType.Portal_ToD)
                {
                    if ((m_flags & 0x08) == 0x08)
                    {
                        // read uint 32 directing us at some 0x11?????? file
                        uint elevenFile = m_sourceFile.ReadUInt32();
                    }
                }

                Debug.Assert(m_sourceFile.HasReachedEnd);
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
            if (breakAfterParsing)
            {
                //Debugger.Break();
            }
        }

        public uint[] TextureInfo
        {
            get { return m_textureInfo; }
        }

        public uint VertexCount
        {
            get { return m_vertexCount; }
        }

        public VertexInfo[] VertexInfo
        {
            get { return m_vertexInfo; }
        }

        public uint ID
        {
            get { return m_sourceFile.FileId; }
        }

        public TrifanSet FirstTrifanSet
        {
            get { return m_firstTrifanSet; }
        }

        public TrifanSet SecondTrifanSet
        {
            get { return m_secondTrifanSet; }
        }

        public bool HasFirstTrifanSet
        {
            get { return (m_firstTrifanSet != null); }
        }

        public bool HasSecondTrifanSet
        {
            get { return (m_secondTrifanSet != null); }
        }

        /// <summary>
        /// source file.
        /// </summary>
        private EmbeddedFile m_sourceFile;

        private uint m_flags;
        private uint m_textureCount;
        private uint[] m_textureInfo;
        private uint m_vertexCount;
        private float m_unknown3_a;
        private float m_unknown3_b;
        private float m_unknown3_c;
        private VertexInfo[] m_vertexInfo;
        private TrifanSet m_firstTrifanSet;
        private TrifanSet m_secondTrifanSet;
    }

    public struct UV
    {
        public float U;
        public float V;

        public override string ToString()
        {
            return String.Format("u;v = {0}; {1}", U, V);
        }
    }

    /// <summary>
    /// Structure for storing vertex data
    /// </summary>
    public struct VertexInfo
    {
        public ushort VertexIndex;
        public float X, Y, Z;
        public float NX, NY, NZ;
        public ushort CUVDataLength;
        public UV[] CUVData;

        public override string ToString()
        {
            if (CUVData != null)
            {
                return String.Format("VertexInfo, cuv length={0}", CUVData.Length);
            }
            return String.Format("VertexInfo");
        }
    }

    /// <summary>
    /// Structure for storing a trifan set (trifans + trailing recursive structure)
    /// </summary>
    public class TrifanSet
    {
        public TrifanSet()
        {
        }

        public BP BP
        {
            get { return m_bp; }
            set { m_bp = value; }
        }

        public uint TrifanCount
        {
            get { return m_trifanCount; }
            set { m_trifanCount = value; }
        }

        public TrifanInfo[] TrifanData
        {
            get { return m_trifanData; }
            set { m_trifanData = value; }
        }

        private BP m_bp;
        private uint m_trifanCount;
        private TrifanInfo[] m_trifanData;
    }

    /// <summary>
    /// Structure for storing triangle fan data
    /// </summary>
    public struct TrifanInfo
    {
        public ushort TrifanIndex;
        public byte VertexCount;
        public byte Flags;
        public ushort[] VertexIndices;

        public uint Unknown4;
        public uint Unknown5;
        public ushort UVDepth;
        public uint TextureIndex;
        public byte[] UVIndex;

        public override string ToString()
        {
            return String.Format("trifan, count={0}  uv length={1}  texture={2} flags={3}", VertexCount, (UVIndex != null ? UVIndex.Length : -1), TextureIndex, Flags);
        }
    }
}

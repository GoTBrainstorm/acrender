using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Gawa.ACRender
{
    /// <summary>
    /// Represents a dungeon in the cell.dat
    /// </summary>
    public class Dungeon
    {
        public Dungeon(ushort id)
        {
            m_id = id;
        }

        /// <summary>
        /// Read the dungeon from the source file
        /// </summary>
        public void ReadDungeon()
        {
            DatReader cellFile = DataProvider.Instance.CellDatReader;

            // find out how many files are available
            uint startId = (((uint)m_id << 16) | (uint)0x100);
            uint id = startId;
            while (cellFile.LocateFile(id) != null)
            {
                id++;
            }

            uint blockCount = id - startId;
            m_blocks = new DungeonBlockInfo[blockCount];
            for (uint i = 0; i < blockCount; i++)
            {
                id = startId + i;
                m_blocks[i] = new DungeonBlockInfo(cellFile.DatType, cellFile.LocateFile(id));
                m_blocks[i].ReadDungeonBlockInfo();
            }
        }

        /// <summary>
        /// ID of the dungeon
        /// </summary>
        private ushort m_id;
        /// <summary>
        /// Buidling blocks of this dungeon
        /// </summary>
        private DungeonBlockInfo[] m_blocks;
    }

    public class DungeonBlockInfo
    {
        public DungeonBlockInfo(DatType sourceType, EmbeddedFile sourceFile)
        {
            m_sourceFile = sourceFile;
            m_sourceType = sourceType;
        }

        private readonly DatType m_sourceType;
  
        /// <summary>
        /// Read the dungeon from the source file
        /// </summary>
        public void ReadDungeonBlockInfo()
        {
            try
            {
                m_sourceFile.PrepareFileForReading();
                if (m_sourceType == DatType.Cell_ToD)
                {
                    // skip additional ID
                    m_sourceFile.Skip(sizeof(uint));
                }
                m_type = m_sourceFile.ReadUInt32();
                uint id = m_sourceFile.ReadUInt32();
                Debug.Assert(id == m_sourceFile.FileId);
                m_textureCount = m_sourceFile.ReadByte();
                m_connectionCount = m_sourceFile.ReadByte();
                m_visibilityCount = m_sourceFile.ReadUInt16();
                m_textureIDs = new ushort[m_textureCount]; 
                for (int i = 0; i < m_textureCount; i++)
                {
                    m_textureIDs[i] = m_sourceFile.ReadUInt16();
                }
                if (m_sourceType == DatType.Cell_ToD)
                {
                    m_geometryId = m_sourceFile.ReadUInt16();
                    m_sourceFile.AlignToDwordBoundary();
                }
                else
                {
                    m_sourceFile.AlignToDwordBoundary();
                    m_geometryId = m_sourceFile.ReadUInt32();
                }

                if (id == 0x0138016f)
                {
                    Debugger.Break();
                }

                // unknown byte, always zero?
                uint unk1 = m_sourceFile.ReadUInt32();
                //Debug.Assert(unk1 == ((id & 0x0FFFF) - 0x100));

                m_x = m_sourceFile.ReadSingle();
                m_y = m_sourceFile.ReadSingle();
                m_z = m_sourceFile.ReadSingle();
                m_a = m_sourceFile.ReadSingle();
                m_b = m_sourceFile.ReadSingle();
                m_c = m_sourceFile.ReadSingle();
                m_d = m_sourceFile.ReadSingle();

                if (m_sourceType == DatType.Cell_ToD)
                {
                    m_sourceFile.Skip((3 + (m_connectionCount - 1) * 4) * 2);       // words            // 2: 7  1: 3
                }
                else
                {
                    m_connectivity = new ConnectionInfo[m_connectionCount];
                    for (int i = 0; i < m_connectionCount; i++)
                    {
                        m_connectivity[i].Unk1 = m_sourceFile.ReadUInt32();
                        m_connectivity[i].Unk2 = m_sourceFile.ReadUInt32();
                    }
                }

                m_visibilityData = new ushort[m_visibilityCount];
                for (int i = 0; i < m_visibilityCount; i++)
                {
                    m_visibilityData[i] = m_sourceFile.ReadUInt16();
                }

                // tod does not align anymore
                if (m_sourceType != DatType.Cell_ToD)
                {
                    m_sourceFile.AlignToDwordBoundary();
                }

                if (HasObjects)
                {

                    m_objectCount = m_sourceFile.ReadUInt32();
                    m_objects = new LandscapeObject[m_objectCount];
                    for (int i = 0; i < m_objectCount; i++)
                    {
                        m_objects[i].ID = m_sourceFile.ReadUInt32();
                        m_objects[i].X = m_sourceFile.ReadSingle();
                        m_objects[i].Y = m_sourceFile.ReadSingle();
                        m_objects[i].Z = m_sourceFile.ReadSingle();
                        m_objects[i].A = m_sourceFile.ReadSingle();
                        m_objects[i].B = m_sourceFile.ReadSingle();
                        m_objects[i].C = m_sourceFile.ReadSingle();
                        m_objects[i].D = m_sourceFile.ReadSingle();
                    }
                }
                //if (!SourceFile.HasReachedEnd)
                //{
                //    Debugger.Break();
                //}
                Debug.Assert(m_sourceFile.HasReachedEnd);
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
        }

        /// <summary>
        /// Gets whether this dungeon block is a surface structure
        /// </summary>
        public bool IsSurfaceStructure
        {
            get { return (m_type & 0x01) == 0x01; }
        }

        public bool HasObjects
        {
            get { return (m_type & 0x02) == 0x02; }
        }

        public override string ToString()
        {
            return String.Format("dngblinf: id {2:X8}, block {0:X4}, textures {1:X}, ", m_geometryId, m_textureCount, m_sourceFile.FileId);
        }

        /// <summary>
        /// The source file
        /// </summary>
        private EmbeddedFile m_sourceFile;
        private uint m_type;
        private byte m_textureCount;
        private byte m_connectionCount;
        private ushort m_visibilityCount;
        private ushort[] m_textureIDs;
        private uint m_geometryId;
        private float m_x, m_y, m_z, m_a, m_b, m_c, m_d;
        private ConnectionInfo[] m_connectivity;
        private ushort[] m_visibilityData;
        private uint m_objectCount;
        private LandscapeObject[] m_objects;
    }

    public struct ConnectionInfo
    {
        public uint Unk1;
        public uint Unk2;
    }

    public struct LandscapeObject
    {
        public uint ID;
        public float X;
        public float Y;
        public float Z;
        public float A;
        public float B;
        public float C;
        public float D;
    }
}

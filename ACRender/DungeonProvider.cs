using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    public class DungeonProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private DungeonProvider()
        {
            m_cellreader = DataProvider.Instance.CellDatReader;
            m_portalReader = DataProvider.Instance.PortalDatReader;
        }

        public static DungeonProvider Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new DungeonProvider();
                }
                return m_instance;
            }
        }

        public List<ushort> GetDungeonList()
        {
            List<EmbeddedFileEntry> files = m_cellreader.GetFileIDsByMask(0x0000FFFF, 0x00000100);
            List<ushort> idList = new List<ushort>();
            foreach (EmbeddedFileEntry entry in files)
            {
                idList.Add((ushort)((entry.FileID & 0xFFFF0000) >> 16));
            }
            return idList;
        }

        public Dungeon GetDungeon(ushort id)
        {
            Dungeon dungeon = new Dungeon(id);
            dungeon.ReadDungeon();
            return dungeon;
        }

        public List<EmbeddedFileEntry> GetDungeonBlockList()
        {
            return m_portalReader.GetFileIDs(0x0D000000, 0x0DFFFFFF);
        }

        /// <summary>
        /// Get the specified dungeon block.
        /// </summary>
        /// <param name="blockId">The block number (0x0Dnnnnnn)</param>
        /// <returns>The dungeon block, or nul if it could not be found.</returns>
        public DungeonBlock GetDungeonBlock(uint blockId)
        {
            EmbeddedFile file = m_portalReader.LocateFile(blockId);
            if (file == null)
            {
                return null;
            }
            DungeonBlock block = new DungeonBlock(file);
            block.ReadDungeonBlock();
            return block;
        }

        /// <summary>
        /// Reader of the cell.dat
        /// </summary>
        private DatReader m_cellreader;
        /// <summary>
        /// Reader of the portal.dat
        /// </summary>
        private DatReader m_portalReader;
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static DungeonProvider m_instance;
    }
}

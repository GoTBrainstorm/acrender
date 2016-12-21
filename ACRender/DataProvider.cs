using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    /// <summary>
    /// Helper class for reading data from the data files
    /// </summary>
    public class DataProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private DataProvider()
        {
            m_portalReader = new DatReader(Settings.PortalDatFile, Settings.PortalDatType);
            m_portalReader.Initialize(false);
            m_cellReader = new DatReader(Settings.CellDatFile, Settings.CellDatType);
            m_cellReader.Initialize(false);
        }

        public static DataProvider Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new DataProvider();
                }
                return m_instance;
            }
        }

        /// <summary>
        /// Get the portal.dat reader
        /// </summary>
        public DatReader PortalDatReader
        {
            get { return m_portalReader; }
        }

        /// <summary>
        /// Get the cell.dat reader
        /// </summary>
        public DatReader CellDatReader
        {
            get { return m_cellReader; }
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static DataProvider m_instance;
        /// <summary>
        /// Reader for the portal.dat
        /// </summary>
        private DatReader m_portalReader;
        /// <summary>
        /// Reader for the cell.dat
        /// </summary>
        private DatReader m_cellReader;
    }
}

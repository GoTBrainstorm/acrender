using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    /// <summary>
    /// Helper class for providing mesh data
    /// </summary>
    public class MeshProvider
    {
        private MeshProvider()
        {
            m_portalReader = DataProvider.Instance.PortalDatReader;
        }

        public static MeshProvider Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MeshProvider();
                }
                return m_instance;
            }
        }

        /// <summary>
        /// Loads the simple mesh with the specified ID
        /// </summary>
        /// <param name="id">The ID of the simple mesh</param>
        /// <returns>The simple mesh, or null if it couldn't be found</returns>
        public SimpleMesh LoadSimpleMesh(uint id)
        {
            EmbeddedFile file = m_portalReader.LocateFile(id);
            if (file == null)
            {
                return null;
            }
            SimpleMesh mesh = new SimpleMesh(file);
            mesh.ReadSimpleMesh();
            return mesh;
        }

        public List<EmbeddedFileEntry> GetSimpleMeshList()
        {
            return m_portalReader.GetFileIDs(0x01000000, 0x01FFFFFF);
        }

        public ComplexMesh LoadComplexMesh(uint id)
        {
            EmbeddedFile file = m_portalReader.LocateFile(id);
            if (file == null)
            {
                return null;
            }
            ComplexMesh mesh = new ComplexMesh(file);
            mesh.ReadComplexMesh();
            return mesh;
        }

        public List<EmbeddedFileEntry> GetComplexMeshList()
        {
            return m_portalReader.GetFileIDs(0x02000000, 0x02FFFFFF);
        }

        public List<EmbeddedFileEntry> GetComplexMeshListBySimpleMeshID(uint simpleMeshID)
        {
            List<EmbeddedFileEntry> rawList = GetComplexMeshList();
            List<EmbeddedFileEntry> resultList = new List<EmbeddedFileEntry>();
            foreach (EmbeddedFileEntry entry in rawList)
            {
                try
                {
                    ComplexMesh mesh = LoadComplexMesh(entry.FileID);
                    foreach (uint id in mesh.SimpleMeshIDs)
                    {
                        if (id == simpleMeshID)
                        {
                            resultList.Add(entry);
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }
            return resultList;
        }

        private static MeshProvider m_instance;
        private DatReader m_portalReader;
    }
}

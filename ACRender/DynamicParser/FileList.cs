using System;
using System.Collections.Generic;
using System.Text;
using Gawa.ACRender;
using System.Diagnostics;
using System.IO;
using Gawa.ACRender.DynamicParser.CachedFileList;

namespace Gawa.ACRender.DynamicParser
{
    /// <summary>
    /// Caches the lists of file entries in the different dat files
    /// </summary>
    public class FileList
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="datReader">Data file for which the file list needs to
        /// be constructed.</param>
        public FileList(DatReader datReader)
        {
            Debug.Assert(datReader != null);
            m_sourceHash = datReader.GetFileNameHash();
            m_datReader = datReader;
            m_fileGroups = new Dictionary<int, FileGroup>();
        }

        /// <summary>
        /// (Re)loads the list. A cached version of the list will be loaded
        /// if available.
        /// </summary>
        /// <param name="disableCache">Do not use caching, this means that the
        /// list will always be recalculated.</param>
        public void LoadList(bool disableCache)
        {
            if (disableCache || !LoadCachedList())
            {
                RecalculateList();
            }
        }

        /// <summary>
        /// Recalculates the file list from scratch.
        /// </summary>
        private void RecalculateList()
        {
            m_fileGroups.Clear();

            // get the files
            List<EmbeddedFileEntry> files = m_datReader.GetFileIDs(uint.MinValue, uint.MaxValue);

            // create a temporary list of groups an their entries
            Dictionary<int, List<uint>> tempList = new Dictionary<int, List<uint>>();
            foreach (EmbeddedFileEntry entry in files)
            {
                int fileGroup = (int)((entry.FileID & 0xFF000000) >> 24);
                if (!tempList.ContainsKey(fileGroup))
                {
                    tempList.Add(fileGroup, new List<uint>());
                }
                tempList[fileGroup].Add(entry.FileID);
            }

            // sort everything
            foreach (List<uint> list in tempList.Values)
            {
                list.Sort();
            }

            // create a memory model
            foreach (int fileGroupID in tempList.Keys)
            {
                uint[] fileGroupEntries = tempList[fileGroupID].ToArray();
                FileGroup group = new FileGroup();
                group.number = fileGroupID;
                group.entry = fileGroupEntries;
                m_fileGroups.Add(fileGroupID, group);
            }

            SaveList();
        }

        /// <summary>
        /// Loads the cached list
        /// </summary>
        /// <returns>Returns whether loading of the cached list
        /// succeeded.</returns>
        private bool LoadCachedList()
        {
            try
            {
                FileInfo cacheFile = new FileInfo(Settings.FileListCacheDir + m_sourceHash + ".xml");
                if (cacheFile.Exists)
                {
                    ObjectFromXMLDeserializer<CachedFileList.CachedFileList> deser = new ObjectFromXMLDeserializer<Gawa.ACRender.DynamicParser.CachedFileList.CachedFileList>();
                    XmlDeserializationResult<CachedFileList.CachedFileList> deserResult = deser.Deserialize(cacheFile.FullName, XSDType.CachedFileList);
                    if (deserResult.Success)
                    {
                        m_fileGroups.Clear();
                        foreach (FileGroup group in deserResult.Value.FileGroup)
                        {
                            m_fileGroups.Add(group.number, group);
                        }
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the list
        /// </summary>
        private void SaveList()
        {
            CachedFileList.CachedFileList fileList = new CachedFileList.CachedFileList();
            fileList.FileGroup = new FileGroup[m_fileGroups.Count];
            int i = 0;
            foreach (FileGroup group in m_fileGroups.Values)
            {
                fileList.FileGroup[i] = group;
                i++;
            }

            string xmlFile = ObjectToXMLSerializer.SerializeObject(fileList);
            FileStream fs = null;
            StreamWriter writer = null;
            try
            {
                fs = new FileStream(Settings.FileListCacheDir + m_sourceHash + ".xml", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(fs);
                writer.Write(xmlFile);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Gets a list of file group IDs
        /// </summary>
        /// <returns>List of group IDs</returns>
        public int[] GetFileGroups()
        {
            int[] groupIDs = new int[m_fileGroups.Count];
            int i = 0;
            foreach (int key in m_fileGroups.Keys)
            {
                groupIDs[i] = key;
                i++;
            }
            Array.Sort(groupIDs);
            return groupIDs;
        }

        /// <summary>
        /// Get the contents of a group (a list of file IDs)
        /// </summary>
        public uint[] GetGroupContents(int groupId)
        {
            FileGroup group = m_fileGroups[groupId];
            return group.entry;
        }

        /// <summary>
        /// Identifier ash of the source
        /// </summary>
        private string m_sourceHash;
        /// <summary>
        /// Dat reader that contains the source
        /// </summary>
        private DatReader m_datReader;
        /// <summary>
        /// File groups
        /// </summary>
        private Dictionary<int, FileGroup> m_fileGroups;
    }
}

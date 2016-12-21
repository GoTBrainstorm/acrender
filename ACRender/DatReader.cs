using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gawa.ACRender
{
    /// <summary>
    /// Provides functions to read the Asheron's Call data files.
    /// </summary>
    public class DatReader : IDisposable
    {
        /// <summary>
        /// Whether a directory can have more subdirectories than there are files
        /// </summary>
        private bool m_canHaveMoreSubDirsThanFiles;

        /// <summary>
        /// The type of dat file that we're dealing with.
        /// </summary>
        private readonly DatType m_datType;

        /// <summary>
        /// Offset within an entry
        /// </summary>
        private int m_entryDataOffset;

        /// <summary>
        /// Size of a directory entry
        /// </summary>
        private int m_entrySize;

        /// <summary>
        /// Offset within a directory where the amount of entries can be found.
        /// </summary>
        private int m_fileCountOffset;

        private Dictionary<uint, EmbeddedFileEntry> m_filePosCache;

        /// <summary>
        /// Maximum amount of files per directory
        /// </summary>
        private int m_filesPerDirectory;

        /// <summary>
        /// File stream, used to read the data from.
        /// </summary>
        private FileStream m_inputStream;

        /// <summary>
        /// Loaction of the root directory
        /// </summary>
        private uint m_rootPointer;

        /// <summary>
        /// Size of a file sector, in amount of uints
        /// </summary>
        private int m_sectorSize;

        /// <summary>
        /// Amount of sectors that are at most needed to form a directory
        /// </summary>
        private int m_sectorsPerDirectory;

        /// <summary>
        /// The actual file that we're reading from.
        /// </summary>
        private readonly FileInfo m_sourceFile;

        public DatReader(FileInfo sourceFile, DatType datType)
        {
            Debug.Assert(sourceFile != null && sourceFile.Exists);
            m_sourceFile = sourceFile;
            m_datType = datType;
        }

        /// <summary>
        /// Get the type of dat file that we're reading from.
        /// </summary>
        public DatType DatType
        {
            get
            {
                return m_datType;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the DatReader
        /// </summary>
        public void Dispose()
        {
            if (m_inputStream != null)
            {
                m_inputStream.Dispose();
                m_inputStream = null;
            }
        }

        #endregion

        public void Initialize(bool doCache)
        {
            // Determines some parameters that are needed for reading the dat file.
            // These parameters are based on the type of dat file.
            switch (m_datType)
            {
                case DatType.Cell_ACDM:
                    m_sectorSize = 64;
                    m_rootPointer = 0x148;
                    m_entrySize = 3;
                    m_entryDataOffset = 0;
                    m_sectorsPerDirectory = 4;
                    m_filesPerDirectory = 63;
                    m_fileCountOffset = 63;
                    m_canHaveMoreSubDirsThanFiles = false;
                    break;
                case DatType.Cell_ToD:
                    m_sectorSize = 64;
                    m_rootPointer = 0x160;
                    m_entrySize = 6;
                    m_entryDataOffset = 1;
                    m_sectorsPerDirectory = 7;
                    m_filesPerDirectory = 63;
                    m_fileCountOffset = 63;
                    m_canHaveMoreSubDirsThanFiles = false;
                    break;
                case DatType.Portal_ToD:
                    m_sectorSize = 256;
                    m_rootPointer = 0x160;
                    m_entrySize = 6;
                    m_entryDataOffset = 1;
                    m_sectorsPerDirectory = 7;
                    m_filesPerDirectory = 63;
                    m_fileCountOffset = 63;
                    m_canHaveMoreSubDirsThanFiles = false;
                    break;
                case DatType.Portal_ACDM:
                    m_sectorSize = 256;
                    m_rootPointer = 0x148;
                    m_entrySize = 3;
                    m_entryDataOffset = 0;
                    m_sectorsPerDirectory = 1;
                    m_filesPerDirectory = 63;
                    m_fileCountOffset = 63;
                    m_canHaveMoreSubDirsThanFiles = false;
                    break;
                default:
                    throw new Exception("Unknown dat type.");
            }


            // open the filestream reader
            m_inputStream = m_sourceFile.OpenRead();

            // read the position of the actual root
            byte[] tmp = new byte[sizeof (uint)];
            m_inputStream.Seek(m_rootPointer, SeekOrigin.Begin);
            m_inputStream.Read(tmp, 0, sizeof (uint));
            m_rootPointer = BitConverter.ToUInt32(tmp, 0);

            if (doCache)
            {
                // cache file positions
                m_filePosCache = new Dictionary<uint, EmbeddedFileEntry>();
                CacheFilePositions();
            }
        }

        private void CacheFilePositions()
        {
            // get list of all files
            List<EmbeddedFileEntry> files = new List<EmbeddedFileEntry>();
            FileIDMatcher matcher = delegate { return true; };
            ListFiles(m_rootPointer, files, matcher, null);

            // construct dictionary
            foreach (EmbeddedFileEntry file in files)
            {
                m_filePosCache[file.FileID] = file;
            }
        }

        /// <summary>
        /// Locates and returns the file with the specified ID
        /// </summary>
        /// <param name="fileId">The ID of the file that we need.</param>
        /// <returns>An embedded file. Returns null if the file could not be found.</returns>
        public EmbeddedFile LocateFile(uint fileId)
        {
            uint fileLength;
            uint filePosition = FindFilePosition(fileId, out fileLength);
            if (filePosition == 0 || fileLength == 0)
            {
                return null;
            }
            else
            {
                return new EmbeddedFile(this, fileId, filePosition, fileLength);
            }
        }

        /// <summary>
        /// Retrieves the content of the specified file as a byte array
        /// </summary>
        /// <param name="file">The file that's to be retrieved</param>
        /// <returns>The file data</returns>
        public byte[] RetrieveFileData(EmbeddedFile file)
        {
            Debug.Assert(file != null);
            try
            {
                byte[] data = new byte[file.FileLength];
                int dataPtr = 0;
                uint filePointer = file.StartFilePointer;
                byte[] tmp = new byte[4];

                while (dataPtr < file.FileLength)
                {
                    // read the next sector pointer
                    m_inputStream.Seek(filePointer, SeekOrigin.Begin);
                    m_inputStream.Read(tmp, 0, tmp.Length);
                    filePointer = (BitConverter.ToUInt32(tmp, 0) & 0x7FFFFFFF);

                    // read the amount of bytes that we still need
                    int remaining = (int) (file.FileLength - dataPtr);
                    remaining = Math.Min(remaining, (m_sectorSize - 1) * sizeof (uint));
                    m_inputStream.Read(data, dataPtr, remaining);
                    dataPtr += remaining;
                }
                return data;
            }
            catch (IOException)
            {
                // all IO exceptions are considered to be an invalid attempts to
                // locate a file / read data
                return null;
            }
        }

        /// <summary>
        /// Returns a list of file IDs that are contained in this data file
        /// </summary>
        /// <param name="minValue">The minimum file ID</param>
        /// <param name="maxValue">The max file ID</param>
        /// <returns>A list of file IDs</returns>
        public List<EmbeddedFileEntry> GetFileIDs(uint minValue, uint maxValue)
        {
            List<EmbeddedFileEntry> fileList = new List<EmbeddedFileEntry>();
            ListFiles(m_rootPointer, fileList, RangeCompare, new RangeState(minValue, maxValue));
            return fileList;
        }

        public List<EmbeddedFileEntry> GetFileIDsByMask(uint mask, uint match)
        {
            List<EmbeddedFileEntry> fileList = new List<EmbeddedFileEntry>();
            ListFiles(m_rootPointer, fileList, MaskCompare, new MaskState(mask, match));
            return fileList;
        }

        private static bool RangeCompare(uint fileId, object stateObject)
        {
            RangeState state = (RangeState) stateObject;
            return (fileId >= state.MinID && fileId <= state.MaxID);
        }

        private static bool MaskCompare(uint fileId, object stateObject)
        {
            MaskState mask = (MaskState) stateObject;
            return (fileId & mask.Mask) == mask.Match;
        }

        private void ListFiles(uint filePointer, List<EmbeddedFileEntry> destinationList, FileIDMatcher matcher,
                               object stateObject)
        {
            byte[] directory = new byte[m_sectorsPerDirectory * m_sectorSize * sizeof (uint)];

            // read the directory
            Util.UIntToBArr(directory, 0, filePointer);
            for (int i = 0; i < m_sectorsPerDirectory && BitConverter.ToUInt32(directory, 0) != 0x00; i++)
            {
                // always read the next sector pointer into the first position
                // of the data array. Then read the rest of the sector to the back
                // of the array
                filePointer = BitConverter.ToUInt32(directory, 0);
                m_inputStream.Seek(filePointer, SeekOrigin.Begin);
                m_inputStream.Read(directory, 0, sizeof (uint));
                m_inputStream.Read(directory, (i * (m_sectorSize - 1) + 1) * sizeof (uint),
                                   (m_sectorSize - 1) * sizeof (uint));
            }

            // find the amount of files
            int fileCount = BitConverter.ToInt32(directory, m_fileCountOffset * sizeof (uint));
            Debug.Assert(fileCount < m_filesPerDirectory);

            // add the file numbers to the list
            for (int index = 0; index < fileCount; index++)
            {
                uint fileId =
                    BitConverter.ToUInt32(directory,
                                          (index * m_entrySize + m_filesPerDirectory + 1 + m_entryDataOffset) *
                                          sizeof(uint));
                if (fileId != 0x00)
                {
                    if (matcher(fileId, stateObject))
                    {
                        uint fileSize =
                            BitConverter.ToUInt32(directory,
                                                  (index * m_entrySize + m_filesPerDirectory + 3 + m_entryDataOffset) *
                                                  sizeof (uint));
                        EmbeddedFileEntry newEntry = new EmbeddedFileEntry(fileSize, fileId);
                        if (!destinationList.Contains(newEntry))
                        {
                            destinationList.Add(newEntry);
                        }
                    }
                }
            }

            // recurse into subdirs, if needed
            bool hasSubDirs = (BitConverter.ToUInt32(directory, sizeof (uint)) != 0);
            if (hasSubDirs)
            {
                int max = fileCount;
                if (m_canHaveMoreSubDirsThanFiles)
                {
                    max = 62;
                }

                for (int index = 0; index < max; index++)
                {
                    uint subDirPointer = BitConverter.ToUInt32(directory, index * sizeof (uint) + sizeof (uint));
                    if (subDirPointer != 0x00)
                    {
                        ListFiles(subDirPointer, destinationList, matcher, stateObject);
                    }
                }
            }
        }

        public DirectoryContents GetRootDirectoryContents()
        {
            return GetDirectoryContents(m_rootPointer);
        }

        public DirectoryContents GetDirectoryContents(uint filePointer)
        {
            byte[] directory = new byte[m_sectorsPerDirectory * m_sectorSize * sizeof (uint)];

            // read the directory
            Util.UIntToBArr(directory, 0, filePointer);
            for (int i = 0; i < m_sectorsPerDirectory && BitConverter.ToUInt32(directory, 0) != 0x00; i++)
            {
                // always read the next sector pointer into the first position
                // of the data array. Then read the rest of the sector to the back
                // of the array
                filePointer = BitConverter.ToUInt32(directory, 0);
                m_inputStream.Seek(filePointer, SeekOrigin.Begin);
                m_inputStream.Read(directory, 0, sizeof (uint));
                m_inputStream.Read(directory, (i * (m_sectorSize - 1) + 1) * sizeof (uint),
                                   (m_sectorSize - 1) * sizeof (uint));
            }

            // find the amount of files
            int fileCount = BitConverter.ToInt32(directory, m_fileCountOffset * sizeof (uint));
            Debug.Assert(fileCount < m_filesPerDirectory);

            DirectoryContents contents = new DirectoryContents();

            // add filenumbers to the list
            for (int index = 0; index < fileCount; index++)
            {
                uint fileId =
                    BitConverter.ToUInt32(directory,
                                          (index * m_entrySize + m_filesPerDirectory + 1 + m_entryDataOffset) *
                                          sizeof (uint));
                contents.m_fileIds.Add(fileId);
            }

            // add subdirectories
            bool hasSubDirs = (BitConverter.ToUInt32(directory, sizeof (uint)) != 0);
            if (hasSubDirs)
            {
                int max = fileCount;
                if (m_canHaveMoreSubDirsThanFiles)
                {
                    max = 62;
                }

                for (int index = 0; index < max; index++)
                {
                    uint subDirPointer = BitConverter.ToUInt32(directory, index * sizeof (uint) + sizeof (uint));
                    contents.m_subDirLocations.Add(subDirPointer);
                }
            }

            return contents;
        }

        /// <summary>
        /// Finds the position of a specific file withing the data file
        /// </summary>
        /// <param name="fileId">Id of the file that we're looking for.</param>
        /// <returns>Position of the file, if 0 is returned then
        /// the file could not be found.</returns>
        /// <param name="fileLength">Length of the file</param>
        private uint FindFilePosition(uint fileId, out uint fileLength)
        {
            // load from the cache if possible
            //if (m_filePosCache != null)
            //{
            //    if (m_filePosCache.ContainsKey(fileId))
            //    {
            //        EmbeddedFileEntry entry = m_filePosCache[fileId];
            //        fileLength = entry.Size;
            //        return entry.FileID;
            //    }
            //    else
            //    {
            //        fileLength = UInt32.MaxValue;
            //        return UInt32.MaxValue;
            //    }
            //}

            try
            {
                // reserve enough space for a single directory
                byte[] directory = new byte[m_sectorsPerDirectory * m_sectorSize * sizeof (uint)];
                uint filePointer = m_rootPointer; // start at the root

                // read until we return with a position
                while (true)
                {
                    // read the directory
                    Util.UIntToBArr(directory, 0, filePointer);
                    for (int i = 0; i < m_sectorsPerDirectory && BitConverter.ToUInt32(directory, 0) != 0x00; i++)
                    {
                        // always read the next sector pointer into the first position
                        // of the data array. Then read the rest of the sector to the back
                        // of the array
                        filePointer = BitConverter.ToUInt32(directory, 0);
                        m_inputStream.Seek(filePointer, SeekOrigin.Begin);
                        m_inputStream.Read(directory, 0, sizeof (uint));
                        m_inputStream.Read(directory, (i * (m_sectorSize - 1) + 1) * sizeof (uint),
                                           (m_sectorSize - 1) * sizeof (uint));
                    }
                    uint[] tmpDbg = Util.BArrToUintArr(directory);

                    // find the amount of files
                    int fileCount = BitConverter.ToInt32(directory, m_fileCountOffset * sizeof (uint));
                    Debug.Assert(fileCount < m_filesPerDirectory);

                    int index = -1;
                    uint tmpFile;
                    do
                    {
                        index++;
                        tmpFile =
                            BitConverter.ToUInt32(directory,
                                                  (index * m_entrySize + m_filesPerDirectory + 1 + m_entryDataOffset) *
                                                  sizeof (uint));
                    }
                    while (index < fileCount && fileId > tmpFile);

                    // if we found the file or the position of the subdirectory.
                    if (index < fileCount)
                    {
                        // if this is the file we're looking for.
                        uint fileIdAtIndex =
                            BitConverter.ToUInt32(directory,
                                                  (index * m_entrySize + m_filesPerDirectory + 1 + m_entryDataOffset) *
                                                  sizeof (uint));
                        if (fileId == fileIdAtIndex)
                        {
                            fileLength =
                                BitConverter.ToUInt32(directory,
                                                      sizeof (uint) *
                                                      (index * m_entrySize + m_filesPerDirectory + 3 + m_entryDataOffset));
                            return
                                BitConverter.ToUInt32(directory,
                                                      sizeof (uint) *
                                                      (index * m_entrySize + m_filesPerDirectory + 2 + m_entryDataOffset));
                        }
                    }

                    if (BitConverter.ToUInt32(directory, 1 * sizeof (uint)) == 0)
                    {
                        fileLength = 0;
                        return 0;
                    }

                    filePointer = BitConverter.ToUInt32(directory, (index + 1) * sizeof (uint));
                }
            }
            catch (IOException)
            {
                // all IO exceptions are considered to be an invalid attempt to
                // locate a file.
                fileLength = 0;
                return 0;
            }
        }

        /// <summary>
        /// Gets the hashed source name.
        /// </summary>
        /// <returns>Hashed source name</returns>
        public string GetFileNameHash()
        {
            byte[] buffer = Encoding.Unicode.GetBytes(m_sourceFile.FullName);
            buffer = MD5.Create().ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }

    internal delegate bool FileIDMatcher(uint fileId, object stateObject);

    internal struct RangeState
    {
        public uint MaxID;
        public uint MinID;

        public RangeState(uint minId, uint maxId)
        {
            MinID = minId;
            MaxID = maxId;
        }
    }

    internal struct MaskState
    {
        public uint Mask;
        public uint Match;

        public MaskState(uint mask, uint match)
        {
            Mask = mask;
            Match = match;
        }
    }

    public class DirectoryContents
    {
        public List<uint> m_fileIds;
        public List<uint> m_subDirLocations;

        public DirectoryContents()
        {
            m_subDirLocations = new List<uint>();
            m_fileIds = new List<uint>();
        }
    }
}
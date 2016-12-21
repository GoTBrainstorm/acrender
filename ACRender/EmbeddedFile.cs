using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Gawa.ACRender
{
    /// <summary>
    /// Defines a single file, embedded in the data files.
    /// </summary>
    public class EmbeddedFile
    {
        /// <summary>
        /// Constructs a new embedded file
        /// </summary>
        /// <param name="dataSource">The source we're reading data from</param>
        /// <param name="fileId">The ID of the file</param>
        /// <param name="filePosition">The position of the start of the file witin
        /// the data source.</param>
        /// <param name="fileLength">The length of the file, in bytes.</param>
        public EmbeddedFile(DatReader dataSource, uint fileId, uint filePosition, uint fileLength)
        {
            Debug.Assert(dataSource != null);
            Debug.Assert(fileId > 0);
            Debug.Assert(filePosition > 0);
            Debug.Assert(fileLength > 0);

            m_dataSource = dataSource;
            m_fileId = fileId;
            m_fileLength = fileLength;
            m_startFilePointer = filePosition;
            m_fileData = null;
        }

        /// <summary>
        /// Clones the embedded files, but it won't be open for reading yet
        /// </summary>
        /// <returns></returns>
        public EmbeddedFile CreateCopy()
        {
            EmbeddedFile clone = new EmbeddedFile(m_dataSource, m_fileId, m_startFilePointer, m_fileLength);
            return clone;
        }

        /// <summary>
        /// Cache the data of this file, as a preparation of reading in it.
        /// </summary>
        public void PrepareFileForReading()
        {
            m_fileData = m_dataSource.RetrieveFileData(this);
            Debug.Assert(m_fileLength == m_fileData.Length);
            Rewind();

            m_debugData = new uint[m_fileData.Length / 4];
            m_debugshorts = new ushort[m_fileData.Length / 2];
            m_debugDataFloats = new float[m_fileData.Length / 4];
            for (int i = 0; i < m_fileData.Length / 4; i++)
            {
                m_debugData[i] = BitConverter.ToUInt32(m_fileData, i * 4);
                m_debugDataFloats[i] = BitConverter.ToSingle(m_fileData, i * 4);
            }
            for (int i = 0; i < m_fileData.Length / 2; i++)
            {
                m_debugshorts[i] = BitConverter.ToUInt16(m_fileData, i * 2);
            }
        }

        /// <summary>
        /// Delete the cached data of this file.
        /// </summary>
        public void FileReadingComplete()
        {
            m_fileData = null;
        }

        /// <summary>
        /// Rewinds the file to the start of the data
        /// </summary>
        public void Rewind()
        {
            Seek(0);
        }

        /// <summary>
        /// Seek to the specified file position.
        /// </summary>
        /// <param name="position">The position to seek to.</param>
        public void Seek(uint position)
        {
            Debug.Assert(position <= m_fileLength);
            m_internalPointer = (int)position;
        }

        /// <summary>
        /// Reads the next uint32 from the file data
        /// </summary>
        /// <returns></returns>
        public UInt32 ReadUInt32()
        {
            Debug.Assert(m_internalPointer <= m_fileLength - sizeof(UInt32));
            UInt32 val = BitConverter.ToUInt32(m_fileData, m_internalPointer);
            m_internalPointer += sizeof(UInt32);
            return val;
        }

        /// <summary>
        /// Reads the next int32 from the file data
        /// </summary>
        /// <returns></returns>
        public Int32 ReadInt32()
        {
            Debug.Assert(m_internalPointer <= m_fileLength - sizeof(Int32));
            Int32 val = BitConverter.ToInt32(m_fileData, m_internalPointer);
            m_internalPointer += sizeof(Int32);
            return val;
        }

        /// <summary>
        /// Reads the next UInt16 from the file data
        /// </summary>
        /// <returns></returns>
        public UInt16 ReadUInt16()
        {
            Debug.Assert(m_internalPointer <= m_fileLength - sizeof(UInt16));
            UInt16 val = BitConverter.ToUInt16(m_fileData, m_internalPointer);
            m_internalPointer += sizeof(UInt16);
            return val;
        }

        /// <summary>
        /// Reads the next byte from the file data
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            Debug.Assert(m_internalPointer <= m_fileLength - sizeof(byte));
            byte val = m_fileData[m_internalPointer];
            m_internalPointer++;
            return val;
        }

        /// <summary>
        /// Reads a byte array from the file data
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes(int length)
        {
            Debug.Assert(m_internalPointer <= m_fileLength - length);
            byte[] val = new byte[length];
            Array.Copy(m_fileData, m_internalPointer, val, 0, length);
            m_internalPointer += length;
            return val;
        }

        /// <summary>
        /// Gets all bytes that are remaining, does not advance the reading
        /// cursor.
        /// </summary>
        /// <returns>The bytes that are left</returns>
        public byte[] GetRemainingBytes()
        {
            int length = (int)m_fileLength - m_internalPointer;
            int pointerCopy = m_internalPointer;
            byte[] left = ReadBytes(length);
            m_internalPointer = pointerCopy;
            return left;
        }

        /// <summary>
        /// Reads the next Single from the file data
        /// </summary>
        /// <returns></returns>
        public Single ReadSingle()
        {
            Debug.Assert(m_internalPointer <= m_fileLength - sizeof(Single));
            Single val = BitConverter.ToSingle(m_fileData, m_internalPointer);
            m_internalPointer += sizeof(Single);
            return val;
        }

        /// <summary>
        /// skip the specified amount of bytes
        /// </summary>
        /// <param name="byteCount"></param>
        public void Skip(int byteCount)
        {
            Debug.Assert(m_internalPointer <= m_fileLength - byteCount);
            m_internalPointer += byteCount;
        }

        public void AlignToDwordBoundary()
        {
            int skipCount = m_internalPointer % 4;
            if (skipCount > 0)
            {
                Skip(4 - skipCount);
            }
        }

        public bool HasReachedEnd
        {
            get { return m_internalPointer >= m_fileLength; }
        }

        /// <summary>
        /// Get the ID of the file.
        /// </summary>
        public uint FileId
        {
            get { return m_fileId; }
        }

        /// <summary>
        /// Get the length of the file.
        /// </summary>
        public uint FileLength
        {
            get { return m_fileLength; }
        }

        /// <summary>
        /// Get the type of dat file that we're reading from
        /// </summary>
        public DatType DatType
        {
            get { return m_dataSource.DatType; }
        }

        /// <summary>
        /// Get the pointer to the start of the file.
        /// </summary>
        public uint StartFilePointer
        {
            get { return m_startFilePointer; }
        }

        /// <summary>
        /// Get the file data
        /// </summary>
        public byte[] FileData
        {
            get { return m_fileData; }
        }

        /// <summary>
        /// Current position of the reading pointer
        /// </summary>
        public int Position
        {
            get { return m_internalPointer; }
        }

        /// <summary>
        /// Internal pointer that determines from what position we're
        /// currently reading data from. Relative to the start of the file
        /// data.
        /// </summary>
        private int m_internalPointer;
        /// <summary>
        /// Start position of the file.
        /// </summary>
        private readonly uint m_startFilePointer;

        /// <summary>
        /// The dat reader that acts as the source of data for this file.
        /// </summary>
        private readonly DatReader m_dataSource;
        /// <summary>
        /// The ID of the file
        /// </summary>
        private uint m_fileId;
        /// <summary>
        /// The length of the file.
        /// </summary>
        private readonly uint m_fileLength;
        /// <summary>
        /// internal data of the file.
        /// </summary>
        private byte[] m_fileData;

        public uint[] m_debugData;
        public ushort[] m_debugshorts;
        public float[] m_debugDataFloats;

        public uint[] DebugDataNext
        {
            get
            {
                uint length = (m_fileLength - (uint)m_internalPointer) / 4;
                if (length > 50)
                {
                    length = 50;
                }

                uint[] data = new uint[length];

                for (uint i = 0; i < length; i++)
                {
                    data[i] = BitConverter.ToUInt32(m_fileData, m_internalPointer + ((int)i * 4));
                }
                return data;
            }
        }

        public ushort[] DebugDataNextShorts
        {
            get
            {
                uint length = (m_fileLength - (uint)m_internalPointer) / 2;
                if (length > 1000)
                {
                    length = 1000;
                }

                ushort[] data = new ushort[length];

                for (uint i = 0; i < length; i++)
                {
                    data[i] = BitConverter.ToUInt16(m_fileData, m_internalPointer + ((int)i * 2));
                }
                return data;
            }
        }

        public int UintPointer
        {
            get { return m_internalPointer / 4; }
        }
    }
}

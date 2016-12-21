using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    public struct EmbeddedFileEntry : IComparable
    {
        public EmbeddedFileEntry(uint size, uint fileId)
        {
            Size = size;
            FileID = fileId;
        }

        public override string ToString()
        {
            return String.Format("0x{0:X8} ({1} bytes)", FileID, Size);
        }

        public readonly uint Size;
        public readonly uint FileID;

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return FileID.CompareTo(((EmbeddedFileEntry) obj).FileID);
        }

        #endregion
    }
}

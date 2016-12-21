using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    public abstract class BP
    {
        public BP(int uintIndex)
        {
            Debug_UintIndex = uintIndex;
        }

        public uint? TrifanRefCount;
        public ushort[] TrifanRefs;

        /// <summary>
        /// uint index at which the structure started
        /// </summary>
        public int Debug_UintIndex;

        public override string ToString()
        {
            string text = base.ToString();
            if (text.IndexOf('.') >= 0)
            {
                text = text.Substring(text.LastIndexOf('.') + 1);
            }
            return text + String.Format(" @ 0x{0:X8} ({0})", Debug_UintIndex);
        }
    }

    public class BPnN : BP
    {
        public BPnN(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;
        public BP Unk_3;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;
    }

    public class BPnn : BP
    {
        public BPnn(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;
    }

    //public class BPLF : BP
    //{
    //    public BPLF(int uintIndex)
    //        : base(uintIndex)
    //    {
    //    }

    //    public float Unk_1a;
    //    public float Unk_1b;
    //    public float Unk_1c;
    //    public float Unk_1d;

    //    public BP Unk_2;
    //}

    public class BPFL : BP
    {
        public BPFL(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;
        public TrifanInfo[] Trifans;
        public uint Unk_3;
        public BP Unk_4;
    }

    public class BPIN : BP
    {
        public BPIN(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;
        public BP Unk_3;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;
    }

    public class BPIn : BP
    {
        public BPIn(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;
    }

    public class BpIN : BP
    {
        public BpIN(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;
    }

    public class BpIn : BP
    {
        public BpIn(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;
    }

    public class BPOL : BP
    {
        public BPOL(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;
    }

    public class LEAF : BP
    {
        public LEAF(int uintIndex)
            : base(uintIndex)
        {
        }

        public uint Unk_1;
        public float Unk_2a;
        public float Unk_2b;
        public float Unk_2c;
        public float Unk_2d;
    }

    public class PORT : BP
    {
        public PORT(int uintIndex)
            : base(uintIndex)
        {
        }

        public float Unk_1a;
        public float Unk_1b;
        public float Unk_1c;
        public float Unk_1d;

        public BP Unk_2;
        public BP Unk_3;

        public float Unk_4a;
        public float Unk_4b;
        public float Unk_4c;
        public float Unk_4d;

        public uint Unk_5_count;
        public ushort[] Unk_5;
        public uint Unk_6_count;
        public ushort[][] Unk_6;
    }
}

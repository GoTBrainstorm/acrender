using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    public class Util
    {
        public static void UIntToBArr(byte[] array, int index, uint value)
        {
            Array.Copy(BitConverter.GetBytes(value), 0, array, index, sizeof(uint));
        }

        public static uint[] BArrToUintArr(byte[] barr)
        {
            uint[] vals = new uint[barr.Length / 4];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = BitConverter.ToUInt32(barr, i * 4);
            }
            return vals;
        }
    }
}

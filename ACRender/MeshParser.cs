using System;
using System.Diagnostics;

namespace Gawa.ACRender
{
    public enum TrifanType
    {
        T2, T1, T0   
    }

    public class MeshParser
    {
        public MeshParser(EmbeddedFile sourceFile)
        {
            m_sourceFile = sourceFile;
        }

        public TrifanSet ParseTrifanSet(bool doTaggedRecord, uint trifanCount, TrifanType triType)
        {
            return ParseTrifanSet(doTaggedRecord, trifanCount, triType, null);
        }

        public TrifanSet ParseTrifanSet(bool doTaggedRecord, uint trifanCount, TrifanType triType, int? dungeonUnk3)
        {
            TrifanSet set = new TrifanSet();

            // trifan count & data
            set.TrifanCount = trifanCount;
            set.TrifanData = new TrifanInfo[set.TrifanCount];

            //int hasFlagFourCount = 0; // amount of trifans with flag bit 0x04 set
            for (int i = 0; i < set.TrifanCount; i++)
            {
                set.TrifanData[i] = ParseTrifanInfo();
                //if ((set.TrifanData[i].Flags & 0x04) == 0x04)
                //{
                //    hasFlagFourCount++;
                //}
            }

            if (m_sourceFile.Position % 4 != 0)
            {
                //Debugger.Break();
            }

            if (m_sourceFile.DatType != DatType.Portal_ToD)
            {
                m_sourceFile.AlignToDwordBoundary();
            }

            if (dungeonUnk3.HasValue)
            {
                int skipCount = dungeonUnk3.Value * sizeof (UInt16);
                m_sourceFile.Skip(skipCount);
                m_sourceFile.AlignToDwordBoundary();
            }

            //ushort[] UnkShorts = new ushort[hasFlagFourCount];
            //if (hasFlagFourCount > 0)
            //{
            //    for (int i = 0; i < UnkShorts.Length; i++)
            //    {
            //        UnkShorts[i] = m_sourceFile.ReadUInt16();
            //    }
            //}
            //else
            //{
            //    uint unkUint32 = m_sourceFile.ReadUInt32();      // should be 0x00
            //    if (unkUint32 != 0x00)
            //    {
            //        Debugger.Break();
            //    }
            //}


            if (doTaggedRecord)
            {
                set.BP = ParseTaggedRecord(triType);
            }
            return set;
        }

        public TrifanInfo ParseTrifanInfo()
        {
            TrifanInfo trifan = new TrifanInfo();
            trifan.TrifanIndex = m_sourceFile.ReadUInt16();
            trifan.VertexCount = m_sourceFile.ReadByte();
            trifan.Flags = m_sourceFile.ReadByte();
            trifan.Unknown4 = m_sourceFile.ReadUInt32();
            trifan.Unknown5 = m_sourceFile.ReadUInt32();

            //bool isSolidColor = (set.TrifanData[i].Flags & 0x04) == 0x04;
            //if (isSolidColor)
            //{
            //    set.TrifanData[i].TextureIndex = (set.TrifanData[i].Unknown4 & 0xFFFF);
            //}
            //else
            //{
            //    set.TrifanData[i].TextureIndex = (set.TrifanData[i].Unknown5 & 0xFFFF);
            //}
            //set.TrifanData[i].UVDepth = (ushort)(set.TrifanData[i].Unknown4 & 0xFFFF);

            //if (set.TrifanData[i].Flags != 0x04)        // if flag 0x04 is not set
            //{
            trifan.TextureIndex = (trifan.Unknown5 & 0xFFFF);
            //}
            //else
            //{
            //    set.TrifanData[i].TextureIndex = UInt32.MaxValue;   // set to invalid value
            //}
            trifan.UVDepth = (ushort)(trifan.Unknown4 & 0xFFFF);

            if (trifan.UVDepth == 0)
            {
                trifan.UVDepth = 1;
            }

            trifan.VertexIndices = new ushort[trifan.VertexCount];
            for (int j = 0; j < trifan.VertexCount; j++)
            {
                trifan.VertexIndices[j] = m_sourceFile.ReadUInt16();
            }

            if (trifan.Flags != 0x04)
            {
                for (int d = 0; d < trifan.UVDepth; d++)
                {
                    trifan.UVIndex = m_sourceFile.ReadBytes(trifan.VertexCount);
                }
            }

            if (m_sourceFile.DatType != DatType.Portal_ToD)
            {
                m_sourceFile.AlignToDwordBoundary();
            }
            return trifan;
        }


        // node list:
        //
        // 0x4250464C   BPFL
        // 0x4250494E   BPIN
        // 0x4250496E   BPIn
        // 0x42504F4C   BPOL
        // 0x42506E4E   BPnN
        // 0x42506E6E   BPnn
        // 0x4270494E   BpIN
        // 0x4270496E   BpIn
        // 0x4C454146   LEAF
        // 0x504f5254   PORT

        public BP ParseTaggedRecord(TrifanType triType)
        {
            int num;
            //depth++;
            uint bpType = m_sourceFile.ReadUInt32();
            switch (bpType)
            {
                case 0x42506e4e:
                case 0x4250494e:
                    m_sourceFile.Skip(16);
                    ParseTaggedRecord(triType);
                    ParseTaggedRecord(triType);
                    break;

                case 0x42506e6e:
                case 0x4250496e:
                case 0x4270494e:
                case 0x42706e4e:
                    m_sourceFile.Skip(16);
                    ParseTaggedRecord(triType);
                    break;

                case 0x4c454146:
                    uint unkFlag = m_sourceFile.ReadUInt32();
                    if (triType != TrifanType.T1)
                    {
                        return null;
                    }
                    m_sourceFile.Skip(20);
                    num = m_sourceFile.ReadInt32();
                    m_sourceFile.Skip(num * 2);

                    if (m_sourceFile.DatType == DatType.Portal_ToD)
                    {
                        //if (unkFlag == 0x01)
                        //{
                        //    m_sourceFile.Skip(4 * sizeof(uint));
                        //}
                    }
                    else
                    {
                        m_sourceFile.AlignToDwordBoundary();
                    }
                    return null;

                case 0x504f5254:
                    {
                        m_sourceFile.Skip(16);
                        ParseTaggedRecord(triType);
                        ParseTaggedRecord(triType);
                        //if ((triType == TrifanType.T1) || (triType == TrifanType.T2))
                        {
                            m_sourceFile.Skip(16);
                        }
                        num = m_sourceFile.ReadInt32();
                        int num2 = m_sourceFile.ReadInt32();
                        m_sourceFile.Skip((num * 2) + (num2 * 4));
                        m_sourceFile.AlignToDwordBoundary();
                        return null;
                    }
                case 0x42504f4c:
                case 0x4270496e:
                case 0x4250464c:
                    {
                        m_sourceFile.Skip(16);
                        break;
                    }
                default:
                    {
                        m_sourceFile.Skip(16);
                        break;
                    }
            }
            if ((triType == TrifanType.T0) || (triType == TrifanType.T1))
            {
                m_sourceFile.Skip(16);
            }
            if (triType == TrifanType.T0)
            {
                num = m_sourceFile.ReadInt32();
                m_sourceFile.Skip(num * 2);
            }
            if (m_sourceFile.DatType != DatType.Portal_ToD)
            {
                m_sourceFile.AlignToDwordBoundary();
            }
            return null;
        }

        /*private BP ParseTaggedRecordX(ref uint irec, bool trailer, bool parseLeaves, bool extendPort,
            int flTrifanDepth)
        {
            int uintIndex = m_sourceFile.UintPointer;
            uint tag = m_sourceFile.ReadUInt32();
            switch (tag)
            {
                case 0x42506E6E:        // BPnn
                    BPnn BPnn = new BPnn(uintIndex);

                    BPnn.Unk_1a = m_sourceFile.ReadSingle();
                    BPnn.Unk_1b = m_sourceFile.ReadSingle();
                    BPnn.Unk_1c = m_sourceFile.ReadSingle();
                    BPnn.Unk_1d = m_sourceFile.ReadSingle();

                    BPnn.Unk_2 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);

                    return BPnn;
                //case 0x42504C46:        // BPLF
                //    BPLF BPLF = new BPLF(uintIndex);

                //    BPLF.Unk_1a = SourceFile.ReadSingle();
                //    BPLF.Unk_1b = SourceFile.ReadSingle();
                //    BPLF.Unk_1c = SourceFile.ReadSingle();
                //    BPLF.Unk_1d = SourceFile.ReadSingle();

                //    //BPLF.Unk_2 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort);

                //    return BPLF;
                case 0x4250464C:        // BPFL
                    BPFL BPFL = new BPFL(uintIndex);

                    BPFL.Unk_1a = m_sourceFile.ReadSingle();
                    BPFL.Unk_1b = m_sourceFile.ReadSingle();
                    BPFL.Unk_1c = m_sourceFile.ReadSingle();
                    BPFL.Unk_1d = m_sourceFile.ReadSingle();

                    BPFL.Trifans = new TrifanInfo[flTrifanDepth];
                    for (int i = 0; i < BPFL.Trifans.Length; i++)
                    {
                        BPFL.Trifans[i] = ParseTrifanInfo();
                    }
                    BPFL.Unk_2 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);
                    BPFL.Unk_3 = m_sourceFile.ReadUInt32();
                    BPFL.Unk_4 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);

                    return BPFL;
                case 0x42506E4E:        // BPnN
                    BPnN BPnN = new BPnN(uintIndex);
                    BPnN.Unk_1a = m_sourceFile.ReadSingle();
                    BPnN.Unk_1b = m_sourceFile.ReadSingle();
                    BPnN.Unk_1c = m_sourceFile.ReadSingle();
                    BPnN.Unk_1d = m_sourceFile.ReadSingle();
                    BPnN.Unk_2 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);
                    BPnN.Unk_3 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);
                    BPnN.Unk_4a = m_sourceFile.ReadSingle();
                    BPnN.Unk_4b = m_sourceFile.ReadSingle();
                    BPnN.Unk_4c = m_sourceFile.ReadSingle();
                    BPnN.Unk_4d = m_sourceFile.ReadSingle();
                    ReadTrailer(BPnN, trailer);
                    return BPnN;
                case 0x4250494E:        // BPIN
                    BPIN BPIN = new BPIN(uintIndex);
                    BPIN.Unk_1a = m_sourceFile.ReadSingle();
                    BPIN.Unk_1b = m_sourceFile.ReadSingle();
                    BPIN.Unk_1c = m_sourceFile.ReadSingle();
                    BPIN.Unk_1d = m_sourceFile.ReadSingle();
                    BPIN.Unk_2 = ParseTaggedRecord(ref irec, trailer, false, extendPort, flTrifanDepth);
                    BPIN.Unk_3 = ParseTaggedRecord(ref irec, trailer, false, extendPort, flTrifanDepth);
                    BPIN.Unk_4a = m_sourceFile.ReadSingle();
                    BPIN.Unk_4b = m_sourceFile.ReadSingle();
                    BPIN.Unk_4c = m_sourceFile.ReadSingle();
                    BPIN.Unk_4d = m_sourceFile.ReadSingle();
                    ReadTrailer(BPIN, trailer);
                    return BPIN;
                case 0x4C454146:        // LEAF
                    uint irecFound = m_sourceFile.ReadUInt32();
                    Debug.Assert(irecFound == irec);
                    irec++;
                    LEAF leaf = new LEAF(uintIndex);
                    //bool doRest = parseLeaves;//true;
                    byte[] leftB = m_sourceFile.GetRemainingBytes();

                    if (!trailer)
                    {
                        leaf.Unk_1 = m_sourceFile.ReadUInt32();
                        leaf.Unk_2a = m_sourceFile.ReadSingle();
                        leaf.Unk_2b = m_sourceFile.ReadSingle();
                        leaf.Unk_2c = m_sourceFile.ReadSingle();
                        leaf.Unk_2d = m_sourceFile.ReadSingle();
                    }
                    if (parseLeaves)
                    {
                        ReadTrailer(leaf, true);
                    }
                    return leaf;
                case 0x4250496E:        // BPIn
                    BPIn BPIn = new BPIn(uintIndex);
                    BPIn.Unk_1a = m_sourceFile.ReadSingle();
                    BPIn.Unk_1b = m_sourceFile.ReadSingle();
                    BPIn.Unk_1c = m_sourceFile.ReadSingle();
                    BPIn.Unk_1d = m_sourceFile.ReadSingle();
                    BPIn.Unk_2 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);
                    BPIn.Unk_4a = m_sourceFile.ReadSingle();
                    BPIn.Unk_4b = m_sourceFile.ReadSingle();
                    BPIn.Unk_4c = m_sourceFile.ReadSingle();
                    BPIn.Unk_4d = m_sourceFile.ReadSingle();
                    ReadTrailer(BPIn, trailer);
                    return BPIn;
                case 0x4270494E:        // BpIN
                    BpIN BpIN = new BpIN(uintIndex);
                    BpIN.Unk_1a = m_sourceFile.ReadSingle();
                    BpIN.Unk_1b = m_sourceFile.ReadSingle();
                    BpIN.Unk_1c = m_sourceFile.ReadSingle();
                    BpIN.Unk_1d = m_sourceFile.ReadSingle();
                    BpIN.Unk_2 = ParseTaggedRecord(ref irec, trailer, parseLeaves, extendPort, flTrifanDepth);
                    BpIN.Unk_4a = m_sourceFile.ReadSingle();
                    BpIN.Unk_4b = m_sourceFile.ReadSingle();
                    BpIN.Unk_4c = m_sourceFile.ReadSingle();
                    BpIN.Unk_4d = m_sourceFile.ReadSingle();
                    ReadTrailer(BpIN, trailer);
                    return BpIN;
                case 0x42504F4C:        // BPOL
                    BPOL BPOL = new BPOL(uintIndex);
                    BPOL.Unk_1a = m_sourceFile.ReadSingle();
                    BPOL.Unk_1b = m_sourceFile.ReadSingle();
                    BPOL.Unk_1c = m_sourceFile.ReadSingle();
                    BPOL.Unk_1d = m_sourceFile.ReadSingle();
                    BPOL.Unk_4a = m_sourceFile.ReadSingle();
                    BPOL.Unk_4b = m_sourceFile.ReadSingle();
                    BPOL.Unk_4c = m_sourceFile.ReadSingle();
                    BPOL.Unk_4d = m_sourceFile.ReadSingle();
                    ReadTrailer(BPOL, trailer);
                    return BPOL;
                case 0x4270496E:        // BpIn
                    BpIn BpIn = new BpIn(uintIndex);
                    BpIn.Unk_1a = m_sourceFile.ReadSingle();
                    BpIn.Unk_1b = m_sourceFile.ReadSingle();
                    BpIn.Unk_1c = m_sourceFile.ReadSingle();
                    BpIn.Unk_1d = m_sourceFile.ReadSingle();
                    BpIn.Unk_4a = m_sourceFile.ReadSingle();
                    BpIn.Unk_4b = m_sourceFile.ReadSingle();
                    BpIn.Unk_4c = m_sourceFile.ReadSingle();
                    BpIn.Unk_4d = m_sourceFile.ReadSingle();
                    ReadTrailer(BpIn, trailer);
                    return BpIn;
                case 0x504f5254:        // PORT
                    PORT PORT = new PORT(uintIndex);
                    PORT.Unk_1a = m_sourceFile.ReadSingle();
                    PORT.Unk_1b = m_sourceFile.ReadSingle();
                    PORT.Unk_1c = m_sourceFile.ReadSingle();
                    PORT.Unk_1d = m_sourceFile.ReadSingle();
                    PORT.Unk_2 = ParseTaggedRecord(ref irec, trailer, false, extendPort, flTrifanDepth);
                    PORT.Unk_3 = ParseTaggedRecord(ref irec, trailer, false, extendPort, flTrifanDepth);
                    PORT.Unk_4a = m_sourceFile.ReadSingle();
                    PORT.Unk_4b = m_sourceFile.ReadSingle();
                    PORT.Unk_4c = m_sourceFile.ReadSingle();
                    PORT.Unk_4d = m_sourceFile.ReadSingle();

                    PORT.Unk_5_count = m_sourceFile.ReadUInt32();
                    PORT.Unk_6_count = m_sourceFile.ReadUInt32();
                    PORT.Unk_5 = new ushort[PORT.Unk_5_count];
                    for (int k = 0; k < PORT.Unk_5_count; k++)
                    {
                        PORT.Unk_5[k] = m_sourceFile.ReadUInt16();
                    }
                    PORT.Unk_6 = new ushort[PORT.Unk_6_count][];
                    for (int k = 0; k < PORT.Unk_6_count; k++)
                    {
                        PORT.Unk_6[k] = new ushort[2];
                        PORT.Unk_6[k][0] = m_sourceFile.ReadUInt16();
                        PORT.Unk_6[k][1] = m_sourceFile.ReadUInt16();
                        if (PORT.Unk_6[k][0] != PORT.Unk_6[k][1])
                        {
                            //Debugger.Break();
                        }
                    }

                    m_sourceFile.AlignToDwordBoundary();

                    return PORT;
                default:
                    // copy remaining bytes for debug purposes
                    byte[] left = m_sourceFile.GetRemainingBytes();
                    throw new Exception("Unknown recursive instruction " + tag.ToString("X8"));
            }
        }*/

        public void ReadThreeFloats(out float Unk1, out float Unk2, out float Unk3)
        {
            Unk1 = m_sourceFile.ReadSingle();
            Unk2 = m_sourceFile.ReadSingle();
            Unk3 = m_sourceFile.ReadSingle();
        }

        private readonly EmbeddedFile m_sourceFile;
    }
}

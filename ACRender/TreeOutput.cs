using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gawa.ACRender
{
    public class TreeOutput
    {
        public static void OutputTree(string directory, SimpleMesh mesh)
        {
            OutputTree(new DirectoryInfo(directory), mesh);
        }

        public static void OutputTree(DirectoryInfo directory, SimpleMesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            FileInfo file = new FileInfo(directory.FullName + "\\tree_" + mesh.ID.ToString("X8") + ".txt");
            using (FileStream stream = file.OpenWrite())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    if (mesh.HasFirstTrifanSet)
                    {
                        writer.WriteLine("First trifan set");
                        OutputNode(0, mesh.FirstTrifanSet.BP, writer);
                        writer.WriteLine(writer.NewLine);
                    }
                    if (mesh.HasSecondTrifanSet)
                    {
                        writer.WriteLine("Second trifan set");
                        OutputNode(0, mesh.SecondTrifanSet.BP, writer);
                    }
                }
            }
        }

        public static void OutputTree(DirectoryInfo directory, DungeonBlock block,
            int bytesLeft, uint[] data)
        {
            if (block == null)
            {
                return;
            }

            FileInfo file = new FileInfo(directory.FullName + "\\tree_" + block.SourceFile.FileId.ToString("X8") + ".txt");
            using (FileStream stream = file.OpenWrite())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("First trifan set");
                    OutputNode(0, block.T2TrifanSet.BP, writer);
                    writer.WriteLine(writer.NewLine);
                    writer.WriteLine("Bytes left: " + bytesLeft);
                }
            }
        }

        private static void OutputNode(int depth, BP node, StreamWriter writer)
        {
            for (int i = 0; i < depth; i++)
            {
                writer.Write("\t");
            }
            writer.Write("- ");

            Type nodeType = node.GetType();
            // 0x4250464C   BPFL
            // 0x4250494E   BPIN  done
            // 0x4250496E   BPIn  done
            // 0x42504F4C   BPOL  done
            // 0x42506E4E   BPnN  done
            // 0x42506E6E   BPnn  done
            // 0x4270494E   BpIN  done
            // 0x4270496E   BpIn  done
            // 0x4C454146   LEAF  done
            // 0x504f5254   PORT  done
            if (nodeType == typeof(BPnN))
            {
                writer.Write("BPnN");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((BPnN)node).Unk_2, writer);
                OutputNode(depth + 1, ((BPnN)node).Unk_3, writer);
            }
            else if (nodeType == typeof(BPIN))
            {
                writer.Write("BPIN");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((BPIN)node).Unk_2, writer);
                OutputNode(depth + 1, ((BPIN)node).Unk_3, writer);
            }
            else if (nodeType == typeof(BPFL))
            {
                writer.Write("BPFL");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((BPFL)node).Unk_2, writer);
                OutputNode(depth + 1, ((BPFL)node).Unk_4, writer);
            }
            else if (nodeType == typeof(BPIn))
            {
                writer.Write("BPIn");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((BPIn)node).Unk_2, writer);
            }
            else if (nodeType == typeof(BpIN))
            {
                writer.Write("BpIN");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((BpIN)node).Unk_2, writer);
            }
            else if (nodeType == typeof(BPnn))
            {
                writer.Write("BPnn");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((BPnn)node).Unk_2, writer);
            }
            else if (nodeType == typeof(BpIn))
            {
                writer.Write("BpIn");
                WriteTrailer(node, writer);
            }
            else if (nodeType == typeof(BPOL))
            {
                writer.Write("BPOL");
                WriteTrailer(node, writer);
            }
            else if (nodeType == typeof(LEAF))
            {
                writer.Write("LEAF");
                WriteTrailer(node, writer);
            }
            else if (nodeType == typeof(PORT))
            {
                writer.Write("PORT");
                WriteTrailer(node, writer);
                OutputNode(depth + 1, ((PORT)node).Unk_2, writer);
                OutputNode(depth + 1, ((PORT)node).Unk_3, writer);
            }
            else
            {
                writer.Write("UNKNOWN NODE TYPE " + nodeType.ToString());
            }
        }

        private static void WriteTrailer(BP node, StreamWriter writer)
        {
            if (node.TrifanRefCount.HasValue)
            {
                writer.Write(" (trailer: [");
                for (int i = 0; i < node.TrifanRefs.Length; i++)
                {
                    if (i != 0)
                    {
                        writer.Write(", ");
                    }
                    writer.Write("0x{0:X}", node.TrifanRefs[i]);
                }
                writer.Write("]");
            }
            else
            {
                writer.Write(" (no trailer)");
            }

            writer.Write(writer.NewLine);
        }
    }
}

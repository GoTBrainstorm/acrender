using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Gawa.ACRender
{
    /// <summary>
    /// Class for debug exporting files
    /// </summary>
    public class FileExporter
    {
        #region simple meshes
        public static void TestSimpleMeshes()
        {
            DatReader reader = DataProvider.Instance.PortalDatReader;

            TrySimpleMesh(reader, 0x01000848);

            List<EmbeddedFileEntry> fileList = reader.GetFileIDs(0x01000000, 0x01FFFFFF);
            fileList.Sort();
            uint minS = UInt32.MaxValue, minF = 0;
            foreach (EmbeddedFileEntry fileEntry in fileList)
            {
                try
                {
                    TrySimpleMesh(reader, fileEntry.FileID);
                }
                catch (Exception ex)
                {
                }
                if (fileEntry.Size < minS)
                {
                    minS = fileEntry.Size;
                    minF = fileEntry.FileID;
                }
                //Debugger.Break();
                //Console.WriteLine("Failed on object 0x{0:X8}, error: {1}", fileEntry.FileID, ex.Message);
            }

            //fileList = reader.GetFileIDs(0x08000000, 0x08FFFFFF);
            //fileList.Sort();
            //foreach (EmbeddedFileEntry fileEntry in fileList)
            //{
            //    CheckTextureInfo(reader, fileEntry.FileID);
            //}
        }

        private static void TrySimpleMesh(DatReader reader, uint fileId)
        {
            EmbeddedFile file = reader.LocateFile(fileId);
            if (file == null)
            {
                throw new Exception("Unable to locate file");
            }
            SimpleMesh mesh = new SimpleMesh(file);
            mesh.ReadSimpleMesh();
        }
        #endregion

        #region complex meshes
        public static void TestComplexMeshes()
        {
            DatReader reader = DataProvider.Instance.PortalDatReader;

            //EmbeddedFile file = reader.LocateFile(0x0200001e);
            EmbeddedFile file = reader.LocateFile(0x020000A1);
            //EmbeddedFile file = reader.LocateFile(0x0200012c);
            if (file == null)
            {
                Console.WriteLine("File not found.");
            }
            else
            {
                Console.WriteLine("File found, has length of {0}.", file.FileLength);

                // dump the file data

                byte[] data = reader.RetrieveFileData(file);
                Dump(data, "mesh 0x" + file.FileId.ToString("X8"));

                ComplexMesh mesh = new ComplexMesh(file);
                mesh.ReadComplexMesh();

                //TreeOutput.OutputTree(@"D:\", mesh, mesh.ID);

                List<EmbeddedFileEntry> fileList = MeshProvider.Instance.GetComplexMeshList();
                fileList.Sort();
                uint minS = UInt32.MinValue, minF = 0;
                List<CMDBGData> dbgList = new List<CMDBGData>();
                foreach (EmbeddedFileEntry fileEntry in fileList)
                {
                    try
                    {
                        if (fileEntry.Size == 164)
                        {
                            TryComplexMesh(reader, fileEntry.FileID);
                            //Debugger.Break();
                        }
                        if (fileEntry.Size > minS)
                        {
                            minS = fileEntry.Size;
                            minF = fileEntry.FileID;
                        }
                        ComplexMesh listMesh = TryComplexMesh(reader, fileEntry.FileID);
                        dbgList.Add(new CMDBGData(fileEntry.FileID, fileEntry.Size, listMesh.Flags, listMesh.SimpleMeshCount));
                    }
                    catch (Exception)
                    {
                        //Debugger.Break();
                        //Console.WriteLine("Failed on object 0x{0:X8}, error: {1}", fileEntry.FileID, ex.Message);
                    }
                }
                dbgList.Sort();

                FileInfo exportFile = new FileInfo(Settings.TreeExportDir + "\\complex_listing.txt");
                if (exportFile.Exists)
                {
                    exportFile.Delete();
                }
                using (FileStream stream = exportFile.OpenWrite())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(String.Format("Flag\tCount\tSize\tID"));
                        foreach (CMDBGData dbgData in dbgList)
                        {
                            writer.WriteLine(String.Format("{1:X1}\t{3:000}\t{4:000}\t{2:X8}", writer.NewLine, dbgData.Flags, dbgData.ID, dbgData.SimpleCount, dbgData.Size));
                        }
                    }
                }

                //Debugger.Break();
            }
        }

        class CMDBGData : IComparable<CMDBGData>
        {
            public CMDBGData(uint id, uint size, uint flags, uint simpleCount)
            {
                ID = id;
                Size = size;
                Flags = flags;
                SimpleCount = simpleCount;
            }

            public uint ID;
            public uint Size;
            public uint Flags;
            public uint SimpleCount;

            public int CompareTo(CMDBGData other)
            {
                if (Flags != other.Flags)
                {
                    return Flags.CompareTo(other.Flags);
                }
                if (SimpleCount != other.SimpleCount)
                {
                    return SimpleCount.CompareTo(other.SimpleCount);
                }
                if (Size != other.Size)
                {
                    return Size.CompareTo(other.Size);
                }
                return ID.CompareTo(other.ID);
            }
        }

        private static ComplexMesh TryComplexMesh(DatReader reader, uint fileId)
        {
            EmbeddedFile file = reader.LocateFile(fileId);
            if (file == null)
            {
                throw new Exception("Unable to locate file");
            }
            ComplexMesh mesh = new ComplexMesh(file);
            mesh.ReadComplexMesh();
            return mesh;
        }
        #endregion

        #region Dungeons
        public static void TestDungeons()
        {
            // 01380100
            Dungeon dungeon = DungeonProvider.Instance.GetDungeon(0x0138);

            List<ushort> dungeons = DungeonProvider.Instance.GetDungeonList();
            foreach (ushort dungeonId in dungeons)
            {
                DungeonProvider.Instance.GetDungeon(dungeonId);
            }

            //List<EmbeddedFileEntry> blocks = DungeonProvider.Instance.GetDungeonBlockList();
            //uint s = UInt32.MaxValue, f = 0;
            //foreach (EmbeddedFileEntry entry in blocks)
            //{
            //    DungeonBlock block = DungeonProvider.Instance.GetDungeonBlock(entry.FileID);
            //    if (entry.Size < s)
            //    {
            //        f = entry.FileID;
            //        s = entry.Size;
            //    }
            //}

            //DungeonBlock block = DungeonProvider.Instance.GetDungeonBlock(f);
            //Debugger.Break();
        }
        #endregion

        public static void TestDungeonBlocks()
        {
            List<EmbeddedFileEntry> fileList = DungeonProvider.Instance.GetDungeonBlockList();
            fileList.Sort();

            DungeonBlock test = DungeonProvider.Instance.GetDungeonBlock(0x0d000025);


            foreach (EmbeddedFileEntry fileEntry in fileList)
            {
                DungeonBlock block = DungeonProvider.Instance.GetDungeonBlock(fileEntry.FileID);
                //try
                //{
                    //block.ReadDungeonBlock();
                //}
                //catch (Exception)
                //{
                //    bool retry = false;
                //    System.Diagnostics.Debugger.Break();
                //    if (retry)
                //    {
                //        try
                //        {
                //            block.ReadDungeonBlock();
                //        }
                //        catch (Exception)
                //        {
                //            System.Diagnostics.Debugger.Break();
                //        }
                //    }
                //}
            }
        }

        public static void TestTextures()
        {
            List<EmbeddedFileEntry> textures = TextureLoader.Instance.GetTextureList();
            foreach (EmbeddedFileEntry fileEntry in textures)
            {
                TextureLoader.Instance.LoadTexture(fileEntry.FileID);
            }
        }

        #region Other methods
        public static void Dump(byte[] data, string identifier)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i % 28 == 0 && i != 0)
                {
                    sb.Append("\r\n");
                }
                else if (i % 2 == 0 && i != 0)
                {
                    sb.Append(" ");
                }
                sb.Append(data[i].ToString("X2"));
            }

            string hexValues = sb.ToString();
            Console.WriteLine(String.Format("=== BEGIN OF FILE DUMP '{0}' ===", identifier));
            Console.WriteLine(hexValues);
            Console.WriteLine("===  END OF FILE DUMP  ===");
        }

        private static void CheckTextureInfo(DatReader reader, uint id)
        {
            EmbeddedFile textureFile = reader.LocateFile(id);
            Debug.Assert((id & 0xFF000000) == 0x08000000 && textureFile != null);

            try
            {
                textureFile.PrepareFileForReading();

                // first dword is the ID
                uint textureId = textureFile.ReadUInt32();
                Debug.Assert(textureId == id);

                uint textureType = textureFile.ReadUInt32();
                switch (textureType)
                {
                    case 0x01:
                        uint unk01_01 = textureFile.ReadUInt32();
                        uint unk01_02 = textureFile.ReadUInt32();
                        Debug.Assert(unk01_02 == 0x00);
                        break;
                    default:
                        return;
                    //case 0x02:
                    //    uint textureNumber02 = textureFile.ReadUInt32();
                    //    uint paletteNumber02 = textureFile.ReadUInt32();
                    //    uint unk02_01 = textureFile.ReadUInt32();
                    //    Debug.Assert((textureNumber02 & 0xFF000000) == 0x05000000);
                    //    Debug.Assert(paletteNumber02 == 0x00 || (paletteNumber02 & 0xF000000) == 0x04000000);
                    //    Debug.Assert(unk02_01 == 0x00);
                    //    break;
                    //case 0x04:
                    //    uint textureNumber04 = textureFile.ReadUInt32();
                    //    uint paletteNumber04 = textureFile.ReadUInt32();
                    //    uint unk04_01 = textureFile.ReadUInt32();
                    //    Debug.Assert((textureNumber04 & 0xFF000000) == 0x05000000);
                    //    Debug.Assert(paletteNumber04 == 0x00 || (paletteNumber04 & 0xFF000000) == 0x04000000);
                    //    Debug.Assert(unk04_01 == 0x00);
                    //    break;
                    //case 0x11:
                    //    uint unk11_01 = textureFile.ReadUInt32();
                    //    uint unk11_02 = textureFile.ReadUInt32();
                    //    //Debug.Assert(unk11_02 == 0x3F800000);
                    //    break;
                }

                // read closing bytes..
                uint unknown1 = textureFile.ReadUInt32();
                uint unknown2 = textureFile.ReadUInt32();
                Debug.Assert(unknown1 == 0x00 || unknown1 == 0x3F800000);
                Debug.Assert(unknown2 == 0x3F800000);
            }
            finally
            {
                textureFile.FileReadingComplete();
            }
        }
        #endregion
    }
}

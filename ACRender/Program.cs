using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gawa.ACRender
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int choise = 8;
            switch (choise)
            {
                case 1:
                    Application.Run(new RenderForm());
                    break;
                case 2:
                    FileExporter.TestSimpleMeshes();
                    break;
                case 3:
                    FileExporter.TestComplexMeshes();
                    break;
                case 4:
                    List<EmbeddedFileEntry> list = MeshProvider.Instance.GetComplexMeshListBySimpleMeshID(0x010002c8);
                    System.Diagnostics.Debugger.Break();
                    break;
                case 5:
                    FileExporter.TestDungeons();
                    break;
                case 6:         // ToD compatible
                    FileExporter.TestDungeonBlocks();
                    break;
                case 7:
                    DynamicParser.DynamicFileParser parser = new Gawa.ACRender.DynamicParser.DynamicFileParser();
                    if (!parser.RefreshTypeMappings())
                    {
                        throw new Exception("Failed to refresh type mappings.");
                    }
                    break;
                case 8:
                    Application.Run(new DynamicParser.FileDisplay());
                    break;
                case 9:
                    FileExporter.TestTextures();
                    break;
                case 10:
                    PortalTreeView.PortalTreeView view = new PortalTreeView.PortalTreeView();
                    Application.Run(view);
                    break;
            }
        }
    }
}

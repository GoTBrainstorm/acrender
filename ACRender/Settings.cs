using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gawa.ACRender
{
    public class Settings
    {
        
        public static FileInfo PortalDatFile = new FileInfo(@"E:\gawa\data\acrender\PORTAL_ACRENDER.DAT");
        public static DatType PortalDatType = DatType.Portal_ACDM;
        //public static FileInfo PortalDatFile = new FileInfo(@"E:\Program Files\Turbine\Asheron's Call - Throne of Destiny 2\client_portal.dat");
        //public static DatType PortalDatType = DatType.Portal_ToD;

        public static FileInfo CellDatFile = new FileInfo(@"E:\gawa\data\acrender\CELL.DAT");
        public static DatType CellDatType = DatType.Cell_ACDM;

        public static string TreeExportDir = @"E:\gawa\data\acrender\";
        public static string FileListCacheDir = @"E:\gawa\data\acrender\";


        /*
        public static FileInfo PortalDatFile = new FileInfo(@"E:\gawa\data\acrender\ToD\client_portal.dat");
        public static DatType PortalDatType = DatType.Portal_ToD;
        //public static FileInfo PortalDatFile = new FileInfo(@"E:\Program Files\Turbine\Asheron's Call - Throne of Destiny 2\client_portal.dat");
        //public static DatType PortalDatType = DatType.Portal_ToD;

        public static FileInfo CellDatFile = new FileInfo(@"E:\gawa\data\acrender\ToD\client_cell_1.dat");
        public static DatType CellDatType = DatType.Cell_ToD;
          
        public static string TreeExportDir = @"E:\gawa\data\acrender\ToD";
        public static string FileListCacheDir = @"E:\gawa\data\acrender\ToD\";
         */

    }
}

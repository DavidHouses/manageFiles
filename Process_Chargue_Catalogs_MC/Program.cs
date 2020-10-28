using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process_Chargue_Catalogs_MC
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Program begins" + "\nProcess Start Time: " + DateTime.Now.ToString("hh:mm:ss"));

                FilesRoutes objFiles = new FilesRoutes();
                List<FilesRoutes> lstFolders = new List<FilesRoutes>();

                objFiles.DownloadArchive();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.ReadLine();
            }
        }
    }
}

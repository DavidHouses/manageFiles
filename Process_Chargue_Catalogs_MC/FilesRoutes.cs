using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Renci.SshNet;
using System.Text.RegularExpressions;
using LinqToExcel;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using LinqToExcel.Domain;
using SpreadsheetLight;
using System.Data;
using System.Data.Entity.Core.Mapping;
using LinqToExcel.Extensions;
using System.Data.Entity.Validation;
using System.Globalization;

namespace Process_Chargue_Catalogs_MC
{
    public class FilesRoutes
    {
        static string Route = ConfigurationManager.AppSettings["FilesRoute"];
        //Datos del servidor sftp al que se subiran los archivos <.txt> generados
        static string UpServer = ConfigurationManager.AppSettings["UpServer"];    //Ruta
        static string UpServerRoute = ConfigurationManager.AppSettings["UpServerRoute"];    //Ruta
        static string UpServerUser = ConfigurationManager.AppSettings["UpServerUser"];  //User
        static string UpServerPass = ConfigurationManager.AppSettings["UpServerPass"];  //Password
        static int UpServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["UpServerPort"]);    //Ruta

        public void DownloadArchive()
        {
            try
            {
                string remoteDirectory = "/ftp/";
                string finalDir = Route;


                using (var sftp = new SftpClient(UpServer, UpServerUser, UpServerPass))
                {
                    Console.WriteLine("Connecting to " + UpServer + " as " + UpServerUser);
                    sftp.Connect();
                    Console.WriteLine("Connected!");
                    var fileFilter = sftp.ListDirectory(remoteDirectory).Where(x => !x.Name.StartsWith(".")).ToList();

                    foreach (var file in fileFilter)
                    {

                        string remoteFileName = file.Name;
                        Console.WriteLine("Download archive: " + file.Name);

                        var _tempFilePath = Path.Combine(ConfigurationManager.AppSettings["FilesRoute"], file.Name);

                        if (!Directory.Exists(ConfigurationManager.AppSettings["FilesRoute"]))
                            (new FileInfo(_tempFilePath)).Directory.Create();

                        using (Stream file1 = File.OpenWrite(_tempFilePath))
                        {
                            sftp.DownloadFile(file.FullName, file1);
                        }

                        var book = new ExcelQueryFactory(_tempFilePath);
                        //book.DatabaseEngine = DatabaseEngine.Ace;

                        InsertDATA(book, remoteFileName);
                    }

                }
            }
            catch (Exception ex)
            {

                var message = ex.Message;
            }
        }

        public void InsertDATA(ExcelQueryFactory book, string remoteFileName)
        {
            try
            {
                //List<Cat_Stations_M> Cat_Stations_M = new List<Cat_Stations_M>();

                using (BI_TableauEntities db = new BI_TableauEntities())
                {

                    int numeroLotes = 0;
                    db.Configuration.AutoDetectChangesEnabled = false;

                    switch (remoteFileName)
                    {
                        case "MC_CAT_Estaciones.xlsx":
                            var Cat_Stations_M = (from row in book.Worksheet("MC_CAT_Estaciones")
                                                  let item = new Cat_Stations_M
                                                  {
                                                      idStation = row[0].Cast<int>(),
                                                      STATION = row[1].Cast<string>(),
                                                      DESC_CITY = row[2].Cast<string>(),
                                                      DESC_AIRPORTS = row[3].Cast<string>(),
                                                      DESC_COUNTRY = row[4].Cast<string>(),
                                                      ID_ZONE = row[5].Cast<string>(),
                                                      HORA_Z = row[6].GetType() == typeof(string) || row[6].Cast<string>() == null || row[6].Cast<string>() == "" ? 0 : row[6].Cast<double>(),//row[6].Cast<string>() != null || row[6].Cast<string>()  != "" ? row[6].Cast<double>() : 0,
                                                      DIF_MEX = row[7].Cast<string>(),
                                                      NUM = row[8].GetType() == typeof(string) || row[8].Cast<string>() == null || row[8].Cast<string>() == "" ? 0 : row[8].Cast<double>(),//row[8].Cast<string>() != null || row[8].Cast<string>() != "" ? row[8].Cast<double>() : 0,
                                                      HRS = row[9].GetType() == typeof(string) || row[9].Cast<string>() == "00:00" ? 0 : row[9].Cast<double>(),
                                                      GHA = row[10].Cast<string>(),
                                                      DETALLE1 = row[11].Cast<string>(),
                                                      DETALLE2 = row[12].Cast<string>(),
                                                      DETALLE3 = row[13].Cast<string>(),
                                                      DETALLE4 = row[14].Cast<string>(),
                                                      DETALLE5 = row[15].Cast<string>(),
                                                      REGION1 = row[16].Cast<string>(),
                                                      REGION2 = row[17].Cast<string>()
                                                      //,CAMPOS_A_MODIFICAR = row[18].Cast<string>()
                                                  }
                                                  select item).ToList();
                            book.Dispose();


                            Console.WriteLine("\nTruncate table  [Cat_Stations_M]");
                            db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Cat_Stations_M]");
                            Console.WriteLine("\nDatabase Insertion Process Begins : " + "\nInserting Catalog: [Cat_Stations_M]");

                            foreach (var item in Cat_Stations_M)
                            {

                                Cat_Stations_M cat_em = new Cat_Stations_M
                                {
                                    idStation = item.idStation,
                                    STATION = item.STATION,
                                    DESC_CITY = item.DESC_CITY,
                                    DESC_AIRPORTS = item.DESC_AIRPORTS,
                                    DESC_COUNTRY = item.DESC_COUNTRY,
                                    ID_ZONE = item.ID_ZONE,
                                    HORA_Z = item.HORA_Z,
                                    DIF_MEX = item.DIF_MEX,
                                    NUM = item.NUM,
                                    HRS = item.HRS,
                                    GHA = item.GHA,
                                    DETALLE1 = item.DETALLE1,
                                    DETALLE2 = item.DETALLE2,
                                    DETALLE3 = item.DETALLE3,
                                    DETALLE4 = item.DETALLE4,
                                    DETALLE5 = item.DETALLE5,
                                    REGION1 = item.REGION1,
                                    REGION2 = item.REGION2
                                    ///,CAMPOS_A_MODIFICAR = item.CAMPOS_A_MODIFICAR
                                };
                                db.Cat_Stations_M.Add(cat_em);

                                numeroLotes++;

                                if (numeroLotes == 3800)
                                {
                                    db.SaveChanges();
                                    numeroLotes = 0;
                                }
                            }
                            db.SaveChanges();
                            numeroLotes = 0;

                            break;
                    }

                    Console.WriteLine("Number of inserted records: " + numeroLotes);

                    Console.WriteLine("Insertion Completed Successfully" + "\nProcess end time: " + DateTime.Now.ToString("hh:mm:ss"));


                }


            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("La entidad de tipo \"{0}\" en estado \"{1}\" tiene los siguientes errores de validación:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Propiedad: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }
    }
}

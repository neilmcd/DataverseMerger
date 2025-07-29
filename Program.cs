using System;
using System.Configuration;
using System.IO;
using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using CsvHelper;
using CsvHelper.Configuration;

namespace DataverseMerger
{
    
    // The CSV file columns
    public class MergeRecord
    {
        public Guid Master { get; set; }
        public Guid Child { get; set; }
    }

    internal class Program
    {
        private static void Main()
        {
            var url = ConfigurationManager.AppSettings["DataverseUrl"];
            var csvFilePath = ConfigurationManager.AppSettings["CsvFile"];
            var table = ConfigurationManager.AppSettings["DataverseTable"].ToLower();

            Console.WriteLine($"Starting Dataverse {table} merger...");

            //Validate CSV file path
            if (!File.Exists(csvFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: The file '{csvFilePath}' was not found.");
                Console.ResetColor();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            try
            {
                //Connect to Dataverse
                Console.WriteLine($"Connecting to Dataverse environment: {url}...");
                var connectionString = $"AuthType=OAuth;Url=https://{url};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto";
                var service = new CrmServiceClient(connectionString);

                if (!service.IsReady)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to connect to Dataverse. Error: {service.LastCrmError}");
                    Console.ResetColor();
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Successfully connected to Dataverse.");
                Console.ResetColor();

                //Read and process the CSV file
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var records = csv.GetRecords<MergeRecord>().ToList();
                    Console.WriteLine($"Found {records.Count} records in the CSV file.");

                    var successCount = 0;
                    var errorCount = 0;

                    foreach (var record in records)
                    {
                        try
                        {
                            Console.WriteLine($"\nProcessing record: Master GUID = {record.Master}, Child GUID = {record.Child}");

                            //Create the MergeRequest
                            var mergeRequest = new MergeRequest
                            {
                                Target = new EntityReference(table, record.Master), //master Guid from the CSV
                                SubordinateId = record.Child,//Chid guid from the CSV
                                UpdateContent = new Entity(table),
                                PerformParentingChecks = false
                            };

                            Console.WriteLine("Executing merge request...");

                            //Execute the request
                            service.Execute(mergeRequest);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Successfully merged Child ({record.Child}) into Master ({record.Master}).");
                            Console.ResetColor();
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error processing record (Master: {record.Master}, Child: {record.Child}): {ex.Message}");
                            Console.ResetColor();
                            errorCount++;
                        }
                    }
                    
                    Console.WriteLine("Complete.");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Successful merges: {successCount}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed merges: {errorCount}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}
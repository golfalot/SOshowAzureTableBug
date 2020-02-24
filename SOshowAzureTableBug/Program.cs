using System;
using System.Collections.Generic;

using LEGACY_STORAGE = Microsoft.WindowsAzure.Storage;
using LEGACY_RETRY = Microsoft.WindowsAzure.Storage.RetryPolicies;
using LEGACY_TABLE = Microsoft.WindowsAzure.Storage.Table; //8.7.0 because this is the base for 1.0.6

using NEWEST_TABLE = Microsoft.Azure.Cosmos.Table; // version 1.0.6
using Microsoft.Azure.Cosmos.Table; // had to add this to get access CreateCloudTableClient extension method

using System.Diagnostics;

namespace SOshowAzureTableBug
{
    class Program
    {
        const string connectionTableSAS = "TableSecondaryEndpoint=http://127.0.0.1:10002/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;SharedAccessSignature=st=2020-02-24T12%3A47%3A31Z&se=2020-02-25T12%3A47%3A31Z&sp=r&sv=2018-03-28&tn=ratingreflocationa20200217aandmarketvalueb20200217bxml&sig=wc7tW52nstzdGMzlQuaRuakShJ%2BHmpbv8jbMlnn1lug%3D";
        static void Main(string[] args)
        {
            /* Legacy Table SDK */
            var storageAccountLegacy = LEGACY_STORAGE.CloudStorageAccount.Parse(connectionTableSAS);
            var tableClientLegacy = storageAccountLegacy.CreateCloudTableClient();
            Debug.Assert(tableClientLegacy.StorageUri.SecondaryUri != null); // demonstrate SecondaryUri initialised

            var tableRequestOptionsLegacy = new LEGACY_TABLE.TableRequestOptions () { LocationMode = LEGACY_RETRY.LocationMode.SecondaryOnly };
            tableClientLegacy.DefaultRequestOptions = tableRequestOptionsLegacy;

            var tableLegacy = tableClientLegacy.GetTableReference("foo"); // don't need table to exist to show the issue
            var retrieveOperation = LEGACY_TABLE.TableOperation.Retrieve(string.Empty, string.Empty, new List<string>() { "bar" });

            var tableResult = tableLegacy.Execute(retrieveOperation);
            Console.WriteLine("Legacy PASS");


            /* Newset Table SDK */
            var storageAccountNewest = NEWEST_TABLE.CloudStorageAccount.Parse(connectionTableSAS);
            var tableClientNewest = storageAccountNewest.CreateCloudTableClient(new TableClientConfiguration());
            Debug.Assert(tableClientNewest.StorageUri.SecondaryUri != null); // demonstrate SecondaryUri initialised

            var tableRequestOptionsNewest = new NEWEST_TABLE.TableRequestOptions() { LocationMode = NEWEST_TABLE.LocationMode.SecondaryOnly };
            tableClientNewest.DefaultRequestOptions = tableRequestOptionsNewest;

            var tableNewset = tableClientNewest.GetTableReference("foo"); // don't need table to exist to show the issue
            var retrieveOperationNewset = NEWEST_TABLE.TableOperation.Retrieve(string.Empty, string.Empty, new List<string>() { "bar" });

            /* throws Microsoft.Azure.Cosmos.Table.StorageException
             * Exception thrown while initializing request: This operation can only be executed against the primary storage location
             */
            var tableResultNewset = tableNewset.Execute(retrieveOperationNewset);
            
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;


using Microsoft.WindowsAzure.Management.HDInsight;
using Microsoft.Hadoop.Client;

namespace SubmitHiveJob
{
    
    
    
    class Program
    {
        private static void WaitForJobCompletion(JobCreationResults jobResults, IJobSubmissionClient client)
        {
            JobDetails jobInProgress = client.GetJob(jobResults.JobId);
            while (jobInProgress.StatusCode != JobStatusCode.Completed && jobInProgress.StatusCode != JobStatusCode.Failed)
            {
                Console.WriteLine("waiting...");
                jobInProgress = client.GetJob(jobInProgress.JobId);
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }


        static void Main(string[] args)
        {
            string subscriptionId = ConfigurationManager.AppSettings["AzureSubscriptionID"];
            string clusterName = ConfigurationManager.AppSettings["ClusterName"];
            string certfriendlyname = ConfigurationManager.AppSettings["AzurePublisherCertName"];


           
            // define the Hive job
            HiveJobCreateParameters hiveJobDefinition = new HiveJobCreateParameters()
            {
                JobName = "process a tab file",
                StatusFolder = "/AAATweetsOutPut",
                Query =  "DROP TABLE TWEETS;" +
                         "CREATE EXTERNAL TABLE TWEETS(t1 string, t2 string, t3 string, t4 string, t5 string, t6 string, t7 string) "+
                         "ROW FORMAT DELIMITED FIELDS TERMINATED BY  '\t' STORED AS TEXTFILE " +
                         "LOCATION 'wasb://aidanhdinsight@aidan.blob.core.windows.net/AAATweets/';" +
                         "SELECT  COUNT(*) as cnt  FROM TWEETS;"
            };


            // Get the certificate object from certificate store using the friendly name to identify it
            X509Store store = new X509Store();
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2 cert = store.Certificates.Cast<X509Certificate2>().First(item => item.FriendlyName == certfriendlyname);
            JobSubmissionCertificateCredential creds = new JobSubmissionCertificateCredential(new Guid(subscriptionId), cert, clusterName);


            // Submit the Hive job
            var jobClient = JobSubmissionClientFactory.Connect(creds);
            JobCreationResults jobResults = jobClient.CreateHiveJob(hiveJobDefinition);


            // Wait for the job to complete
            WaitForJobCompletion(jobResults, jobClient);


            // Print the Hive job output
            System.IO.Stream stream = jobClient.GetJobOutput(jobResults.JobId);


            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());


            Console.WriteLine("Press ENTER to continue.");
            Console.ReadLine();


        }
    }
}

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


        public static void LoadTweets(JobSubmissionCertificateCredential creds)
        {

           var hiveJobDefinition = new HiveJobCreateParameters()
            {
                JobName = "Load tweets to external table",
                StatusFolder = "/AAALoadTweets",

                Query = "DROP TABLE tweets;" +
                             "CREATE EXTERNAL TABLE tweets( id_str string, created_at string, retweet_count string, tweetText string, userName string, userId string, screenName string, countryCode string, placeType string, placeName string, placeType1 string, coordinates array<string>)" +
                             " ROW FORMAT DELIMITED FIELDS TERMINATED BY '\t' COLLECTION ITEMS TERMINATED BY ',' STORED AS TEXTFILE ;" +
                              " LOAD DATA INPATH 'wasb://aidoshdinsight@aidosacct.blob.core.windows.net/tweets/data' OVERWRITE INTO TABLE tweets;" +
                             "SELECT  COUNT(*) as tweetCount  FROM tweets"
            };


            // Submit the Hive job
            var jobClient = JobSubmissionClientFactory.Connect(creds);
            var jobResults = jobClient.CreateHiveJob(hiveJobDefinition);

            WaitForJobCompletion(jobResults, jobClient);


            // Print the Hive job output
            var stream = jobClient.GetJobOutput(jobResults.JobId);

            var reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());


        }


        public static void AggregateGeoLocations(JobSubmissionCertificateCredential creds)
        {
            var hiveJobDefinition = new HiveJobCreateParameters()
            {
                JobName = "Load tweets to external table",
                StatusFolder = "/AAALoadTweets",

                Query = " DROP TABLE GEOLOCATIONS;" +
                             " CREATE EXTERNAL TABLE GEOLOCATIONS (latitude string, longitude string, magnitude string);" +
                              " INSERT OVERWRITE TABLE GEOLOCATIONS " +
                             " select  coordinates[1],coordinates[0], count(coordinates[0]) from tweets  where coordinates[0] is not null group by coordinates[0],coordinates[1] ;" +
                             " select * from GEOLOCATIONS LIMIT 5"
            };

            // Submit the Hive job
            var jobClient = JobSubmissionClientFactory.Connect(creds);
            var jobResults = jobClient.CreateHiveJob(hiveJobDefinition);

            WaitForJobCompletion(jobResults, jobClient);


            // Print the Hive job output
            var stream = jobClient.GetJobOutput(jobResults.JobId);

            var reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
        }

        private static JobSubmissionCertificateCredential GenerateJobSubmissionCert(AzureSettings settings)
        {
            var store = new X509Store();
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.Cast<X509Certificate2>().First(item => item.FriendlyName == settings.LocalCertName);
            var creds = new JobSubmissionCertificateCredential(new Guid(settings.SuscriptionId), cert, settings.ClusterName);
            return creds;
        }


        public static void TestBasicQuery(JobSubmissionCertificateCredential creds)
        {
            var hiveJobDefinition = new HiveJobCreateParameters()
            {
                JobName = "show tables job",
                StatusFolder = "/AAAShowTables",
                Query = "select * from GEOLOCATIONS;"
            };

            // Submit the Hive job
            var jobClient = JobSubmissionClientFactory.Connect(creds);
            var jobResults = jobClient.CreateHiveJob(hiveJobDefinition);

            WaitForJobCompletion(jobResults, jobClient);


            // Print the Hive job output
            System.IO.Stream stream = jobClient.GetJobOutput(jobResults.JobId);

            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
        }


        static void Main(string[] args)
        {

            string subscriptionId = ConfigurationManager.AppSettings["AzureSubscriptionID"];
            string clusterName = ConfigurationManager.AppSettings["ClusterName"];
            string certfriendlyname = ConfigurationManager.AppSettings["AzurePublisherCertName"];

            var settings = new AzureSettings
                                         {
                                             ClusterName = clusterName,
                                             LocalCertName = certfriendlyname,
                                             SuscriptionId = subscriptionId
                                         };

            var creds = GenerateJobSubmissionCert(settings);
            AggregateGeoLocations(creds);

            Console.WriteLine("Press ENTER to continue.");
            Console.ReadLine();


        }
    }
}

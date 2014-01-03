Add-AzureAccount

$subscriptionName = "Windows Azure MSDN - Visual Studio Ultimate"
$storageAccountName ="aidan"
$containerName="aidanhdinsight"
$clusterName = "aidanhdinsight"

Use-AzureHDInsightCluster $clusterName 


$queryString = "DROP TABLE TWEETS;" +
"CREATE EXTERNAL TABLE TWEETS(t1 string, t2 string, t3 string, t4 string, t5 string, t6 string, t7 string) 
ROW FORMAT DELIMITED FIELDS TERMINATED BY  '\t' 
STORED AS TEXTFILE 
LOCATION 'wasb://$containerName@$storageAccountName.blob.core.windows.net/AAATweets/';" +
                 "SELECT  COUNT(*) as cnt FROM TWEETS;"

$hiveJobDefinition = New-AzureHDInsightHiveJobDefinition -Query $queryString

$hiveJob = Start-AzureHDInsightJob -Cluster $clusterName -JobDefinition $hiveJobDefinition -Verbose
Wait-AzureHDInsightJob -Job $hiveJob -WaitTimeoutInSeconds 3600 -Verbose
Get-AzureHDInsightJobOutput -Cluster $clusterName -JobId $hiveJob.JobId -StandardOutput


***********************************
$subscriptionName = "Windows Azure MSDN - Visual Studio Ultimate"
$storageAccountName ="aidan"
$containerName="aidanhdinsight"


$fileName ="C:\Code\github\tweets.log"
$blobName = "AAATweets/data"


# Get the storage account key
Select-AzureSubscription $subscriptionName
$storageaccountkey = get-azurestoragekey $storageAccountName | %{$_.Primary}


# Create the storage context object
$destContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageaccountkey


# Copy the file from local workstation to the Blob container        
Set-AzureStorageBlobContent -File $fileName -Container $containerName -Blob $blobName -context $destContext


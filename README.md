HadoopTweetAnalyser
===================
A node.js twitter stream aggregator and Hadoop file processor.



# Basic Usage

Specify the key words/ hash tags you want to aggregate from the twitter streaming API 

```javascript
var watchList = ['#nye', '#newyearseve', '#newyear' , '#newyears','#happynewyear', 'new year'];
];
```

Specify a  temporary folder location to store all tweets

```javascript
var RotatingLog = require('rotating-log')
,   logfile     = 'Z:/Downloads-2/Tweets/nye.log'
,   log         = RotatingLog(logfile, {keep:200, maxsize:20000000}) // 20MB in size
```

Ingest and Aggregate the tweets into Hapdoop Distribution (HDInsight) , aggregate by geo -location

Generate a JSON file of the output to visualize using Google Chromes Web GL Globe project

# Technologies

- node.js
- HIVE & HDInsight ( Hadoop distribution on Windows Azure)
- C#.NET to submit jobs to Hive
- Chrome Experiments - WebGL Globe

# More Info

http://acaseyblog.wordpress.com/2014/01/06/a-visualization-of-new-years-eve-2014-on-twitter/
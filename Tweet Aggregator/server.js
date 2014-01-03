var express = require('express')
  , app = express()
  , http = require('http')
  , server = http.createServer(app)
  ,Twit = require('twit')
  , io = require('socket.io').listen(server);

 // var azure = require('azure');


var config = require('./config.json');

io.set('log level', 1); 


var RotatingLog = require('rotating-log')
,   logfile     = 'Z:/Downloads-2/Tweets/nye.log'
,   log         = RotatingLog(logfile, {keep:200, maxsize:20000000}) // 10MB in size

log.on('rotated', function () {
    console.log('The log file was just rotated.')


})
log.on('error', function (err) {
    console.error('There was an error: %s', err.message || err)
})


server.listen(8080);

// routing
app.get('/', function (req, res) {
res.sendfile(__dirname + '/index.html');
});

var watchList = ['#nye', '#newyearseve', '#newyear' , '#newyears','#happynewyear', 'new year'];
 var T = new Twit({
    consumer_key:         config.consumer_key
  , consumer_secret:      config.consumer_secret
  , access_token:         config.access_token
  , access_token_secret:  config.access_token_secret
})


io.sockets.on('connection', function (socket) {
  console.log('Connected');


 var stream = T.stream('statuses/filter', { track: watchList })

  stream.on('tweet', function (tweet) {


var tweetText  = tweet.text;
tweetText = tweetText.replace(/[\n\r\t]/g, '');

    var tweetData = tweet.id_str +  '\t'
                  + tweet.created_at +  '\t'
                  + tweet.retweet_count  +  '\t' 
                  + tweetText +  '\t' 
                  + tweet.user.name +  '\t' 
                  + tweet.user.id_str +  '\t' 
                  + tweet.user.screen_name  +  '\t' ;

    if (tweet.place==null)
    {

     tweetData = tweetData + '\t' +  '\t' +  '\t' +  '\t' +  '\t'   + '\n';

    }               
    else
    {
     tweetData = tweetData  + tweet.place.country_code  +  '\t' 
                  + tweet.place.place_type  +  '\t' 
                  + tweet.place.full_name  +  '\t' 
                  + tweet.place.place_type  +  '\t';
         if(tweet.place.bounding_box ==null)
        {
         tweetData = tweetData+ '\t';
        }
        else
        {
        tweetData = tweetData + tweet.place.bounding_box.coordinates +   '\t' ;
        }
    tweetData = tweetData+ '\n';
                 
    }

    log.write(tweetData);

    io.sockets.emit('stream',tweet.text);

    //log.write( JSON.stringify(tweet) + '\n' );
    //io.sockets.emit('stream',tweet.text);


  });
 });
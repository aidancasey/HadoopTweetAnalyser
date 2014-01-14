var express = require('express'),
    app = express(),
    http = require('http'),
    server = http.createServer(app),
    Twit = require('twit'),
    io = require('socket.io').listen(server);

var config = require('./config.json');

io.set('log level', 1);


var RotatingLog = require('rotating-log'),
    logfile = config.log_directory,
    log = RotatingLog(logfile, {
        keep: 200,
        maxsize: config.log_size
    })

    log.on('rotated', function () {
        console.log('The log file was just rotated.')
    })
    log.on('error', function (err) {
        console.error('There was an error: %s', err.message || err)
    })

    var port = process.env.PORT || 8080;

server.listen(port);

console.log("listening on port " + port);

// routing
app.get('/', function (req, res) {
    res.sendfile(__dirname + '/index.html');
});

var watchList = config.tweet_keywords.split(",");;
var twit = new Twit({
    consumer_key: config.consumer_key,
    consumer_secret: config.consumer_secret,
    access_token: config.access_token,
    access_token_secret: config.access_token_secret
})


io.sockets.on('connection', function (socket) {
    console.log('Connected');


    var stream = twit.stream('statuses/filter', {
        track: watchList
    })

    var logFormatter = require('./tweetParser.js')

    stream.on('tweet', function (tweet) {

        var tweetData = logFormatter.format(tweet);
        log.write(tweetData);
        io.sockets.emit('stream', tweet.text);

    });
});
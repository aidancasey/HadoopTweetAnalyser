module.exports = {

format : function(tweet) {
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
    console.log('tweetData is' + tweetData);
    return tweetData;
   
}};

using MongoDB.Bson;

namespace com.tweetapp.Models
{
    public class Reply
    {
        public ObjectId _id { get; set; }
        public string username { get; set; }
        public string Msg { get; set; }

        public string tweet_id { get; set; }
    }
}

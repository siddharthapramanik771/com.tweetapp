using MongoDB.Bson;
using System.Collections.Generic;

namespace com.tweetapp.Models
{
    public class Tweet
    {
       
        public ObjectId _id { get; set; }
        public string Msg { get; set; }
        public string username { get; set; }
        public List<string> users_liked { get; set; }

        public string GetId
        {
            get
            {
                // returns the string form of the _id at the time of HttpGet call
                return _id.ToString();
            }
        }
        public int likes
        {
            get
            {
                return users_liked.Count;
            }
        }
    }
}

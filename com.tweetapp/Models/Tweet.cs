using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace com.tweetapp.Models
{
    public class Tweet
    {
       
        public ObjectId _id { get; set; }
        [MaxLength(144)]
        public string msg { get; set; }
        public string username { get; set; }
        public List<string> users_liked { get; set; }

        public string getId
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
    public class Msg
    {
        public string msg { get; set; }
    }
}

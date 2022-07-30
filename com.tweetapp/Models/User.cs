using MongoDB.Bson;
using MongoDB.Driver;
using System.Security;

namespace com.tweetapp.Models
{
    public class User
    {
        public ObjectId _id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ContactNumber { get; set; }


    }
}

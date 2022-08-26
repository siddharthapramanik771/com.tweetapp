using MongoDB.Bson;
using MongoDB.Driver;
using System.Security;

namespace com.tweetapp.Models
{
    public class User
    {
        public ObjectId _id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string contactNumber { get; set; }


    }

    public class Password
    {
        public string new_password { get; set; }
    }
}

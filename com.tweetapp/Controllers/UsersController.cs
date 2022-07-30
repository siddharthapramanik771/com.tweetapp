using com.tweetapp.Kafka;
using com.tweetapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace com.tweetapp.Controllers
{
    [Route(V)]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private const string V = "api/v1.0/tweets/";
        private readonly IMongoCollection<User> _collection;
        private readonly IProducer procuder;
        public UsersController(IConfiguration configuration,IProducer producer)
        {
            var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoConnection"));
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("tweetapp");
            _collection = database.GetCollection<User>("users");
            this.procuder = producer;
        }
        private bool Email_validator(string email)
        {
            var user_count = _collection.Find(x => x.Email == email).CountDocuments();
            if (user_count >= 1)
            {
                return false;
            }
            return true;
        }
        private bool username_validator(string username)
        {
            var user_count = _collection.Find(x => x.username == username).CountDocuments();
            if (user_count >= 1)
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        [Route("register")]
        public async  Task<JsonResult> register(string first_name,string last_name,string username,
            string email,string password,string contact_number)
        {
            var user = new User();
            user.FirstName = first_name;
            user.LastName = last_name;
            user.username = username;
            user.Email = email;
            user.Password = new PasswordEvidence(password).GetHashCode().ToString(); ;
            user.ContactNumber = contact_number;
            if (!Email_validator(email))
            {
                return new JsonResult("Email already used by some user");
            }
            if (!username_validator(username))
            {
                return new JsonResult("This username is not available");
            }
            string data = JsonSerializer.Serialize(user);
            return new JsonResult( await procuder.SendRequestToKafkaAsync(Global.request_types[0], data));
            
        }
        [HttpGet]
        [Route("login")]
        public JsonResult login(string username,string password)
        {
            var filter=Builders<User>.Filter.Eq("username", username);
            var users = _collection.Find(filter).ToList();
            if (users.Count == 0)
            {
                return new JsonResult("user not found");
            }
            var user = users[0];
            var _password = new PasswordEvidence(password).GetHashCode().ToString();
            if (user.Password != _password)
            {
                return new JsonResult("Wrong password");
            }
            return new JsonResult("user loggeed in");
        }

        [HttpGet]
        [Route("users/all")]
        public JsonResult users()
        {
            var user_list = _collection.AsQueryable();
            return new JsonResult(user_list);
        }

        [HttpGet]
        [Route("/api/v/1.0/tweets/user/search/{username}")]
        public JsonResult search_users(string username)
        {
            var search = username;
            var builder = Builders<User>.Filter;
            var filter = builder.Regex("username", "^.*" + search + ".*$");
            var user_list=_collection.Find(filter).ToList();
            return new JsonResult(user_list);
        }

        
    }
}

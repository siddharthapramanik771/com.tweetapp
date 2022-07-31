using com.tweetapp.Kafka;
using com.tweetapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
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
        public async  Task<dynamic> register(string first_name,string last_name,string username,
            string email,string password,string contact_number)
        {
            if (email == null | first_name ==null | last_name ==null | username == null | password == null | contact_number == null)
            {
                return "Please fill all the fields";
            }
            var user = new User();
            user.FirstName = first_name;
            user.LastName = last_name;
            user.username = username;
            user.Email = email;
            user.Password = new PasswordEvidence(password).GetHashCode().ToString(); ;
            user.ContactNumber = contact_number;
            if (!Email_validator(email))
            {
                return "Email already used by some user";
            }
            if (!username_validator(username))
            {
                return "This username is not available";
            }
            string data = JsonSerializer.Serialize(user);
            return await procuder.SendRequestToKafkaAsync(Global.request_types[0], data);
            
        }
        [HttpGet]
        [Route("login")]
        public string login(string username,string password)
        {
            var filter=Builders<User>.Filter.Eq("username", username);
            var users = _collection.Find(filter).ToList();
            if (users.Count == 0)
            {
                return "user not found";
            }
            var user = users[0];
            var _password = new PasswordEvidence(password).GetHashCode().ToString();
            if (user.Password != _password)
            {
                return "Wrong password";
            }
            return "user loggeed in";
        }
        [HttpPost]
        [Route("{username}/forgot")]
        public async  Task<dynamic> reset_password(string username, string password, string new_password)
        {
            var login_response = login(username, password);
            if (login_response== "user loggeed in" )
            {
                var user = new User();
                user.username=username;
                user.Password = new PasswordEvidence(new_password).GetHashCode().ToString();
                string data = JsonSerializer.Serialize(user);
                return await procuder.SendRequestToKafkaAsync(Global.request_types[6], data);
            }
            else
            {
                return login_response;
            }
        }

        [HttpGet]
        [Route("users/all")]
        public JsonResult users()
        {
            var user_list = _collection.Find(new BsonDocument()).Project(u =>new  { u.username }).ToList();
            return new JsonResult(user_list);
        }
        // searchs the usernames based on the given string
        [HttpGet]
        [Route("/api/v/1.0/tweets/user/search/{username}")]
        public JsonResult search_users(string username)
        {
            var search = username;
            var builder = Builders<User>.Filter;
            var filter = builder.Regex("username", "^.*" + search + ".*$");
            var fields = Builders<User>.Projection.Include(p => p.username).ToString();
            var user_list=_collection.Find(filter).Project(u => new { u.username }).ToList();
            return new JsonResult(user_list);
        }
        
    }
}

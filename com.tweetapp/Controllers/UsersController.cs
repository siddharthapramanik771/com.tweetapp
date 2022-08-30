using com.tweetapp.Kafka;
using com.tweetapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private IConfiguration _config;

        public UsersController(IConfiguration configuration,IProducer producer)
        {
            var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoConnection"));
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("tweetapp");
            _collection = database.GetCollection<User>("users");
            this.procuder = producer;
            this._config = configuration;
        }
        private bool Email_validator(string email)
        {
            var user_count = _collection.Find(x => x.email == email).CountDocuments();
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
        private bool validator(string input)
        {
            if (input == null)
            {
                return false;
            }
            if (input == "")
            {
                return false;
            }
            if (input == " ")
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        [Route("register")]
        public async  Task<JsonResult> register(User user)
        {
            if (!validator(user.email) | !validator(user.firstName) | !validator(user.lastName) | !validator(user.username) | !validator(user.password) | !validator(user.contactNumber))
            {
                return new JsonResult(new JsonStructure("Please fill up all the fields",false));
            }
            
            user.password = new PasswordEvidence(user.password).GetHashCode().ToString(); ;
            if (!Email_validator(user.email))
            {
                return new JsonResult(new JsonStructure("Email already used by some user",false));
            }
            if (!username_validator(user.username))
            {
                return new JsonResult(new JsonStructure("This username is not available",false));
            }
            string data = JsonSerializer.Serialize(user);
            var result = await procuder.SendRequestToKafkaAsync(Global.request_types[0], data);
            if(result == true)
            {
                var result_data = new JsonStructure("User registered successfully", true);
                result_data.data = user;
                return new JsonResult(result_data);
            }
            else
            {
                return new JsonResult(new JsonStructure("unexpected error try again", false));
            }
        }
        [HttpGet]
        [Route("login")]
        public JsonResult login(string username,string password)
        {
            var filter=Builders<User>.Filter.Eq("username", username);
            var users = _collection.Find(filter).ToList();
            if (users.Count == 0)
            {
                return new JsonResult(new JsonStructure("user not found",false));
            }
            var user = users[0];
            var _password = new PasswordEvidence(password).GetHashCode().ToString();
            if (user.password != _password)
            {
                return new JsonResult(new JsonStructure("Wrong password", false));
            }
            var result = new JsonStructure("user logged in", true);
            result.data = user;
            return new JsonResult(result);
        }
        [HttpPut]
        [Route("{username}/forgot")]
 
        public async  Task<JsonResult> reset_password(string username, Password password)
        {
            var user = new User();
            user.username = username;
            user.password = new PasswordEvidence(password.new_password).GetHashCode().ToString();
            string data = JsonSerializer.Serialize(user);
            var result = await procuder.SendRequestToKafkaAsync(Global.request_types[6], data);
            if (result)
            {
                return new JsonResult(new JsonStructure("password changed successfully", true));
            }
            else
            {
                return new JsonResult(new JsonStructure("Unexpected error", false));
            }
        }

        [HttpGet]
        [Route("users/all")]
        public JsonResult users()
        {
            var user_list = _collection.AsQueryable().ToList();
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
            //var fields = Builders<User>.Projection.Include(p => p.username).ToString();
            var user_list=_collection.Find(filter).ToList();
            return new JsonResult(user_list);
        }
        
    }
}

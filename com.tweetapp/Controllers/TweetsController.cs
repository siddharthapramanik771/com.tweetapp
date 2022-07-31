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
    public class TweetsController : ControllerBase
    {
        private const string V = "api/v1.0/tweets/";
        private readonly IMongoCollection<Tweet> _collection;
        private readonly IMongoDatabase _database;
        private readonly IProducer procuder;
        public TweetsController(IConfiguration configuration, IProducer procuder)
        {
            var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoConnection"));
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("tweetapp");
            _collection = database.GetCollection<Tweet>("tweets");
            this.procuder = procuder;
            this._database= database;
        }

        [HttpGet]
        [Route("all")]
        public JsonResult tweets()
        {
            var tweet_list = _collection.AsQueryable();
            return new JsonResult(tweet_list);
        }
        [HttpGet]
        [Route("{username}")]
        public JsonResult fetch_all_tweets_of_user(string username)
        {
            var filter = Builders<Tweet>.Filter.Eq("username", username);
            var tweets = _collection.Find(filter).ToList();
            return new JsonResult(tweets);
        }

        [HttpPost]
        [Route("{username}/add")]
        public async Task<dynamic> post_tweet(string username,string msg)
        {
            var filter = Builders<User>.Filter.Eq("username", username);
            var users = _database.GetCollection<User>("users").Find(filter).ToList();
            if (users.Count == 0)
            {
                return "user not found";
            }
            var tweet = new Tweet();
            tweet.username= username;
            tweet.Msg = msg;
            tweet.users_liked = new List<string>();
            string data = JsonSerializer.Serialize(tweet);
            return await procuder.SendRequestToKafkaAsync(Global.request_types[1], data);


        }

        [HttpPut]
        [Route("{username}/update/{id}")]
        public async Task<dynamic> update_tweet(string username,string id,string msg)
        {
            var _id =new  ObjectId(id);
            var filter1= Builders<Tweet>.Filter.Eq("_id", _id);
            var tweets = _collection.Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return "tweet not found";
            }
            Tweet tweet = tweets[0];
            if (tweet.username != username)
            {
                return "unauthorized access";
            }
            DataForTweet data = new DataForTweet();
            data.id = tweet.GetId;
            data.msg= msg;
            string final_data = JsonSerializer.Serialize(data);
            return await procuder.SendRequestToKafkaAsync(Global.request_types[2], final_data);
            
        }

        [HttpDelete]
        [Route("{username}/delete/{id}")]
        public async Task<dynamic> delete_tweet(string username, string id)
        {
            var _id = new ObjectId(id);
            var filter1 = Builders<Tweet>.Filter.Eq("_id", _id);
            var tweets = _collection.Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return "tweet not found";
            }
            var tweet = tweets[0];
            if (tweet.username != username)
            {
                return "unauthorized access";
            }
            DataForTweet data=new DataForTweet();
            data.id= tweet.GetId;
            string finaldata = JsonSerializer.Serialize(data);
            return await procuder.SendRequestToKafkaAsync(Global.request_types[3], finaldata);
        }

        [HttpPut]
        [Route("{username}/like/{id}")]
        public async Task<dynamic> like_tweet(string username, string id)
        {
            var filter = Builders<User>.Filter.Eq("username", username);
            var users = _database.GetCollection<User>("users").Find(filter).ToList();
            if (users.Count == 0)
            {
                return "user not found";
            }
            var _id = new ObjectId(id);
            var filter1 = Builders<Tweet>.Filter.Eq("_id", _id);
            var tweets = _collection.Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return "tweet not found";
            }
            Tweet tweet = tweets[0];
            if (tweet.username == username)
            {
                return "You Can not like your own tweet";
            }
            if (tweet.users_liked.Contains(username))
            {
                return "You have already liked this tweet";
            }
            DataForTweet data = new DataForTweet();
            data.id = tweet.GetId;
            data.user_liked = username;
            string finaldata = JsonSerializer.Serialize(data);
            return await procuder.SendRequestToKafkaAsync(Global.request_types[4], finaldata);
        }
    }
}

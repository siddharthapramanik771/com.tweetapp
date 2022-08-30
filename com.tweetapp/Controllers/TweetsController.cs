using com.tweetapp.Kafka;
using com.tweetapp.Models;
using Microsoft.AspNetCore.Authorization;
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
            var tweet_list = _collection.Find(new BsonDocument()).SortByDescending(x=>x._id).ToList();
            return new JsonResult(tweet_list);
        }
        [HttpGet]
        [Route("{username}")]
        public JsonResult fetch_all_tweets_of_user(string username)
        {
            var filter = Builders<Tweet>.Filter.Eq("username", username);
            var tweets = _collection.Find(filter).SortByDescending(x => x._id).ToList();
            return new JsonResult(tweets);
        }

        [HttpPost]
        [Route("{username}/add")]
        public async Task<JsonResult> post_tweet(string username,Msg msg)
        {
            var filter = Builders<User>.Filter.Eq("username", username);
            var users = _database.GetCollection<User>("users").Find(filter).ToList();
            if (users.Count == 0)
            {
                return new JsonResult(new JsonStructure("user not found",false));
            }
            var tweet = new Tweet();
            tweet.username= username;
            tweet.msg = msg.msg;
            tweet.users_liked = new List<string>();
            string data = JsonSerializer.Serialize(tweet);
            var result = await procuder.SendRequestToKafkaAsync(Global.request_types[1], data);
            var final_msg = "";
            if (result)
            {
                final_msg = "tweet is posted ";
            }
            else
            {
                final_msg = "unexpected error";
            }

            return new JsonResult(new JsonStructure(final_msg,result));


        }

        [HttpPut]
        [Route("{username}/update/{id}")]
        public async Task<JsonResult> update_tweet(string username,string id,Msg msg)
        {
            var _id =new  ObjectId(id);
            var filter1= Builders<Tweet>.Filter.Eq("_id", _id);
            var tweets = _collection.Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return new JsonResult(new JsonStructure("tweet not found",false));
            }
            Tweet tweet = tweets[0];
            if (tweet.username != username)
            {
                return new JsonResult(new JsonStructure("unauthorized access", false));
            }
            DataForTweet data = new DataForTweet();
            data.id = tweet.getId;
            data.msg= msg.msg;
            string final_data = JsonSerializer.Serialize(data);
            var result = await procuder.SendRequestToKafkaAsync(Global.request_types[2], final_data);
            if(result == true)
            {
                return new JsonResult(new JsonStructure("tweet is updating",true ));
            }
            else
            {
                return new JsonResult(new JsonStructure("network error , try again",false));
            }
            
        }

        [HttpDelete]
        [Route("{username}/delete/{id}")]
        public async Task<JsonResult> delete_tweet(string username, string id)
        {
            var _id = new ObjectId(id);
            var filter1 = Builders<Tweet>.Filter.Eq("_id", _id);
            var tweets = _collection.Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return new JsonResult(new JsonStructure("tweet not found",false));
            }
            var tweet = tweets[0];
            if (tweet.username != username)
            {
                return new JsonResult(new JsonStructure("unauthorized access", false));
            }
            DataForTweet data=new DataForTweet();
            data.id= tweet.getId;
            string finaldata = JsonSerializer.Serialize(data);
            var result = await procuder.SendRequestToKafkaAsync(Global.request_types[3], finaldata);
            if (result == true)
            {
                return new JsonResult(new JsonStructure("tweet is deleted", true));
            }
            else
            {
                return new JsonResult(new JsonStructure("network error, try again", false));
            }
        }

        [HttpPut]
        [Route("{username}/like/{id}")]
        public async Task<JsonResult> like_tweet(string username, string id)
        {
            var filter = Builders<User>.Filter.Eq("username", username);
            var users = _database.GetCollection<User>("users").Find(filter).ToList();
            if (users.Count == 0)
            {
                return  new JsonResult(new JsonStructure("user not found", false));
            }
            var _id = new ObjectId(id);
            var filter1 = Builders<Tweet>.Filter.Eq("_id", _id);
            var tweets = _collection.Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return new JsonResult(new JsonStructure("tweet not found", false));
            }
            Tweet tweet = tweets[0];
            if (tweet.username == username)
            {
                return new JsonResult(new JsonStructure("You Can not like your own tweet", false));
            }
            if (tweet.users_liked.Contains(username))
            {
                return new JsonResult(new JsonStructure("You have already liked this tweet", true));
            }
            DataForTweet data = new DataForTweet();
            data.id = tweet.getId;
            data.user_liked = username;
            string finaldata = JsonSerializer.Serialize(data);
            var result = await procuder.SendRequestToKafkaAsync(Global.request_types[4], finaldata);
            if (result)
            {
                return new JsonResult(new JsonStructure("You have liked the tweet",true));
            }
            else
            {
                return new JsonResult(new JsonStructure("unexpected error", false));
            }
        }
    }
}

using com.tweetapp.Kafka;
using com.tweetapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Threading.Tasks;

namespace com.tweetapp.Controllers
{
    [Route(V)]
    [ApiController]
    public class ReplyController : ControllerBase
    {
        private const string V = "api/v1.0/tweets/";
        private readonly IMongoCollection<Reply> _collection;
        private readonly IMongoDatabase _database;
        private readonly IProducer procuder;
        public ReplyController(IConfiguration configuration, IProducer procuder)
        {
            var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoConnection"));
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("tweetapp");
            _collection = database.GetCollection<Reply>("replys");
            this.procuder = procuder;
            _database = database;
        }
        [HttpGet]
        [Route("{id}/replys")]
        public JsonResult fetch_all_replys_of_tweet(string id)
        {
            var reply_list=_collection.Find(x => x.tweet_id == id).SortBy(x => x._id).ToList(); ;
            return new JsonResult(reply_list);

        }

        [HttpPost]
        [Route("{username}/reply/{id}")]
        public async Task<JsonResult> reply_tweet(string username,string id,Msg msg)
        {
            var filter = Builders<User>.Filter.Eq("username", username);
            var users = _database.GetCollection<User>("users").Find(filter).ToList();
            if (users.Count == 0)
            {
                return new JsonResult(new JsonStructure("user not found",false));
            }
            var filter1 = Builders<Tweet>.Filter.Eq("_id", new ObjectId(id));
            var tweets = _database.GetCollection<Tweet>("tweets").Find(filter1).ToList();
            if (tweets.Count == 0)
            {
                return new JsonResult(new JsonStructure("tweet not found",false));
            }
            var reply = new Reply();
            reply.username = username;
            reply.tweet_id = id;
            reply.msg = msg.msg;
            string data = JsonSerializer.Serialize(reply);
            var result= await procuder.SendRequestToKafkaAsync(Global.request_types[5], data);
            if (result)
            {
                return new JsonResult(new JsonStructure("reply message is posted", true));
            }
            else
            {
                return new JsonResult(new JsonStructure("unexpected error", false));
            }
        }

    }
}

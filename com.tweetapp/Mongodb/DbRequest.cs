using com.tweetapp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace com.tweetapp.Mongodb
{
    public class DbRequest:IDbRequest
    {
        private readonly ILogger<DbRequest> logger;
        private readonly IMongoDatabase _database;
        public DbRequest(IConfiguration configuration, ILogger<DbRequest> logger)
        {
            var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoConnection"));
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            _database = client.GetDatabase("tweetapp");
            this.logger = logger;
        }
        
        public async Task<bool> process_request(string request_type,string data)
        {
            try
            {
                if (request_type == Global.request_types[0])
                {
                    User user = JsonSerializer.Deserialize<User>(data);
                    await _database.GetCollection<User>("users").InsertOneAsync(user);
                    return true;
                }
                else if (request_type == Global.request_types[1])
                {
                    Tweet tweet = JsonSerializer.Deserialize<Tweet>(data);
                    await _database.GetCollection<Tweet>("tweets").InsertOneAsync(tweet);
                    return true;
                }
                else if (request_type == Global.request_types[2])
                {
                    DataForTweet tweet = JsonSerializer.Deserialize<DataForTweet>(data);
                    var filter = Builders<Tweet>.Filter.Eq("_id",new ObjectId(tweet.id));
                    var update = Builders<Tweet>.Update.Set("msg", tweet.msg);
                    await _database.GetCollection<Tweet>("tweets").UpdateOneAsync(filter,update); 
                    return true;
                }
                else if (request_type == Global.request_types[3])
                {
                    DataForTweet tweet = JsonSerializer.Deserialize<DataForTweet>(data);
                    var filter = Builders<Tweet>.Filter.Eq("_id",new ObjectId(tweet.id));
                    await _database.GetCollection<Tweet>("tweets").DeleteOneAsync(filter);
                    return true;
                }
                else if (request_type == Global.request_types[4])
                {
                    DataForTweet tweet = JsonSerializer.Deserialize<DataForTweet>(data);
                    var filter = Builders<Tweet>.Filter.Eq("_id", new ObjectId(tweet.id));
                    var update = Builders<Tweet>.Update.Push("users_liked", tweet.user_liked);
                    await _database.GetCollection<Tweet>("tweets").UpdateOneAsync(filter, update);
                    return true;     
                }
                else if(request_type == Global.request_types[5])
                {
                    var reply = JsonSerializer.Deserialize<Reply>(data);
                    await _database.GetCollection<Reply>("replys").InsertOneAsync(reply);
                    return true;
                }
                else if (request_type == Global.request_types[6])
                {
                    User user = JsonSerializer.Deserialize<User>(data);
                    var filter = Builders<User>.Filter.Eq("username", user.username);
                    var update = Builders<User>.Update.Set("password", user.password);
                    await _database.GetCollection<User>("users").UpdateOneAsync(filter, update);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return false;
            }
        }
    }
}

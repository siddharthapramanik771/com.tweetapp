using System.Collections.Generic;
// this class is used as a global data store 
// created by s.pramanik
namespace com.tweetapp
{
    public class Global
    {
        public static readonly List<string> request_types = new List<string> { "register_user", "add_tweet", "edit_tweet", "delete_tweet", "like_tweet", "reply_to_tweet","reset_password" };
    }
}

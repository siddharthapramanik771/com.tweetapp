namespace com.tweetapp.Models
{
    public class JsonStructure
    {
        public JsonStructure(string msg, bool status)
        {
            this.msg = msg;
            this.status = status;
        }

        public string msg { get; set; }
        public bool status { get; set; }
        public object data { get; set; }
    }
}

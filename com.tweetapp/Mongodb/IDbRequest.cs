using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.tweetapp.Mongodb
{
    public interface IDbRequest
    {
        Task<bool> process_request(string request_type, string data);
    }
}
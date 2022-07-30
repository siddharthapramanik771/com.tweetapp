using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.tweetapp.Kafka
{
    public interface IProducer
    {
        Task<bool> SendRequestToKafkaAsync(string topic, string message);
    }
}
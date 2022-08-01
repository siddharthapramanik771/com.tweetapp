using NUnit.Framework;
using com.tweetapp.Controllers;

namespace com.tweetapp.test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            var tweeet = new TweetsController();
        
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}
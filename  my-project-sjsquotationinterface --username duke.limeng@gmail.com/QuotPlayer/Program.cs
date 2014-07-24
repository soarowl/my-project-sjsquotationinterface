using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace QuotPlayer
{
    class Program
    {
        static protected MongoClient mongoClient;
        static protected MongoCollection dataCollection;
        static protected Log4cb.ILog4cbHelper logHelper;
        static void Main(string[] args)
        {
            initMongoDB("192.168.1.167",6678);


        }
        private static void initMongoDB(string ip, int port)
        {
            MongoClientSettings mongoSettings = new MongoClientSettings();
            mongoSettings.Servers = new List<MongoServerAddress>() 
            {
            new MongoServerAddress (ip,port)
            };
            mongoClient = new MongoClient(mongoSettings);
            MongoServer mongoServer = mongoClient.GetServer();
            MongoDatabase mongoDB = mongoServer.GetDatabase("V5");
            dataCollection = mongoDB.GetCollection("MessagePack");
        }

    }
}

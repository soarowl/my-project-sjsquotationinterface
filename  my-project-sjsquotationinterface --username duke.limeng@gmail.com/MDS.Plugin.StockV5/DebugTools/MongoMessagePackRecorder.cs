using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MDS.Plugin.SZQuotV5
{
    class MongoMessagePackRecorder : QuotV5.Binary.IMessagePackRecorder
    {
        protected MongoClient mongoClient;
        protected MongoCollection dataCollection;
        protected Log4cb.ILog4cbHelper logHelper;
        public MongoMessagePackRecorder(string ip, int port, Log4cb.ILog4cbHelper logHelper)
        {
            this.logHelper = logHelper;
            initMongoDB(ip, port);
        }

        public void Record(QuotV5.Binary.MessagePackEx msgPack)
        {
            MessagePack mp = new MessagePack()
            {
                ReciveTime = msgPack.ReceiveTime,
                Data = QuotV5.DataHelper.UnionByteArrays(msgPack.MessagePack.HeaderData, msgPack.MessagePack.BodyData, msgPack.MessagePack.TrailerData)
            };
            try
            {
                this.dataCollection.Insert(mp);
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "保存MessagePack到Mongo异常");
            }
        }

        private void initMongoDB(string ip, int port)
        {
            MongoClientSettings mongoSettings = new MongoClientSettings();
            mongoSettings.Servers = new List<MongoServerAddress>() 
            {
            new MongoServerAddress (ip,port)
            };
            this.mongoClient = new MongoClient(mongoSettings);
            MongoServer mongoServer = this.mongoClient.GetServer();
            MongoDatabase mongoDB = mongoServer.GetDatabase("V5");
            this.dataCollection = mongoDB.GetCollection("MessagePack");
        }
    }
    class MessagePack
    {
        public ObjectId _id;
        public DateTime ReciveTime { get; set; }
        public byte[] Data { get; set; }

    }
}

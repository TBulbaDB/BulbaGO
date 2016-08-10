using System;
using System.Collections.Concurrent;
using MongoDB.Driver;

namespace BulbaGO.Base.MongoDB
{
    public static class MongoHelper
    {
        private static readonly MongoClient Client;
        private static readonly string DefaultDatabase;

        private static readonly ConcurrentDictionary<Type, string> TypeNames = new ConcurrentDictionary<Type, string>();

        static MongoHelper()
        {
            var connectionString = "mongodb://localhost/BulbaGO";
            DefaultDatabase = MongoUrl.Create(connectionString).DatabaseName;
            Client = new MongoClient(connectionString);
        }

        private static IMongoDatabase GetDatabase(string databaseName = null)
        {
            if (databaseName == null)
            {
                databaseName = DefaultDatabase;
            }
            var db = Client.GetDatabase(databaseName);
            return db;
        }

        public static IMongoCollection<T> GetCollection<T>(string collectionName = null, string databaseName = null)
        {
            var db = GetDatabase(databaseName);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = GetTypeName(typeof(T));
            }
            return db.GetCollection<T>(collectionName);
        }

        private static string GetTypeName(Type type)
        {
            return TypeNames.GetOrAdd(type, (t) =>
            {
                var collectionName = t.Name;
                if (t.GenericTypeArguments.Length > 0)
                {
                    collectionName = collectionName.Remove(collectionName.IndexOf('`')) + "-" +
                                     t.GenericTypeArguments[0].Name;
                }
                return collectionName;

            });
        }

        public static void DropCollection<T>(string collectionName = null, string databaseName = null)
        {
            var db = GetDatabase(databaseName);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }
            db.DropCollection(collectionName);
        }
    }
}

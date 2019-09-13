﻿using Contoso.Apps.Common;
using Contoso.Apps.Movies.Data.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGenerator
{
    public class UserActor
    {
        public int Type { get; set; }
        public int UserId { get; set; }

        public UserActor(int userId, int type)
        {
            this.Type = type;
            this.UserId = UserId;
        }

        public void DoWork()
        {
            //execute actions of a user...
            Guid sessionId = Guid.NewGuid();

            //import genre/category
            string endpointUrl = ConfigurationManager.AppSettings["dbConnectionUrl"];
            string authorizationKey = ConfigurationManager.AppSettings["dbConnectionKey"];
            string databaseId = ConfigurationManager.AppSettings["databaseId"];

            DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey, new ConnectionPolicy { ConnectionMode = ConnectionMode.Gateway, ConnectionProtocol = Protocol.Https });
            Database database = client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId }).Result;

            DbHelper.client = client;
            DbHelper.databaseId = databaseId;

            List<Item> movies = DbHelper.GetMoviesByType(Type);

            Random r = new Random();

            int count = 20;

            //loop...
            while (true)
            {
                Thread.Sleep(50);

                //randomly get a movie
                Item p = GetRandomMovie(movies);

                //randomly do this x times
                int val = r.Next(10);

                int action = val % 3;

                if (action == 0)
                    DbHelper.GenerateAction(UserId, p.ItemId.ToString(), "details", sessionId.ToString().Replace("-", ""));

                if (action == 1)
                {
                    DbHelper.GenerateAction(UserId, p.ItemId.ToString(), "buy", sessionId.ToString().Replace("-", ""));

                    int failure = r.Next(10);

                    //simulate a failure...
                    if (failure % 2 == 1 && count > 20)
                    {
                        DbHelper.GenerateAction(UserId, p.ItemId.ToString(), "paymentFailure", sessionId.ToString().Replace("-", ""));
                        count = 0;
                    }

                    count++;
                }

                if (action == 2)
                    DbHelper.GenerateAction(UserId, p.ItemId.ToString(), "addToCart", sessionId.ToString().Replace("-", ""));
            }
        }

        static Item GetRandomMovie(List<Item> movieSet)
        {
            Random r = new Random();
            return movieSet.Skip(r.Next(movieSet.Count)).Take(1).FirstOrDefault();
        }
    }
}

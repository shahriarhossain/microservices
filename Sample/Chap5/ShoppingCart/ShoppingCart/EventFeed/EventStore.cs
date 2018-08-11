﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace ShoppingCart.EventFeed
{
    public class EventStore : IEventStore
    {
        private const string writeEventSql =
            @"insert into EventStore(Name, OccurredAt, Content) values
              (@Name, @OccurredAt, @Content)";

        private const string readEventsSql =
            @"select * from EventStore where ID >= @Start and ID <= @End";

        private const string connectionString = "discover://http://127.0.0.1:2113/";
        private IEventStoreConnection connection = EventStoreConnection.Create(connectionString);

        private static long currentSequenceNumber = 0;
        private static readonly IList<Event> database = new List<Event>();

        public async Task<IEnumerable<Event>> GetEvents(
            long firstEventSequenceNumber,
            long lastEventSequenceNumber)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return (await conn.QueryAsync<dynamic>(
                        readEventsSql,
                        new
                        {
                            Start = firstEventSequenceNumber,
                            End = lastEventSequenceNumber
                        }).ConfigureAwait(false))
                    .Select(row =>
                    {
                        var content = JsonConvert.DeserializeObject(row.Content);
                        return new Event(row.Id, row.OccurredAt, row.Name, content);
                    });
            }
        }

        public async Task Raise(string eventName, object content)
        {
            await connection.ConnectAsync().ConfigureAwait(false);
            var jsonContent = JsonConvert.SerializeObject(content);
            var metaDataJson =
                JsonConvert.SerializeObject(new EventMetadata
                {
                    OccurredAt = DateTimeOffset.Now,
                    EventName = eventName
                });

            var eventData = new EventData(
                Guid.NewGuid(),
                "ShoppingCartEvent",
                isJson: true,
                data: Encoding.UTF8.GetBytes(jsonContent),
                metadata: Encoding.UTF8.GetBytes(metaDataJson)
            );

            //Writes the event to EventStore
            await
                connection.AppendToStreamAsync(
                    "ShoppingCart",
                    ExpectedVersion.Any,
                    eventData);
        }

        private class EventMetadata
        {
            public DateTimeOffset OccurredAt { get; set; }
            public string EventName { get; set; }
        }
    }
}
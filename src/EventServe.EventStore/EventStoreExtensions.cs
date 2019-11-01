using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EventServe.EventStore
{
    public static class EventStoreExtensions
    {
        public static async Task SetDefaultStreamMetaData(this IEventStoreConnection conn, string stream)
        {
            try
            {
                var acl = new StreamAcl("$admins", "$admins", "$admins", "$admins", "$admins");
                var metaData = StreamMetadata.Create(acl: acl);
                var result = await conn.SetStreamMetadataAsync(stream, ExpectedVersion.Any, metaData);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
      
        public static async Task<bool> CreateSubscription(
            this IEventStoreConnection conn, 
            string stream, 
            string group, 
            UserCredentials credentials,
            Microsoft.Extensions.Logging.ILogger logger,
            bool startFromCurrent = false)
        {
            var settings = (startFromCurrent) ?
                PersistentSubscriptionSettings
                .Create()
                .DoNotResolveLinkTos()
                .StartFromCurrent() :
                PersistentSubscriptionSettings
                .Create()
                .DoNotResolveLinkTos()
                .StartFromBeginning();

            try
            {
                await conn.CreatePersistentSubscriptionAsync(stream, group, settings, credentials);
                return true;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                if (invalidOperationException.Message == $"Subscription group {group} on stream {stream} already exists")
                    return true;

                logger.LogError(invalidOperationException, invalidOperationException.Message);
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                throw;
            }
        }
    }

}

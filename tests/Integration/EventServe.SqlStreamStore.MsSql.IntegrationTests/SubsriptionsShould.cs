using EventServe.SqlStreamStore.Subscriptions;
using EventServe.Subscriptions;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Xunit;

namespace EventServe.SqlStreamStore.MsSql.IntegrationTests
{
    [Collection("SqlStreamStore Collection")]
    public class SubsriptionsShould
    {
        private readonly EmbeddedMsSqlStreamStoreFixture _fixture;
        private readonly SqlStreamStoreEventSerializer _serializer;
        private readonly MsSqlStreamStoreSettingsProvider _settingsProvider;
        private readonly MsSqlStreamStoreProvider _storeProvider;

        public SubsriptionsShould(EmbeddedMsSqlStreamStoreFixture fixture)
        {
            _fixture = fixture;
            _serializer = new SqlStreamStoreEventSerializer();
            _settingsProvider = new MsSqlStreamStoreSettingsProvider(_fixture.ConnectionString, "TestSchema");
            _storeProvider = new MsSqlStreamStoreProvider(_settingsProvider);
        }

        //[Fact]
        //public async Task Test()
        //{
        //    //var subscription = new sqlstreamstoretransientsubscriptionconnection(_serializer, _storeprovider, null);
        //    //sub
            
        //    //var count = 0;
        //    //var eventobserver = observer.create<subscriptionmessage>(@event =>
        //    //{
        //    //    count += 1;
        //    //});

        //    //subscription.subscribe(eventobserver);
        //}
    }
}

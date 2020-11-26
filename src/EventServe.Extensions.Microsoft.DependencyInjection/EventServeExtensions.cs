using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventServe.Projections;
using EventServe.Projections.Partitioned;
using EventServe.Services;
using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Transient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace EventServe.Extensions.Microsoft.DependencyInjection
{
    public static class EventServeExtensions
    {
        public static void AddEventServeCore(this IServiceCollection services, Assembly[] assemblies)
        {
            services.AddTransient<ISubscriptionRootManager, SubscriptionRootManager>();
            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();

            services.ConnectImplementationsToTypesClosing(typeof(PartitionedProjectionProfile<>), assemblies, true, ServiceLifetime.Singleton, recognizeType: t => ServiceRegistrationCache.ProjectionProfileTypes.Add(t));
            services.RegisterAllTypesWithBaseType<PersistentSubscriptionProfile>(assemblies, ServiceLifetime.Singleton);
            services.RegisterAllTypesWithBaseType<TransientSubscriptionProfile>(assemblies, ServiceLifetime.Singleton);

            services.ConnectImplementationsToTypesClosing(typeof(ISubscriptionEventHandler<,>), assemblies, false);
            services.ConnectImplementationsToTypesClosing(typeof(IProjectionEventHandler<,>), assemblies, false);
            services.ConnectImplementationsToTypesClosing(typeof(IPartitionedProjectionEventHandler<,>), assemblies, false);
            services.ConnectImplementationsToTypesClosing(typeof(IPartitionedProjectionPartitionIdSelector<,>), assemblies, false);
            services.ConnectImplementationsToTypesClosing(typeof(IPersistentSubscriptionResetHandler<>), assemblies, false);
            services.AddTransient(typeof(IEventRepository<>), typeof(EventRepository<>));
            services.AddTransient(typeof(IPartitionedProjectionQuery<>), typeof(PartitionedProjectionQuery<>));
        }

        public static void UseEventServe(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.RegisterEventServeSubscriptions();
        }

        //TODO - clean this up
        public static void RegisterEventServeSubscriptions(this IApplicationBuilder applicationBuilder)
        {
            var rootManager = applicationBuilder.ApplicationServices.GetRequiredService<ISubscriptionRootManager>();
            var subscriptionsTask = rootManager.GetSubscriptions();
            Task.WaitAll(subscriptionsTask);

            var subscriptions = subscriptionsTask.Result;

            //Start up internal subscriptions
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var managerProfiles = scope.ServiceProvider.GetServices<TransientSubscriptionProfile>();
                foreach (var profile in managerProfiles.Where(x => x.GetType() == typeof(SubscriptionManagerProfile)))
                {
                    //Fetch a new instance persistent subscription from the IoC container
                    var subscription = applicationBuilder.ApplicationServices.GetRequiredService<ITransientStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var profileType = profile.GetType();
                        var messageObserverType = typeof(SubscriptionMessageObserver<,>).MakeGenericType(profileType, eventType);
                        var messageObserver = (IObserver<SubscriptionMessage>)Activator.CreateInstance(messageObserverType, applicationBuilder.ApplicationServices, profile.Filter);
                        subscription.Subscribe(messageObserver);
                    }

                    var subId = Guid.NewGuid();
                    var connectionSettings = new TransientStreamSubscriptionConnectionSettings(subId,
                                                                                               profile.GetType().Name,
                                                                                               profile.StreamPosition,
                                                                                               profile.Filter.SubscribedStreamId,
                                                                                               profile.Filter.AggregateType?.Name);
                    manager.Add(subId, subscription, connectionSettings).Wait();
                    manager.Connect(subId).Wait();
                }
            }

            //Start up transient subscriptions
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices<TransientSubscriptionProfile>();
                foreach (var profile in profiles.Where(x => x.GetType() != typeof(SubscriptionManagerProfile)))
                {
                    if (profile.Disabled) continue;

                    //Fetch a new instance persistent subscription from the IoC container
                    var subscription = applicationBuilder.ApplicationServices.GetRequiredService<ITransientStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var profileType = profile.GetType();
                        var messageObserverType = typeof(SubscriptionMessageObserver<,>).MakeGenericType(profileType, eventType);
                        var messageObserver = (IObserver<SubscriptionMessage>)Activator.CreateInstance(messageObserverType, applicationBuilder.ApplicationServices, profile.Filter);
                        subscription.Subscribe(messageObserver);
                    }

                    var sub = subscriptions.FirstOrDefault(x => x.Name == profile.GetType().Name);
                    if (sub == default)
                    {
                        var subscriptionBase = rootManager.CreateTransientSubscription(profile.GetType().Name).Result;
                        var connectionSettings = new TransientStreamSubscriptionConnectionSettings(subscriptionBase.SubscriptionId,
                                                                                                   profile.GetType().Name,
                                                                                                   profile.StreamPosition,
                                                                                                   profile.Filter.SubscribedStreamId,
                                                                                                   profile.Filter.AggregateType?.Name);
                        manager.Add(subscriptionBase.SubscriptionId, subscription, connectionSettings).Wait();
                        rootManager.StartSubscription(subscriptionBase.SubscriptionId).Wait();
                    }
                    else
                    {
                        var connectionSettings = new TransientStreamSubscriptionConnectionSettings(sub.SubscriptionId,
                                                                                                   profile.GetType().Name,
                                                                                                   profile.StreamPosition,
                                                                                                   profile.Filter.SubscribedStreamId,
                                                                                                   profile.Filter.AggregateType?.Name);

                        manager.Add(sub.SubscriptionId, subscription, connectionSettings).Wait();
                        if (sub.Connected)
                            manager.Connect(sub.SubscriptionId).Wait();
                    }
                }
            }

            //Start up persistent subscriptions
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices<PersistentSubscriptionProfile>();
                foreach (var profile in profiles)
                {
                    if (profile.Disabled) continue;


                    //Fetch a new instance persistent subscription from the IoC container
                    var connection = applicationBuilder.ApplicationServices.GetRequiredService<IPersistentStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var profileType = profile.GetType();

                        var messageObserverType = typeof(SubscriptionMessageObserver<,>).MakeGenericType(profileType, eventType);
                        var messageObserver = (IObserver<SubscriptionMessage>)Activator.CreateInstance(messageObserverType, applicationBuilder.ApplicationServices, profile.Filter);
                        connection.Subscribe(messageObserver);

                        var resetObserverType = typeof(PersistentSubscriptionResetObserver<>).MakeGenericType(profileType);
                        var resetObserver = (IObserver<PersistentSubscriptionResetEvent>)Activator.CreateInstance(resetObserverType, applicationBuilder.ApplicationServices);
                        connection.Subscribe(resetObserver);
                    }

                    var sub = subscriptions.FirstOrDefault(x => x.Name == profile.GetType().Name);
                    SetupPersistentSubscription(
                        sub?.SubscriptionId,
                        profile.GetType().Name,
                        profile.Filter.SubscribedStreamId,
                        profile.Filter.AggregateType?.Name,
                        sub == null ? false : sub.Connected,
                        connection,
                        rootManager,
                        manager);
                }
            }


            //Start up partitioned projections
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices(typeof(IPartitionedProjectionProfile));

                var profileTypes = ServiceRegistrationCache.ProjectionProfileTypes;

                foreach (var profileType in profileTypes)
                {
                    var profile = scope.ServiceProvider.GetRequiredService(profileType) as IPartitionedProjectionProfile;

                    //Fetch a new instance persistent subscription from the IoC container
                    var connection = applicationBuilder.ApplicationServices.GetRequiredService<IPersistentStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var observerType = typeof(PartitionedProjectionObserver<,>).MakeGenericType(profile.ProjectionType, eventType);
                        var observer = (IObserver<SubscriptionMessage>)Activator.CreateInstance(observerType, applicationBuilder.ApplicationServices, profile.Filter);
                        connection.Subscribe(observer);

                        var resetObserverType = typeof(PartitionedProjectionResetObserver<>).MakeGenericType(profile.ProjectionType);
                        var resetObserver = (IObserver<PersistentSubscriptionResetEvent>)Activator.CreateInstance(resetObserverType, applicationBuilder.ApplicationServices);
                        connection.Subscribe(resetObserver);
                    }

                    var sub = subscriptions.FirstOrDefault(x => x.Name == profile.GetType().Name);
                    SetupPersistentSubscription(
                        sub?.SubscriptionId,
                        profile.GetType().Name,
                        profile.Filter.SubscribedStreamId,
                        profile.Filter.AggregateType?.Name,
                        sub == null ? false : sub.Connected,
                        connection,
                        rootManager,
                        manager);
                }
            }

            //Setup partitioned projection queries
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices<IPartitionedProjectionProfile>();

                foreach (var profile in profiles)
                {
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var handlerType = typeof(IPartitionedProjectionEventHandler<,>).MakeGenericType(profile.ProjectionType, eventType);

                        //Register the handler


                        //var observerType = typeof(PartitionedProjectionObserver<,>).MakeGenericType(profile.ProjectionType, eventType);
                        //var observer = (IObserver<SubscriptionMessage>)Activator.CreateInstance(observerType, applicationBuilder.ApplicationServices, profile.Filter);
                        //connection.Subscribe(observer);

                        //var resetObserverType = typeof(PartitionedProjectionResetObserver<>).MakeGenericType(profile.ProjectionType);
                        //var resetObserver = (IObserver<PersistentSubscriptionResetEvent>)Activator.CreateInstance(resetObserverType, applicationBuilder.ApplicationServices);
                    }

                }
            }

            //Start up projections
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices<ProjectionProfile>();

                foreach (var profile in profiles)
                {
                    //Fetch a new instance persistent subscription from the IoC container
                    var connection = applicationBuilder.ApplicationServices.GetRequiredService<IPersistentStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var observerType = typeof(ProjectionObserver<,>).MakeGenericType(profile.ProjectionType, eventType);
                        var observer = (IObserver<SubscriptionMessage>)Activator.CreateInstance(observerType, applicationBuilder.ApplicationServices, profile.Filter);
                        connection.Subscribe(observer);

                        var resetObserverType = typeof(PartitionedProjectionResetObserver<>).MakeGenericType(profile.ProjectionType);
                        var resetObserver = (IObserver<PersistentSubscriptionResetEvent>)Activator.CreateInstance(resetObserverType, applicationBuilder.ApplicationServices);
                        connection.Subscribe(resetObserver);
                    }

                    var sub = subscriptions.FirstOrDefault(x => x.Name == profile.GetType().Name);
                    SetupPersistentSubscription(
                        sub?.SubscriptionId,
                        profile.GetType().Name,
                        profile.Filter.SubscribedStreamId,
                        profile.Filter.AggregateType?.Name,
                        sub == null ? false : sub.Connected,
                        connection,
                        rootManager,
                        manager);
                }
            }

        }

        private static void SetupPersistentSubscription(Guid? subscriptionId, string profileName, StreamId streamId,
                                                        string aggregateType, bool isConnected,
                                                        IPersistentStreamSubscriptionConnection connection,
                                                        ISubscriptionRootManager rootManager,
                                                        ISubscriptionManager manager)
        {
            if (!subscriptionId.HasValue)
            {
                var subscriptionBase = rootManager.CreatePersistentSubscription(profileName).Result;
                var connectionSettings = new PersistentStreamSubscriptionConnectionSettings(subscriptionBase.SubscriptionId, profileName, streamId, aggregateType);
                manager.Add(subscriptionBase.SubscriptionId, connection, connectionSettings).Wait();
                rootManager.StartSubscription(subscriptionBase.SubscriptionId).Wait();
                //manager.Connect(subscriptionBase.SubscriptionId).Wait();
                return;
            }
            else
            {
                var connectionSettings = new PersistentStreamSubscriptionConnectionSettings(subscriptionId.Value, profileName, streamId, aggregateType);
                manager.Add(subscriptionId.Value, connection, connectionSettings).Wait();

                if (isConnected)
                    manager.Connect(subscriptionId.Value).Wait();
            }
        }
    }
}

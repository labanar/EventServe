﻿using EventServe.Services;
using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Transient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventServe.Extensions.Microsoft.DependencyInjection
{
    public static class EventServeExtensions
    {
        public static void AddEventServeCore(this IServiceCollection services, Assembly[] assemblies)
        {
            services.AddTransient<ISubscriptionRootManager, SubscriptionRootManager>();
            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
            services.RegisterAllTypesWithBaseType<PersistentSubscriptionProfile>(assemblies, ServiceLifetime.Singleton);
            services.RegisterAllTypesWithBaseType<TransientSubscriptionProfile>(assemblies, ServiceLifetime.Singleton);
            services.AddTransient<ISusbcriptionProfileHandlerResolver, SubscriptionProfileHandlerResolver>();
            services.ConnectImplementationsToTypesClosing(typeof(ISubscriptionEventHandler<,>), assemblies, false);
            services.AddTransient(typeof(IEventRepository<>), typeof(EventRepository<>));
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
                        var resolver = applicationBuilder.ApplicationServices.GetRequiredService<ISusbcriptionProfileHandlerResolver>();
                        var profileType = profile.GetType();
                        var observerType = typeof(SubscriptionObserver<,>).MakeGenericType(profileType, eventType);
                        var observer = (IObserver<Event>)Activator.CreateInstance(observerType, resolver);
                        subscription.Subscribe(observer);
                    }

                    var connectionSettings = new TransientStreamSubscriptionConnectionSettings(profile.StartPosition, profile.Filter);
                    var subId = Guid.NewGuid();
                    manager.Add(subId, subscription, connectionSettings);
                    manager.Connect(subId);
                }
            }


            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices<TransientSubscriptionProfile>();
                foreach (var profile in profiles.Where(x => x.GetType() != typeof(SubscriptionManagerProfile)))
                {
                    //Fetch a new instance persistent subscription from the IoC container
                    var subscription = applicationBuilder.ApplicationServices.GetRequiredService<ITransientStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var resolver = applicationBuilder.ApplicationServices.GetRequiredService<ISusbcriptionProfileHandlerResolver>();
                        var profileType = profile.GetType();
                        var observerType = typeof(SubscriptionObserver<,>).MakeGenericType(profileType, eventType);
                        var observer = (IObserver<Event>)Activator.CreateInstance(observerType, resolver);
                        subscription.Subscribe(observer);
                    }

                    var connectionSettings = new TransientStreamSubscriptionConnectionSettings(profile.StartPosition, profile.Filter);
                    var sub = subscriptions.FirstOrDefault(x => x.Name == profile.GetType().Name);
                    if(sub == default)
                    {
                        var subscriptionBase = rootManager.CreateTransientSubscription(profile.GetType().Name).Result;
                        rootManager.StartSubscription(subscriptionBase.SubscriptionId);
                    }
                    else
                    {
                        manager.Add(sub.SubscriptionId, subscription, connectionSettings);
                        if (sub.Connected)
                            manager.Connect(sub.SubscriptionId);
                    }

                    
                }
            }

            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
                var profiles = scope.ServiceProvider.GetServices<PersistentSubscriptionProfile>();
                foreach (var profile in profiles)
                {
                    //Fetch a new instance persistent subscription from the IoC container
                    var subscription = applicationBuilder.ApplicationServices.GetRequiredService<IPersistentStreamSubscriptionConnection>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var resolver = applicationBuilder.ApplicationServices.GetRequiredService<ISusbcriptionProfileHandlerResolver>();
                        var profileType = profile.GetType();
                        var observerType = typeof(SubscriptionObserver<,>).MakeGenericType(profileType, eventType);
                        var observer = (IObserver<Event>)Activator.CreateInstance(observerType, resolver);
                        subscription.Subscribe(observer);
                    }

                    var connectionSettings = new PersistentStreamSubscriptionConnectionSettings(profile.GetType().Name, profile.Filter);
                    var sub = subscriptions.FirstOrDefault(x => x.Name == profile.GetType().Name);
                    if (sub == default)
                    {
                        var subscriptionBase = rootManager.CreatePersistentSubscription(profile.GetType().Name).Result;
                        rootManager.StartSubscription(subscriptionBase.SubscriptionId);
                    }
                    else
                    {
                        manager.Add(sub.SubscriptionId, subscription, connectionSettings);
                        if (sub.Connected)
                            manager.Connect(sub.SubscriptionId);
                    }
                }
            }

        }
    }
}
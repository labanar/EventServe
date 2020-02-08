using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Transient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventServe.Extensions.Microsoft.DependencyInjection
{
    public static class EventServeExtensions
    {
        public static void AddEventServeCore(this IServiceCollection services, Assembly[] assemblies)
        {
            services.RegisterAllTypesWithBaseType<PersistentSubscriptionProfile>(assemblies, ServiceLifetime.Singleton);
            services.RegisterAllTypesWithBaseType<TransientSubscriptionProfile>(assemblies, ServiceLifetime.Singleton);
            services.AddTransient<ISusbcriptionProfileHandlerResolver, SubscriptionProfileHandlerResolver>();
            services.ConnectImplementationsToTypesClosing(typeof(ISubscriptionEventHandler<,>), assemblies, false);
        }

        public static void UseEventServe(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.RegisterEventServeSubscriptions();
        }

        public static void RegisterEventServeSubscriptions(this IApplicationBuilder applicationBuilder)
        {
            var pSubs = new List<IPersistentStreamSubscription>();
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var profiles = scope.ServiceProvider.GetServices<PersistentSubscriptionProfile>();
                foreach (var profile in profiles)
                {
                    //Fetch a new instance persistent subscription from the IoC container
                    var subscription = applicationBuilder.ApplicationServices.GetRequiredService<IPersistentStreamSubscription>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var resolver = applicationBuilder.ApplicationServices.GetRequiredService<ISusbcriptionProfileHandlerResolver>();
                        var profileType = profile.GetType();
                        var observerType = typeof(SubscriptionObserver<,>).MakeGenericType(profileType, eventType);
                        var observer = (IObserver<Event>)Activator.CreateInstance(observerType, resolver);
                        subscription.Subscribe(observer);
                    }

                    subscription.Start(profile.GetType().Name, profile.Filter).Wait();
                    pSubs.Add(subscription);
                }
            }


            var tSubs = new List<ITransientStreamSubscription>();
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var profiles = scope.ServiceProvider.GetServices<TransientSubscriptionProfile>();
                foreach (var profile in profiles)
                {
                    //Fetch a new instance persistent subscription from the IoC container
                    var subscription = applicationBuilder.ApplicationServices.GetRequiredService<ITransientStreamSubscription>();
                    foreach (var eventType in profile.SubscribedEvents)
                    {
                        var resolver = applicationBuilder.ApplicationServices.GetRequiredService<ISusbcriptionProfileHandlerResolver>();
                        var profileType = profile.GetType();
                        var observerType = typeof(SubscriptionObserver<,>).MakeGenericType(profileType, eventType);
                        var observer = (IObserver<Event>)Activator.CreateInstance(observerType, resolver);
                        subscription.Subscribe(observer);
                    }

                    subscription.Start(profile.StartPosition, profile.Filter).Wait();
                    tSubs.Add(subscription);
                }
            }
        }
    }
}

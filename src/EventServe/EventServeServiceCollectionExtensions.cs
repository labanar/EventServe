﻿using EventServe.Services;
using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using EventServe.Subscriptions.Transient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventServe
{
    public static class EventServeServiceCollectionExtensions
    {
        public static void UseEventServeCore(this IServiceCollection services, Assembly[] assemblies)
        {
            services.ConnectImplementationsToTypesClosing(typeof(IStreamSubscriptionEventHandler<,>), assemblies, false);
            services.ConnectImplementationsToTypesClosing(typeof(PersistentSubscriptionProfile<>), assemblies, false, ServiceLifetime.Singleton);
            services.ConnectImplementationsToTypesClosing(typeof(TransientSubscriptionProfile<>), assemblies, false, ServiceLifetime.Singleton);
            services.AddTransient(typeof(ITransientSubscriptionBuilder<>), typeof(TransientSubscriptionBuilder<>));
            services.AddTransient(typeof(IPersistentSubscriptionBuilder<>), typeof(PersistentSubscriptionBuilder<>));
            services.AddTransient<ISubscriptionHandlerResolver, SubscriptionHandlerResolver>();
        }

        private static void ConnectImplementationsToTypesClosing(this IServiceCollection services, Type openRequestInterface,        
           IEnumerable<Assembly> assembliesToScan,
           bool addIfAlreadyExists,
           ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var concretions = new List<Type>();
            var interfaces = new List<Type>();
            foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => !t.IsOpenGeneric()))
            {
                var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();
                if (!interfaceTypes.Any()) continue;

                if (type.IsConcrete())
                {
                    concretions.Add(type);
                }

                foreach (var interfaceType in interfaceTypes)
                {
                    interfaces.Fill(interfaceType);
                }
            }

            foreach (var @interface in interfaces)
            {
                var exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
                if (addIfAlreadyExists)
                {
                    foreach (var type in exactMatches)
                    {
                        switch(lifetime)
                        {
                            case ServiceLifetime.Transient: services.AddTransient(@interface, type); break;
                            case ServiceLifetime.Scoped: services.AddScoped(@interface, type); break;
                            case ServiceLifetime.Singleton: services.AddSingleton(@interface, type); break;
                        }
                    }
                }
                else
                {
                    if (exactMatches.Count > 1)
                    {
                        exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
                    }

                    foreach (var type in exactMatches)
                    {
                        services.TryAddTransient(@interface, type);
                    }
                }

                if (!@interface.IsOpenGeneric())
                {
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services, lifetime);
                }
            }
        }

        private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
        {
            if (handlerType == null || handlerInterface == null)
            {
                return false;
            }

            if (handlerType.IsInterface)
            {
                if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                {
                    return true;
                }
            }
            else
            {
                return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return false;
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services, ServiceLifetime lifetime)
        {
            foreach (var type in concretions
                .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
            {
                try
                {
                    switch (lifetime)
                    {
                        case ServiceLifetime.Transient: services.TryAddTransient(@interface, type); break;
                        case ServiceLifetime.Scoped: services.TryAddScoped(@interface, type); break;
                        case ServiceLifetime.Singleton: services.TryAddSingleton(@interface, type); break;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null) return false;

            if (pluggedType == pluginType) return true;

            return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
        }

        public static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
        {
            if (pluggedType == null) yield break;

            if (!pluggedType.IsConcrete()) yield break;

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                    pluggedType.GetInterfaces()
                        .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     (pluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType))
            {
                yield return pluggedType.GetTypeInfo().BaseType;
            }

            if (pluggedType.GetTypeInfo().BaseType == typeof(object)) yield break;

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }

        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value)) return;
            list.Add(value);
        }
    }
}
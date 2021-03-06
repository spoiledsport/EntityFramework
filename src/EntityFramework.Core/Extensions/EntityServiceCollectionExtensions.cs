// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Remotion.Linq.Parsing.Structure;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class EntityServiceCollectionExtensions
    {
        private const int ConfigurationOrder = -1000; // OptionsConstants is internal.

        public static EntityServicesBuilder AddEntityFramework(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] IConfiguration configuration = null)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            // TODO: Is this the appropriate way to register listeners?
            serviceCollection
                .AddScoped<IEntityStateListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IRelationshipListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IPropertyListener>(p => p.GetService<ChangeDetector>());

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryStringValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<GuidValueGenerator>>()
                .AddSingleton<EntityAttacherFactory>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<DbSetSource>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<CompositeEntityKeyFactory>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<CollectionTypeFactory>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<MemberMapper>()
                .AddSingleton<FieldMatcher>()
                .AddSingleton<OriginalValuesFactory>()
                .AddSingleton<RelationshipsSnapshotFactory>()
                .AddSingleton<StoreGeneratedValuesFactory>()
                .AddSingleton<StateEntryMetadataServices>()
                .AddSingleton<IQueryParser, EntityQueryParser>()
                .AddSingleton<ICompiledQueryCache, CompiledQueryCache>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddTypeActivator()
                .AddScoped<ForeignKeyValuePropagator>()
                .AddScoped<NavigationFixer>()
                .AddScoped<StateManager>()
                .AddScoped<StateEntryFactory>()
                .AddScoped<StateEntryNotifier>()
                .AddScoped<StateEntrySubscriber>()
                .AddScoped<ValueGenerationManager>()
                .AddScoped<EntityQueryExecutor>()
                .AddScoped<EntityQueryProvider>()
                .AddScoped<ChangeTracker>()
                .AddScoped<ChangeDetector>()
                .AddScoped<EntityEntryGraphIterator>()
                .AddScoped<DbContextServices>()
                .AddScoped(DbContextServices.ModelFactory)
                .AddScoped(DbContextServices.ContextFactory)
                .AddScoped(DbContextServices.ContextOptionsFactory)
                .AddScoped<DataStoreSelector>()
                .AddScoped(DataStoreServices.DataStoreServicesFactory)
                .AddScoped(DataStoreServices.DataStoreFactory)
                .AddScoped(DataStoreServices.ConnectionFactory)
                .AddScoped(DataStoreServices.DatabaseFactory)
                .AddScoped(DataStoreServices.ValueGeneratorCacheFactory)
                .AddScoped(DataStoreServices.DataStoreCreatorFactory)
                .AddScoped(DataStoreServices.ModelBuilderFactoryFactory)
                .AddOptions());

            return new EntityServicesBuilder(serviceCollection, configuration);
        }

        public static EntityServicesBuilder AddDbContext<TContext>(
            [NotNull] this EntityServicesBuilder builder,
            [CanBeNull] Action<DbContextOptions> optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection.AddSingleton(
                sp => sp.GetRequiredServiceChecked<IOptions<DbContextOptions<TContext>>>().Options);

            if (builder.Configuration != null)
            {
                // TODO: Allows parser to be obtained from service provider. Issue #947
                builder.ServiceCollection.ConfigureOptions(
                    new DbContextConfigureOptions<TContext>(builder.Configuration, new DbContextOptionsParser())
                        {
                            Order = ConfigurationOrder
                        });
            }

            if (optionsAction != null)
            {
                builder.ServiceCollection.Configure<DbContextOptions<TContext>>(optionsAction);
            }

            ServiceCollectionExtensions.AddScoped(builder.ServiceCollection, typeof(TContext), DbContextActivator.CreateInstance<TContext>);

            return builder;
        }
    }
}

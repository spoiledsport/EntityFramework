// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDatabase : Database, IAccessor<Migrator>
    {
        private readonly Migrator _migrator;

        protected RelationalDatabase(
            [NotNull] DbContextService<DbContext> context,
            [NotNull] RelationalDataStoreCreator dataStoreCreator,
            [NotNull] RelationalConnection connection,
            [NotNull] Migrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(context, dataStoreCreator, connection, loggerFactory)
        {
            Check.NotNull(migrator, nameof(context));

            _migrator = migrator;
        }

        public virtual void ApplyMigrations()
        {
            _migrator.ApplyMigrations();
        }

        Migrator IAccessor<Migrator>.Service => _migrator;

        public new virtual RelationalConnection Connection => (RelationalConnection)base.Connection;

        public virtual void Create()
        {
            Logger.CreatingDatabase(Connection.DbConnection.Database);

            RelationalDataStoreCreator.Create();
        }

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.CreatingDatabase(Connection.DbConnection.Database);

            return RelationalDataStoreCreator.CreateAsync(cancellationToken);
        }

        public virtual void CreateTables()
        {
            RelationalDataStoreCreator.CreateTables(Model);
        }

        public virtual Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.CreateTablesAsync(Model, cancellationToken);
        }

        public virtual void Delete()
        {
            RelationalDataStoreCreator.Delete();
        }

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.DeleteAsync(cancellationToken);
        }

        public virtual bool Exists()
        {
            return RelationalDataStoreCreator.Exists();
        }

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.ExistsAsync(cancellationToken);
        }

        public virtual bool HasTables()
        {
            return RelationalDataStoreCreator.HasTables();
        }

        public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.HasTablesAsync(cancellationToken);
        }

        private RelationalDataStoreCreator RelationalDataStoreCreator => (RelationalDataStoreCreator)((IAccessor<DataStoreCreator>)this).Service;

        private ILogger Logger => ((IAccessor<ILogger>)this).Service;

        private IModel Model => ((IAccessor<IModel>)this).Service;
    }
}

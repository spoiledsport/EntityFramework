// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryConfigPatternsTest
    {
        [Fact]
        public void Can_save_and_query_with_implicit_services_and_OnConfiguring()
        {
            using (var context = new ImplicitServicesAndConfigBlogContext())
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                context.SaveChanges();
            }

            using (var context = new ImplicitServicesAndConfigBlogContext())
            {
                var blog = context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);

                context.Blogs.Remove(context.Blogs.ToArray());
                context.SaveChanges();

                Assert.Empty(context.Blogs);
            }
        }

        private class ImplicitServicesAndConfigBlogContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();
            }
        }

        [Fact]
        public void Can_save_and_query_with_implicit_services_and_explicit_config()
        {
            var options = new DbContextOptions();
            options.UseInMemoryStore();

            using (var context = new ImplicitServicesExplicitConfigBlogContext(options))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                context.SaveChanges();
            }

            using (var context = new ImplicitServicesExplicitConfigBlogContext(options))
            {
                var blog = context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);

                context.Blogs.Remove(context.Blogs.ToArray());
                context.SaveChanges();

                Assert.Empty(context.Blogs);
            }
        }

        private class ImplicitServicesExplicitConfigBlogContext : DbContext
        {
            public ImplicitServicesExplicitConfigBlogContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_save_and_query_with_explicit_services_and_OnConfiguring()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            var serviceProvider = services.BuildServiceProvider();

            using (var context = new ExplicitServicesImplicitConfigBlogContext(serviceProvider))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                context.SaveChanges();
            }

            using (var context = new ExplicitServicesImplicitConfigBlogContext(serviceProvider))
            {
                var blog = context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);

                context.Blogs.Remove(context.Blogs.ToArray());
                context.SaveChanges();

                Assert.Empty(context.Blogs);
            }
        }

        private class ExplicitServicesImplicitConfigBlogContext : DbContext
        {
            public ExplicitServicesImplicitConfigBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();
            }
        }

        [Fact]
        public void Can_save_and_query_with_explicit_services_and_explicit_config()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            var serviceProvider = services.BuildServiceProvider();

            var options = new DbContextOptions();
            options.UseInMemoryStore();

            using (var context = new ExplicitServicesAndConfigBlogContext(serviceProvider, options))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                context.SaveChanges();
            }

            using (var context = new ExplicitServicesAndConfigBlogContext(serviceProvider, options))
            {
                var blog = context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);

                context.Blogs.Remove(context.Blogs.ToArray());
                context.SaveChanges();

                Assert.Empty(context.Blogs);
            }
        }

        private class ExplicitServicesAndConfigBlogContext : DbContext
        {
            public ExplicitServicesAndConfigBlogContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_save_and_query_with_explicit_services_and_no_config()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            var serviceProvider = services.BuildServiceProvider();

            using (var context = new ExplicitServicesAndNoConfigBlogContext(serviceProvider))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                context.SaveChanges();
            }

            using (var context = new ExplicitServicesAndNoConfigBlogContext(serviceProvider))
            {
                var blog = context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);

                context.Blogs.Remove(context.Blogs.ToArray());
                context.SaveChanges();

                Assert.Empty(context.Blogs);
            }
        }

        private class ExplicitServicesAndNoConfigBlogContext : DbContext
        {
            public ExplicitServicesAndNoConfigBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Throws_on_attempt_to_use_context_with_no_store()
        {
            Assert.Equal(
                CoreStrings.NoDataStoreConfigured,
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var context = new NoServicesAndNoConfigBlogContext())
                    {
                        context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                        context.SaveChanges();
                    }
                }).Message);
        }

        private class NoServicesAndNoConfigBlogContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Throws_on_attempt_to_use_store_with_no_store_services()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Equal(
                CoreStrings.NoDataStoreService,
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var context = new ImplicitConfigButNoServicesBlogContext(serviceProvider))
                    {
                        context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                        context.SaveChanges();
                    }
                }).Message);
        }

        private class ImplicitConfigButNoServicesBlogContext : DbContext
        {
            public ImplicitConfigButNoServicesBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();
            }
        }

        [Fact]
        public void Can_register_context_with_DI_container_and_have_it_injected()
        {
            var services = new ServiceCollection();
            services.AddTransient<InjectContextBlogContext>()
                .AddTransient<InjectContextController>()
                .AddEntityFramework()
                .AddInMemoryStore();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<InjectContextController>().Test();
        }

        private class InjectContextController
        {
            private readonly InjectContextBlogContext _context;

            public InjectContextController(InjectContextBlogContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectContextBlogContext : DbContext
        {
            public InjectContextBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                Assert.NotNull(serviceProvider);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
        {
            var options = new DbContextOptions();
            options.UseInMemoryStore();

            var services = new ServiceCollection();
            services.AddTransient<InjectContextAndConfigurationBlogContext>()
                .AddTransient<InjectContextAndConfigurationController>()
                .AddInstance(options)
                .AddEntityFramework()
                .AddInMemoryStore();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<InjectContextAndConfigurationController>().Test();
        }

        private class InjectContextAndConfigurationController
        {
            private readonly InjectContextAndConfigurationBlogContext _context;

            public InjectContextAndConfigurationController(InjectContextAndConfigurationBlogContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectContextAndConfigurationBlogContext : DbContext
        {
            public InjectContextAndConfigurationBlogContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
                Assert.NotNull(serviceProvider);
                Assert.NotNull(options);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        // This one is a bit strange because the context gets the configuration from the service provider
        // but doesn't get the service provider and so creates a new one for use internally. This works fine
        // although it would be much more common to inject both when using DI explicitly.
        [Fact]
        public void Can_register_configuration_with_DI_container_and_have_it_injected()
        {
            var options = new DbContextOptions();
            options.UseInMemoryStore(persist: false);

            var services = new ServiceCollection();
            services.AddTransient<InjectConfigurationBlogContext>()
                .AddTransient<InjectConfigurationController>()
                .AddInstance(options)
                .AddEntityFramework()
                .AddInMemoryStore();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<InjectConfigurationController>().Test();
        }

        private class InjectConfigurationController
        {
            private readonly InjectConfigurationBlogContext _context;

            public InjectConfigurationController(InjectConfigurationBlogContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectConfigurationBlogContext : DbContext
        {
            public InjectConfigurationBlogContext(DbContextOptions options)
                : base(options)
            {
                Assert.NotNull(options);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_inject_different_configurations_into_different_contexts()
        {
            var blogOptions = new DbContextOptions<InjectDifferentConfigurationsBlogContext>();
            blogOptions.UseInMemoryStore();

            var accountOptions = new DbContextOptions<InjectDifferentConfigurationsAccountContext>();
            accountOptions.UseInMemoryStore();

            var services = new ServiceCollection();
            services.AddTransient<InjectDifferentConfigurationsBlogContext>()
                .AddTransient<InjectDifferentConfigurationsAccountContext>()
                .AddTransient<InjectDifferentConfigurationsBlogController>()
                .AddTransient<InjectDifferentConfigurationsAccountController>()
                .AddInstance(blogOptions)
                .AddInstance(accountOptions)
                .AddEntityFramework()
                .AddInMemoryStore();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<InjectDifferentConfigurationsBlogController>().Test();
            serviceProvider.GetRequiredService<InjectDifferentConfigurationsAccountController>().Test();
        }

        private class InjectDifferentConfigurationsBlogController
        {
            private readonly InjectDifferentConfigurationsBlogContext _context;

            public InjectDifferentConfigurationsBlogController(InjectDifferentConfigurationsBlogContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                Assert.IsType<DbContextOptions<InjectDifferentConfigurationsBlogContext>>(
                    ((IAccessor<IServiceProvider>)_context).Service.GetRequiredService<DbContextService<IDbContextOptions>>().Service);

                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectDifferentConfigurationsAccountController
        {
            private readonly InjectDifferentConfigurationsAccountContext _context;

            public InjectDifferentConfigurationsAccountController(InjectDifferentConfigurationsAccountContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                Assert.IsType<DbContextOptions<InjectDifferentConfigurationsAccountContext>>(
                    ((IAccessor<IServiceProvider>)_context).Service.GetRequiredService<DbContextService<IDbContextOptions>>().Service);

                _context.Accounts.Add(new Account { Name = "Eeky Bear" });
                _context.SaveChanges();

                var account = _context.Accounts.SingleOrDefault();

                Assert.Equal(1, account.Id);
                Assert.Equal("Eeky Bear", account.Name);
            }
        }

        private class InjectDifferentConfigurationsBlogContext : DbContext
        {
            public InjectDifferentConfigurationsBlogContext(IServiceProvider serviceProvider, DbContextOptions<InjectDifferentConfigurationsBlogContext> options)
                : base(serviceProvider, options)
            {
                Assert.NotNull(serviceProvider);
                Assert.NotNull(options);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        private class InjectDifferentConfigurationsAccountContext : DbContext
        {
            public InjectDifferentConfigurationsAccountContext(IServiceProvider serviceProvider, DbContextOptions<InjectDifferentConfigurationsAccountContext> options)
                : base(serviceProvider, options)
            {
                Assert.NotNull(serviceProvider);
                Assert.NotNull(options);
            }

            public DbSet<Account> Accounts { get; set; }
        }

        [Fact]
        public void Can_inject_different_configurations_into_different_contexts_without_declaring_in_constructor()
        {
            var blogOptions = new DbContextOptions<InjectDifferentConfigurationsNoConstructorBlogContext>();
            blogOptions.UseInMemoryStore();

            var accountOptions = new DbContextOptions<InjectDifferentConfigurationsNoConstructorAccountContext>();
            accountOptions.UseInMemoryStore();

            var services = new ServiceCollection();
            services.AddTransient<InjectDifferentConfigurationsNoConstructorBlogContext>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorAccountContext>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorBlogController>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorAccountController>()
                .AddInstance(blogOptions)
                .AddInstance(accountOptions)
                .AddEntityFramework()
                .AddInMemoryStore();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<InjectDifferentConfigurationsNoConstructorBlogController>().Test();
            serviceProvider.GetRequiredService<InjectDifferentConfigurationsNoConstructorAccountController>().Test();
        }

        private class InjectDifferentConfigurationsNoConstructorBlogController
        {
            private readonly InjectDifferentConfigurationsNoConstructorBlogContext _context;

            public InjectDifferentConfigurationsNoConstructorBlogController(InjectDifferentConfigurationsNoConstructorBlogContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                Assert.IsType<DbContextOptions<InjectDifferentConfigurationsNoConstructorBlogContext>>(
                    ((IAccessor<IServiceProvider>)_context).Service.GetRequiredService<DbContextService<IDbContextOptions>>().Service);

                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectDifferentConfigurationsNoConstructorAccountController
        {
            private readonly InjectDifferentConfigurationsNoConstructorAccountContext _context;

            public InjectDifferentConfigurationsNoConstructorAccountController(InjectDifferentConfigurationsNoConstructorAccountContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                Assert.IsType<DbContextOptions<InjectDifferentConfigurationsNoConstructorAccountContext>>(
                    ((IAccessor<IServiceProvider>)_context).Service.GetRequiredService<DbContextService<IDbContextOptions>>().Service);

                _context.Accounts.Add(new Account { Name = "Eeky Bear" });
                _context.SaveChanges();

                var account = _context.Accounts.SingleOrDefault();

                Assert.Equal(1, account.Id);
                Assert.Equal("Eeky Bear", account.Name);
            }
        }

        private class InjectDifferentConfigurationsNoConstructorBlogContext : DbContext
        {
            public InjectDifferentConfigurationsNoConstructorBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                Assert.NotNull(serviceProvider);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        private class InjectDifferentConfigurationsNoConstructorAccountContext : DbContext
        {
            public InjectDifferentConfigurationsNoConstructorAccountContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                Assert.NotNull(serviceProvider);
            }

            public DbSet<Account> Accounts { get; set; }
        }

        private class Account
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}

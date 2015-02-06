﻿using System;
using Autofac.Builder;
using Xunit;
using Autofac.Core;
using Autofac.Test.Scenarios.Dependencies;

namespace Autofac.Test.Core.Resolving
{
    public class ResolveOperationTests
    {
        [Fact]
        public void CtorPropDependencyOkOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleInstance();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            var dbp = c.Resolve<DependsByProp>();

            Assert.NotNull(dbp.Dep);
            Assert.NotNull(dbp.Dep.Dep);
            Assert.Same(dbp, dbp.Dep.Dep);
        }

        [Fact]
        public void CtorPropDependencyOkOrder2()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>().SingleInstance();
            cb.RegisterType<DependsByProp>().SingleInstance().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            var dbc = c.Resolve<DependsByCtor>();

            Assert.NotNull(dbc.Dep);
            Assert.NotNull(dbc.Dep.Dep);
            Assert.Same(dbc, dbc.Dep.Dep);
        }

        [Fact]
        public void CtorPropDependencyFactoriesOrder1()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<DependsByCtor>();
            cb.RegisterType<DependsByProp>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            Assert.Throws<DependencyResolutionException>(() => c.Resolve<DependsByProp>());
        }

        [Fact]
        public void CtorPropDependencyFactoriesOrder2()
        {
            var cb = new ContainerBuilder();
            var ac = 0;
            // ReSharper disable AccessToModifiedClosure
            cb.RegisterType<DependsByCtor>().OnActivating(e => { ++ac; });
            // ReSharper restore AccessToModifiedClosure
            cb.RegisterType<DependsByProp>().OnActivating(e => { ++ac; })
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            var c = cb.Build();
            Assert.Throws<DependencyResolutionException>(() => c.Resolve<DependsByCtor>());

            Assert.Equal(2, ac);
        }


        [Fact]
        public void ActivatingArgsSuppliesParameters()
        {
            const int provided = 12;
            var passed = 0;

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .OnActivating(e => passed = e.Parameters.TypedAs<int>());
            var container = builder.Build();

            container.Resolve<object>(TypedParameter.From(provided));
            Assert.Equal(provided, passed);
        }

        [Fact]
        public void ActivatedArgsSuppliesParameters()
        {
            const int provided = 12;
            var passed = 0;

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .OnActivated(e => passed = e.Parameters.TypedAs<int>());
            var container = builder.Build();

            container.Resolve<object>(TypedParameter.From(provided));
            Assert.Equal(provided, passed);
        }

        [Fact]
        public void ChainedOnActivatedEventsAreInvokedWithinASingleResolveOperation()
        {
            var builder = new ContainerBuilder();

            bool secondEventRaised = false;
            builder.RegisterType<object>()
                .Named<object>("second")
                .OnActivated(e => secondEventRaised = true);

            builder.RegisterType<object>()
                .OnActivated(e => e.Context.ResolveNamed<object>("second"));

            var container = builder.Build();
            container.Resolve<object>();

            Assert.True(secondEventRaised);
        }

        [Fact]
        public void AfterTheOperationIsFinished_ReusingTheTemporaryContextThrows()
        {
            IComponentContext ctx = null;
            var builder = new ContainerBuilder();
            builder.Register(c => { ctx = c; return new object(); });
            builder.RegisterInstance("Hello");
            var container = builder.Build();
            container.Resolve<string>();
            container.Resolve<object>();
            Assert.Throws<ObjectDisposedException>(() => ctx.Resolve<string>());
        }
    }
}

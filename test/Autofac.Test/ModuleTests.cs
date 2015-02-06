﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Test
{
    public class ModuleTests
    {
        class ObjectModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterInstance(new object());
            }
        }

        [Fact]
        public void LoadsRegistrations()
        {
            var cr = new ComponentRegistry();
            new ObjectModule().Configure(cr);
            Assert.True(cr.IsRegistered(new TypedService(typeof(object))));
        }

        [Fact]
        public void DetectsNullComponentRegistryArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectModule().Configure(null));
        }

        class AttachingModule : Module
        {
            public IList<IComponentRegistration> Registrations = new List<IComponentRegistration>();

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                base.AttachToComponentRegistration(componentRegistry, registration);
                Registrations.Add(registration);
            }
        }

        [Fact]
        public void AttachesToRegistrations()
        {
            var attachingModule = new AttachingModule();
            Assert.Equal(0, attachingModule.Registrations.Count);

            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(object));
            builder.RegisterModule(attachingModule);
            builder.RegisterInstance("Hello!");

            var container = builder.Build();

            Assert.Equal(container.ComponentRegistry.Registrations.Count(), attachingModule.Registrations.Count);
        }

        class ModuleExposingThisAssembly : Module
        {
            public Assembly ModuleThisAssembly { get { return ThisAssembly; }}
        }

        [Fact]
        public void TheAssemblyExposedByThisAssemblyIsTheOneContainingTheConcreteModuleClass()
        {
            var module = new ModuleExposingThisAssembly();
            Assert.Same(typeof(ModuleExposingThisAssembly).Assembly, module.ModuleThisAssembly);
        }

        class ModuleIndirectlyExposingThisAssembly : ModuleExposingThisAssembly
        {
        }

        [Fact]
        public void IndirectlyDerivedModulesCannotUseThisAssembly()
        {
            var module = new ModuleIndirectlyExposingThisAssembly();
            Assert.Throws<InvalidOperationException>(() => { var unused = module.ModuleThisAssembly; });
        }
    }
}

﻿using System;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Metadata;
using Autofac.Util;
using Xunit;

namespace Autofac.Test.Features.Metadata
{
    public class StronglyTypedMeta_WhenMetadataIsSupplied
    {
        const int SuppliedIntValue = 123;
        const string SuppliedNameValue = "Homer";

        IContainer _container;

        public StronglyTypedMeta_WhenMetadataIsSupplied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .WithMetadata("TheInt", SuppliedIntValue)
                .WithMetadata("Name", SuppliedNameValue);
            _container = builder.Build();
        }

        [Fact]
        public void ValuesAreProvidedFromMetadata()
        {
            var meta = _container.Resolve<Meta<object, MyMeta>>();
            Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
        }

        [Fact]
        public void ValuesProvidedAreUniqueToEachRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().WithMetadata("TheInt", 123);
            builder.RegisterType<string>().WithMetadata("TheInt", 321);
            _container = builder.Build();

            var meta1 = _container.Resolve<Meta<object, MyMeta>>();
            Assert.Equal(123, meta1.Metadata.TheInt);

            var meta2 = _container.Resolve<Meta<string, MyMeta>>();
            Assert.Equal(321, meta2.Metadata.TheInt);
        }

        [Fact]
        public void ValuesProvidedFromMetadataOverrideDefaults()
        {
            var meta = _container.Resolve<Meta<object, MyMetaWithDefault>>();
            Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
        }

        [Fact]
        public void ValuesProvidedToTypesWithDictionaryConstructor()
        {
            var meta = _container.Resolve<Meta<object, MyMetaWithDictionary>>();
            Assert.Equal(SuppliedNameValue, meta.Metadata.TheName);
        }

        [Fact]
        public void ReadOnlyPropertiesOnMetadataViewAreIgnored()
        {
            var meta = _container.Resolve<Meta<object, MyMetaWithReadOnlyProperty>>();
            Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithInvalidConstructorThrowsException()
        {
            var exception = Assert.Throws<DependencyResolutionException>(
                () => _container.Resolve<Meta<object, MyMetaWithInvalidConstructor>>());

            var typeName = typeof(MyMetaWithInvalidConstructor).Name;
            var message = string.Format(MetadataViewProviderResources.InvalidViewImplementation, typeName);

            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithInterfaceThrowsException()
        {
            Assert.Throws<ComponentNotRegisteredException>(
                () => _container.Resolve<Meta<object, IMyMetaInterface>>());
        }

        [Fact]
        public void ValuesBubbleUpThroughAdapters()
        {
            var meta = _container.Resolve<Meta<Func<object>, MyMeta>>();
            Assert.Equal(SuppliedIntValue, meta.Metadata.TheInt);
        }
    }
    public class StronglyTypedMeta_WhenNoMatchingMetadataIsSupplied
    {
        IContainer _container;

        public StronglyTypedMeta_WhenNoMatchingMetadataIsSupplied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            _container = builder.Build();
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithoutDefaultValueThrowsException()
        {
            var exception = Assert.Throws<DependencyResolutionException>(
                () => _container.Resolve<Meta<object, MyMeta>>());

            var propertyName = ReflectionExtensions.GetProperty<MyMeta, int>(x => x.TheInt).Name;
            var message = string.Format(MetadataViewProviderResources.MissingMetadata, propertyName);

            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ResolvingStronglyTypedMetadataWithDefaultValueProvidesDefault()
        {
            var m = _container.Resolve<Meta<object, MyMetaWithDefault>>();
            Assert.Equal(42, m.Metadata.TheInt);
        }
    }
}
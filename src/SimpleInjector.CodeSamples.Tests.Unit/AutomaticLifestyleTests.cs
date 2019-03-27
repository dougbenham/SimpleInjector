namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Lifestyles;

    [TestClass]
    public class AutomaticLifestyleTests
    {
        private static readonly Lifestyle Automatic = new AutomaticLifestyle();
        private static readonly ScopedLifestyle Scoped = new AsyncScopedLifestyle();

        [TestMethod]
        public void Registration_TypeWithoutDependencies_Singleton()
        {
            // Arrange
            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<NoDependencies2>();
            container.Verify();

            // Act
            var reg = GetRegistration<NoDependencies2>(container);

            // Assert
            Assert.AreSame(Lifestyle.Singleton, reg.Lifestyle, $"Actual: {reg.Lifestyle.Name}");
        }

        [TestMethod]
        public void Registration_TypeWithAllSingletonDependencies_Singleton()
        {
            // Arrange
            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<DependingOn<NoDependencies1, NoDependencies2>>();

            container.Register<NoDependencies1>(Lifestyle.Singleton);
            container.Register<NoDependencies2>(Lifestyle.Singleton);

            container.Verify();

            // Act
            var reg = GetRegistration<DependingOn<NoDependencies1, NoDependencies2>>(container);

            // Assert
            Assert.AreSame(Lifestyle.Singleton, reg.Lifestyle, $"Actual: {reg.Lifestyle.Name}");
        }

        [TestMethod]
        public void Registration_TypeWithMixedLifestyles1_ShortestLifestyle()
        {
            // Arrange
            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<DependingOn<NoDependencies1, NoDependencies2>>();

            container.Register<NoDependencies1>(Lifestyle.Singleton);
            container.Register<NoDependencies2>(Lifestyle.Scoped);

            container.Verify();

            // Act
            var reg = GetRegistration<DependingOn<NoDependencies1, NoDependencies2>>(container);

            // Assert
            Assert.AreSame(Scoped, reg.Lifestyle, $"Actual: {reg.Lifestyle.Name}");
        }

        [TestMethod]
        public void Registration_TypeWithMixedLifestyles2_ShortestLifestyle()
        {
            // Arrange
            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<DependingOn<NoDependencies1, NoDependencies2>>();

            container.Register<NoDependencies1>(Lifestyle.Scoped);
            container.Register<NoDependencies2>(Lifestyle.Singleton);

            container.Verify();

            // Act
            var registration = GetRegistration<DependingOn<NoDependencies1, NoDependencies2>>(container);

            // Assert
            Assert.AreSame(container.Options.DefaultScopedLifestyle, registration.Lifestyle);
        }

        [TestMethod]
        public void Registration_TypeWithMixedLifestyles3_ShortestLifestyle()
        {
            // Arrange
            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<DependingOn<NoDependencies1, NoDependencies2>>();

            container.Register<NoDependencies1>(Lifestyle.Scoped);
            container.Register<NoDependencies2>(Lifestyle.Transient);

            container.Verify();

            // Act
            var registration = GetRegistration<DependingOn<NoDependencies1, NoDependencies2>>(container);

            // Assert
            Assert.AreSame(Lifestyle.Transient, registration.Lifestyle);
        }

        [TestMethod]
        public void Registration_TypeWithMixedLifestyles4_ShortestLifestyle()
        {
            // Arrange
            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<NoDependencies1>(Lifestyle.Transient);
            container.Register<NoDependencies2>(Lifestyle.Singleton);

            container.Register<DependingOn<NoDependencies1, NoDependencies2>>();
            container.Verify();

            // Act
            var registration = GetRegistration<DependingOn<NoDependencies1, NoDependencies2>>(container);

            // Assert
            Assert.AreSame(Lifestyle.Transient, registration.Lifestyle);
        }

        [TestMethod]
        public void Registration_DecoratorWithOnlySingletonDependencies_ProducesExpectedGraph()
        {
            // Arrange
            string expectedGraph =
@"DependingOn<IService>( // Singleton
    ServiceDecoratorDependingOn<NoDependencies1>( // Singleton
        DependingOn<NoDependencies1, NoDependencies2>( // Singleton
            NoDependencies1(), // Singleton
            NoDependencies2()), // Singleton
        NoDependencies1())) // Singleton";

            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<NoDependencies1>();
            container.Register<NoDependencies2>();
            container.Register<IService, DependingOn<NoDependencies1, NoDependencies2>>();
            container.RegisterDecorator<IService, ServiceDecoratorDependingOn<NoDependencies1>>();
            container.Register<DependingOn<IService>>();

            container.Verify();

            // Act
            var registration = GetRegistration<DependingOn<IService>>(container);

            // Assert
            Assert.AreEqual(expectedGraph, GetGraph(registration));
        }

        [TestMethod]
        public void Registration_DecoratorWithDecorateeWithOneScopedDependency_ProducesExpectedGraph()
        {
            // Arrange
            string expectedGraph =
@"DependingOn<IService>( // Async Scoped
    ServiceDecoratorDependingOn<NoDependencies1>( // Async Scoped
        DependingOn<NoDependencies1, NoDependencies2>( // Async Scoped
            NoDependencies1(), // Singleton
            NoDependencies2()), // Async Scoped
        NoDependencies1())) // Singleton";

            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<DependingOn<IService>>();
            container.RegisterDecorator<IService, ServiceDecoratorDependingOn<NoDependencies1>>();
            container.Register<IService, DependingOn<NoDependencies1, NoDependencies2>>();
            container.Register<NoDependencies2>(Lifestyle.Scoped);
            container.Register<NoDependencies1>();

            container.Verify();

            var registration = GetRegistration<DependingOn<IService>>(container);

            // Assert
            Assert.AreEqual(expectedGraph, GetGraph(registration));
        }

        [TestMethod]
        public void Registration_DecoratorWithSingletonDecorateeAndTransientDependency_ProducesExpectedGraph()
        {
            // Arrange
            string expectedGraph =
@"DependingOn<IService>( // Transient
    ServiceDecoratorDependingOn<NoDependencies2>( // Transient
        DependingOn<NoDependencies1, NoDependencies1>( // Singleton
            NoDependencies1(), // Singleton
            NoDependencies1()), // Singleton
        NoDependencies2())) // Transient";

            Container container = CreateContainerWithAutomicLifestyle();

            container.Register<DependingOn<IService>>();
            container.Register<IService, DependingOn<NoDependencies1, NoDependencies1>>();
            container.RegisterDecorator<IService, ServiceDecoratorDependingOn<NoDependencies2>>();
            container.Register<NoDependencies1>();
            container.Register<NoDependencies2>(Lifestyle.Transient);

            container.Verify();

            var registration = GetRegistration<DependingOn<IService>>(container);

            // Assert
            Assert.AreEqual(expectedGraph, GetGraph(registration));
        }

        private static Container CreateContainerWithAutomicLifestyle()
        {
            var container = new Container();
            container.Options.DefaultLifestyle = Automatic;
            container.Options.DefaultScopedLifestyle = Scoped;
            container.Options.ResolveUnregisteredConcreteTypes = false;
            return container;
        }

        private static InstanceProducer GetRegistration<T>(Container container) =>
            container.GetRegistration(typeof(T), throwOnFailure: true);

        private static string GetGraph(InstanceProducer producer) =>
            producer.VisualizeObjectGraph().Replace($"{nameof(AutomaticLifestyleTests)}.", string.Empty);

        public class DependingOn<T>
        {
            public DependingOn(T t)
            {
            }
        }

        public class IService { }

        public class DependingOn<T1, T2> : IService
        {
            public DependingOn(T1 t1, T2 t2)
            {
            }
        }

        public class ServiceDecoratorDependingOn<T> : IService
        {
            public ServiceDecoratorDependingOn(IService service, T t)
            {

            }
        }

        public interface INoDependencies { }

        public class NoDependencies1 : INoDependencies
        {
        }

        public class NoDependencies2
        {
        }
    }
}
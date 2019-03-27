namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Linq.Expressions;

    public sealed class AutomaticLifestyle : Lifestyle
    {
        internal static readonly AutomaticLifestyle Instance = new AutomaticLifestyle();

        public AutomaticLifestyle() : base("Automatic") { }

        public override int Length
        {
            get { throw new NotImplementedException(); }
        }

        protected override Registration CreateRegistrationCore<TConcrete>(Container container) =>
            new AutomaticRegistration<TConcrete>(container);

        protected override Registration CreateRegistrationCore<TService>(
            Func<TService> instanceCreator, Container container) =>
            throw new NotSupportedException(
                $"The {nameof(AutomaticLifestyle)} does not support Func<T> registrations. " +
                "Please register your Func<T> explicitly using the required lifestyle.");

        private sealed class AutomaticRegistration<TImplementation> : Registration
            where TImplementation : class
        {
            public AutomaticRegistration(Container container) : base(Instance, container)
            {
            }

            public override Type ImplementationType => typeof(TImplementation);

            public override Expression BuildExpression()
            {
                var expression = this.BuildTransientExpression();

                Lifestyle lifestyle = DetermineLifestyle();

                this.Lifestyle.CreateRegistration<TImplementation>(this.Container).BuildExpression();
            }

            private Lifestyle DetermineLifestyle()
            {
                Lifestyle shortestLifestyle = Singleton;

                foreach (var relationship in this.GetRelationships())
                {
                    var dependency = relationship.Dependency;

                    if (shortestLifestyle.Length > dependency.Lifestyle.Length)
                    {
                        shortestLifestyle = dependency.Lifestyle;
                    }
                }

                return shortestLifestyle;
            }

            private sealed class DummyRegistration : Registration
            {
                public DummyRegistration(Container container) : base(Transient, container)
                {
                }

                public override Type ImplementationType => typeof(TImplementation);
                public override Expression BuildExpression() => this.BuildTransientExpression();
            }
        }
    }
}
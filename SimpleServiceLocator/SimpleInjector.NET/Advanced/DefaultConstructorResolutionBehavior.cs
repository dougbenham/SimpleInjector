﻿#region Copyright (c) 2010 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2010 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Advanced
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay("{GetType().Name}")]
    internal sealed class DefaultConstructorResolutionBehavior : IConstructorResolutionBehavior
    {
        public ConstructorInfo GetConstructor(Type serviceType, Type implementationType)
        {
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementationType, "implementationType");

            VerifyTypeIsConcrete(implementationType);

            return GetSinglePublicConstructor(implementationType);
        }

        private static void VerifyTypeIsConcrete(Type implementationType)
        {
            if (implementationType.IsAbstract || implementationType.IsArray ||
                implementationType == typeof(object))
            {
                // About arrays: While array types are in fact concrete, we cannot create them and creating 
                // them would be pretty useless.
                // About object: System.Object is concrete and even contains a single public (default) 
                // constructor. Allowing it to be created however, would lead to confusion, since this allows
                // injecting System.Object into constructors, even though it is not registered explicitly.
                // This is bad, since creating an System.Object on the fly (transient) has no purpose and this
                // could lead to an accidentally valid container configuration, while there is in fact an
                // error in the configuration.
                throw new ActivationException(
                    StringResources.TypeShouldBeConcreteToBeUsedOnThisMethod(implementationType));
            }
        }

        private static ConstructorInfo GetSinglePublicConstructor(Type implementationType)
        {
            var constructors = implementationType.GetConstructors();

            bool hasSuitableConstructor = constructors.Length == 1;

            if (!hasSuitableConstructor)
            {
                throw new ActivationException(
                    StringResources.TypeMustHaveASinglePublicConstructor(implementationType));
            }

            return constructors[0];
        }
    }
}
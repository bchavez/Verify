﻿using System;
using System.Linq;
using System.Reflection;

namespace VerifyTests
{
    public partial class VerifySettings
    {
        object?[]? parameters;

        public void UseParameters(params object?[] parameters)
        {
            Guard.AgainstNullOrEmpty(parameters, nameof(parameters));
            this.parameters = parameters;
        }

        internal object?[] GetParameters(MethodInfo methodInfo)
        {
            var settingsParameters = parameters ?? Array.Empty<object?>();
            var methodParameters = methodInfo.GetParameters();
            if (!methodParameters.Any() || settingsParameters.Any())
            {
                return settingsParameters;
            }

            var names = string.Join(", ", methodParameters.Select(x => x.Name));
            throw new($@"Method `{methodInfo.DeclaringType!.Name}.{methodInfo.Name}` requires parameters, but none have been defined. Add UseParameters. For example:

VerifySettings settings = new();
settings.UseParameters({names});
await Verifier.Verify(target, settings);

or

await Verifier.Verify(target).UseParameters({names});
");
        }
    }
}
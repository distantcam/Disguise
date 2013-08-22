using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Disguise
{
    internal static class Extensions
    {
        public static Collection<TypeReference> GetGenericInstanceArguments(this TypeReference type)
        {
            return ((GenericInstanceType)type).GenericArguments;
        }

        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] args)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                self.DeclaringType.MakeGenericInstanceType(args))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
            }

            return reference;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectre.Cli.Internal
{
    internal static class TypeExtensions
    {
        public static bool IsPairDeconstructable(this Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(ILookup<,>) ||
                    type.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                    type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
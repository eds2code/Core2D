﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace Core2D.Data
{
    public static class ImmutableArrayExtensions
    {
        public static ImmutableArray<T>.Builder Copy<T>(this ref ImmutableArray<T> array, IDictionary<object, object> shared) where T : ObservableObject
        {
            var copy = ImmutableArray.CreateBuilder<T>();

            foreach (var item in array)
            {
                copy.Add((T)item.Copy(shared));
            }

            return copy;
        }
    }
}

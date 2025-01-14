﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minnow.Providers
{
    internal class TypeProvider<T> : ITypeProvider<T>
    {
        public readonly List<Type> _types;

        public TypeProvider(IEnumerable<Type> types)
        {
            _types = new List<Type>(types);
        }

        public IEnumerable<T> CreateInstances()
        {
            foreach (Type type in _types)
            {
                object? instance = Activator.CreateInstance(type);

                if (instance is not null && instance is T casted)
                {
                    yield return casted;
                }

                throw new InvalidOperationException();
            }
        }

        public IEnumerable<T> CreateInstances(IServiceProvider provider)
        {
            foreach (Type type in _types)
            {
                object instance = provider.GetService(type) ?? ActivatorUtilities.CreateInstance(provider, type);

                if (instance is T casted)
                {
                    yield return casted;
                }

                throw new InvalidOperationException();
            }
        }

        public IEnumerable<T> CreateInstances(IServiceProvider provider, params object[] args)
        {
            foreach (Type type in _types)
            {
                object instance = provider.GetService(type) ?? ActivatorUtilities.CreateInstance(provider, type, args);

                if (instance is T casted)
                {
                    yield return casted;
                }

                throw new InvalidOperationException();
            }
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _types.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

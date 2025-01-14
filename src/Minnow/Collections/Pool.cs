﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minnow.Collections
{
    public class Pool<T> : IPool<T>
        where T : class
    {
        private Stack<T> _pool;
        private UInt16 _poolSize;
        private UInt16 _maxPoolSize;

        public Pool(UInt16 maxPoolSize) : this(ref maxPoolSize)
        {

        }
        public Pool(ref UInt16 maxPoolSize)
        {
            _maxPoolSize = maxPoolSize;
            _poolSize = 0;
            _pool = new Stack<T>();
        }

        /// <inheritdoc />
        public virtual Boolean Any()
            => _pool.Any();

        /// <inheritdoc />
        public virtual Boolean TryPull(out T instance)
        {
            if(_pool.TryPop(out instance))
            {
                _poolSize--;

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public virtual Boolean TryReturn(T instance)
        {
            if (_poolSize < _maxPoolSize)
            {
                _pool.Push(instance);
                _poolSize++;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public virtual Int32 Count()
            => _poolSize;
    }
}

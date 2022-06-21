using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Vernuntii.Caching
{
    /// <summary>
    /// Responsible to create a new memory cache.
    /// </summary>
    public interface IMemoryCacheFactory
    {
        /// <summary>
        /// Creates a memory cache.
        /// </summary>
        IMemoryCache Create();
    }
}

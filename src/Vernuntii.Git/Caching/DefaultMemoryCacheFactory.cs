using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Vernuntii.Caching
{
    /// <summary>
    /// Defualt implementation of <see cref="IMemoryCacheFactory"/>.
    /// </summary>
    public class DefaultMemoryCacheFactory : IMemoryCacheFactory
    {
        /// <summary>
        /// The default instance of this type.
        /// </summary>
        public readonly static DefaultMemoryCacheFactory Default = new DefaultMemoryCacheFactory();

        /// <inheritdoc/>
        public IMemoryCache Create() => new DefaultMemoryCache();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.Console
{
    /// <summary>
    /// Extension methods for <see cref="IVernuntiiRunnerBuilder"/>.
    /// </summary>
    public static class VernuntiiRunnerBuilderExtensions
    {
        /// <summary>
        /// Builds the runner for <see cref="Vernuntii"/>.
        /// /// </summary>
        public static VernuntiiRunner Build(this IVernuntiiRunnerBuilder builder) =>
            builder.Build(args: null);
    }
}

namespace Vernuntii.HeightConventions
{
    internal class HeightRule : IHeightRule
    {
        /// <inheritdoc/>
        public int IfDots { get; }
        /// <inheritdoc/>
        public string Template { get; }

        public HeightRule(int ifDots, string template)
        {
            if (ifDots < 0) {
                throw new ArgumentOutOfRangeException(nameof(ifDots), "A pre-release cannot have lesser than zero dots");
            }

            IfDots = ifDots;
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public HeightRule(IHeightRule rule)
            : this(rule.IfDots, rule.Template) { }
    }
}

namespace Vernuntii.HeightVersioning.Ruling
{
    /// <summary>
    /// Represents a build height rule.
    /// </summary>
    public interface IHeightRule
    {
        /// <summary>
        /// A number at how many dots this rule is matching.
        /// </summary>
        int IfDots { get; }
        /// <summary>
        /// The template to be used when rule is matching.
        /// </summary>
        string Template { get; }
    }
}

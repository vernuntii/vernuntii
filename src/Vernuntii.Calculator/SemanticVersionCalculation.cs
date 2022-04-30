using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// Represents a pre-configured calculation of the next version.
    /// </summary>
    internal class SemanticVersionCalculation : ISemanticVersionCalculation
    {
        private readonly ISemanticVersionCalculator _calculator;
        private SemanticVersionCalculationOptions _calculationOptions;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="calculator"></param>
        /// <param name="calculationOptions"></param>
        public SemanticVersionCalculation(ISemanticVersionCalculator calculator, SemanticVersionCalculationOptions calculationOptions)
        {
            _calculator = calculator;
            _calculationOptions = calculationOptions;
        }

        /// <inheritdoc/>
        public ISemanticVersion GetVersion() => _calculator.CalculateVersion(_calculationOptions);
    }
}

using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// Represents a pre-configured calculation of the next version.
    /// </summary>
    internal class SingleVersionCalculation : ISingleVersionCalculation
    {
        private readonly ISingleVersionCalculator _calculator;
        private SingleVersionCalculationOptions _calculationOptions;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="calculator"></param>
        /// <param name="calculationOptions"></param>
        public SingleVersionCalculation(ISingleVersionCalculator calculator, SingleVersionCalculationOptions calculationOptions)
        {
            _calculator = calculator;
            _calculationOptions = calculationOptions;
        }

        /// <inheritdoc/>
        public ISemanticVersion GetVersion() => _calculator.CalculateVersion(_calculationOptions);
    }
}

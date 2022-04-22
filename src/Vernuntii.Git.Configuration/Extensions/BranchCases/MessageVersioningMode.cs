namespace Vernuntii.Extensions.BranchCases
{
    internal enum MessageVersioningMode
    {
        /// <summary>
        /// Increments most significant version as often as indicated.
        /// </summary>
        ContinousDeployment,
        /// <summary>
        /// Increments most significant version number only once until delivered.
        /// </summary>
        ContinousDelivery
    }
}

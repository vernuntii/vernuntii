namespace Vernuntii.Reactive.Coroutines.Steps;

internal interface ITraceEventStep : IStep
{
    IEventTrace Trace { get; }
    IEventConnector Connector { get; }
}

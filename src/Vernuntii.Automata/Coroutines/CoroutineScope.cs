namespace Vernuntii.Coroutines;

public class CoroutineScope
{
    public void HandleCoroutineInvocation(CoroutineInvocationArgumentReceiverAcceptor invocationArgumentReceiverAcceptor) {
        invocationArgumentReceiverAcceptor.Invoke(new CoroutineInvocationArgumentReceiver());
    }
}

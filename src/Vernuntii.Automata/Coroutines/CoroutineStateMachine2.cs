//namespace Vernuntii.Coroutines;

//internal class CoroutineStateMachine2<T>
//{
//    static CoroutineStateMachine2() {

//        var sm = new CoroutineStateMachine2<object?>();


//        var croot = new Coroutine<int>();


//        var t = croot.GetAwaiter();



//        var cwrapperCompletionSource = Coroutine<int>.CompletionSource.RentFromCache();

//        t.OnCompleted(new Action(() => {
//            try {
//                cwrapperCompletionSource.SetResult(t.GetResult());
//            } catch (Exception e) {
//                cwrapperCompletionSource.SetException(e);
//            }
//            sm.MoveNext();
//        }));


//        var cwrapper = new Coroutine<int>(cwrapperCompletionSource.CreateValueTask());
//    }

//    int forked
//    object? _result;

//    //internal void AppendContinaution(Action continuation) {

//    //}

//    void MoveNext() { 
        
//    }

//    internal void SetResult(T result) {
//        var continuation = _continuation;

//        if (continuation is null) { 
//            _
//        }
//        Interlocked.CompareExchange(ref _continuation, 
//    }
//}

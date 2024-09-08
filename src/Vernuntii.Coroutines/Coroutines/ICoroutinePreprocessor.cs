namespace Vernuntii.Coroutines;

internal interface ICoroutinePreprocessor
{
    void PreprocessChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutine) where TCoroutineAwaiter : IChildCoroutine;
    void PreprocessSiblingCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutine) where TCoroutineAwaiter : ISiblingCoroutine;
}

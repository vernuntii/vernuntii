namespace Vernuntii.Coroutines;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/// <summary>
/// An interface implemented by all <see cref="CoroutineMethodBuilder{T}.CoroutineStateMachineBox{TStateMachine}"/> instances, regardless of generics.
/// </summary>
internal interface ICoroutineStateMachineBox
{
    ref CoroutineContext CoroutineContext { get; }

    /// <summary>Move the state machine forward.</summary>
    void MoveNext();

    /// <summary>
    /// Gets an action for moving forward the contained state machine.
    /// This will lazily-allocate the delegate as needed.
    /// </summary>
    Action MoveNextAction { get; }
}

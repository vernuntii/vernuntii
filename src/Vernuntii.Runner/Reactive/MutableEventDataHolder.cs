﻿using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive;

internal class MutableEventDataHolder<T> : IEventDataHolder<T>
{
    [AllowNull, MaybeNull]
    public T EventData { get; set; }

    [MemberNotNullWhen(true, nameof(EventData))]
    public bool HasEventData { get; set; }
}

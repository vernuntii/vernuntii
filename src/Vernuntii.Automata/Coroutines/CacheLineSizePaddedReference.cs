using System.Runtime.InteropServices;

namespace Vernuntii.Coroutines;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/// <summary>Padded reference to an object.</summary>
[StructLayout(LayoutKind.Explicit, Size = CacheLineSizeHolder.CACHE_LINE_SIZE)]
internal struct CacheLineSizePaddedReference
{
    [FieldOffset(0)]
    public object? Object;
}

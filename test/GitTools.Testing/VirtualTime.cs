/* The MIT License (MIT)

Copyright (c) 2021 NServiceBus Ltd, GitTools and contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
namespace GitTools.Testing;

/// <summary>
/// VirtualTime starts at an hour before now, then each time it is called increments by a minute
/// Useful when interacting with git to make sure commits and other interactions are not at the same time
/// </summary>
public static class VirtualTime
{
    private static DateTimeOffset _simulatedTime = DateTimeOffset.Now.AddHours(-1);

    /// <summary>
    /// Increments by 1 minute each time it is called
    /// </summary>
    public static DateTimeOffset Now {
        get {
            _simulatedTime = _simulatedTime.AddMinutes(1);
            return _simulatedTime;
        }
    }
}

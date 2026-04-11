// SPDX-License-Identifier: MPL-2.0
namespace Trowel;

extern alias core;

readonly ref struct Lifetime : IDisposable
{
    readonly Span<bool> _value;

    public Lifetime(out bool value)
    {
        value = true;
        _value = MemoryMarshal.CreateSpan(ref value, 1);
    }

    /// <inheritdoc />
    public void Dispose() => _value[0] = false;
}

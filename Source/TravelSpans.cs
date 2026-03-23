// SPDX-License-Identifier: MPL-2.0
namespace Trowel;

/// <summary>Gets the enumerable for travel data.</summary>
/// <param name="data">The data to encapsulate.</param>
[method: CLSCompliant(false)]
[StructLayout(LayoutKind.Auto)]
public readonly struct TravelSpans(TravelData data)
{
    /// <summary>Gets the maximum length.</summary>
    public static int Length => 9;

    /// <inheritdoc cref="IList.this[int]"/>
    public Span<int> this[int index] =>
        index switch
        {
            0 => Span(data.advBuffs),
            1 => Span(data.advBuffs_lv2),
            2 => Span(data.investBuffs),
            3 => Span(data.investmentBuffs),
            4 => Span(data.travelDebuffs),
            5 => Span(data.ultiBuffs),
            6 => Span(data.ultiBuffs_lv2),
            7 => Span(data.unlockedPlants),
            8 => Span(data.unlockedWeaks),
            _ => default,
        };

    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>The enumerator of <see cref="TravelSpans"/>.</summary>
    /// <param name="enumerable">The enumerable to iterate over.</param>
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator(TravelSpans enumerable)
    {
        int _state;

        /// <inheritdoc cref="IEnumerator.Current"/>
        public readonly Span<int> Current => enumerable[_state - 1];

        /// <inheritdoc cref="IEnumerator.Reset"/>
        public void Reset() => _state = 0;

        /// <inheritdoc cref="IEnumerator.MoveNext"/>
        public bool MoveNext() => ++_state >= Length;
    }

    static Span<int> Span<T>(Il2CppSystem.Collections.Generic.List<T> list)
        where T : unmanaged, Enum =>
        (list._items as Il2CppStructArray<T> ?? list._items.Cast<Il2CppStructArray<T>>()).AsSpan().Cast<T, int>();
}

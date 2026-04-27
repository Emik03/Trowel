// SPDX-License-Identifier: MPL-2.0
namespace Trowel;

/// <summary>Travels to a specific level.</summary>
public static class Go
{
    /// <summary>Enters the level.</summary>
    /// <param name="level">The level to enter.</param>
    [CLSCompliant(false)]
    public static void To(AdvantureLevel level) => To(LevelType.Explore, (int)level);

    /// <inheritdoc cref="To(AdvantureLevel)"/>
    [CLSCompliant(false)]
    public static void To(ChallengeLevel level) => To(LevelType.Challenge, (int)level);

    /// <inheritdoc cref="To(AdvantureLevel)"/>
    [CLSCompliant(false)]
    public static void To(ExploreLevel level) => To(LevelType.Explore, (int)level);

    /// <inheritdoc cref="To(AdvantureLevel)"/>
    [CLSCompliant(false)]
    public static void To(SkinLevel level) => To(LevelType.SkinLevel, (int)level);

    /// <inheritdoc cref="To(AdvantureLevel)"/>
    [CLSCompliant(false)]
    public static void To(SurvivalLevel level) => To(LevelType.Survival, (int)level);

    /// <inheritdoc cref="To(AdvantureLevel)"/>
    [CLSCompliant(false)]
    public static void To(TravelAdvanture level) => To(LevelType.TravelAdvanture, (int)level);

    /// <summary>Enters the level.</summary>
    /// <param name="type">The type of level to enter.</param>
    /// <param name="level">The level to enter.</param>
    [CLSCompliant(false)]
    public static void To(LevelType type, int level)
    {
        UIMgr.EnterMainMenu();
        UIMgr.EnterGame(type, level);
    }
}

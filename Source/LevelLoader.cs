// SPDX-License-Identifier: MPL-2.0
namespace Trowel;

/// <summary>Travels to a specific level.</summary>
public static class Go
{
    /// <summary>Enters the level.</summary>
    /// <param name="level">The level to enter.</param>
    [CLSCompliant(false)]
    public static void To(ChallengeLevel level)
    {
        UIMgr.EnterMainMenu();
        UIMgr.EnterGame(LevelType.Challenge, (int)level);
    }

    /// <inheritdoc cref="To(ChallengeLevel)"/>
    [CLSCompliant(false)]
    public static void To(SurvivalLevel value)
    {
        UIMgr.EnterMainMenu();
        UIMgr.EnterGame(LevelType.Survival, (int)value);
    }

    /// <inheritdoc cref="To(ChallengeLevel)"/>
    [CLSCompliant(false)]
    public static void To(ExploreLevel value)
    {
        UIMgr.EnterMainMenu();
        UIMgr.EnterGame(LevelType.Explore, (int)value);
    }

    /// <inheritdoc cref="To(ChallengeLevel)"/>
    [CLSCompliant(false)]
    public static void To(TravelAdvanture value)
    {
        UIMgr.EnterMainMenu();
        UIMgr.EnterGame(LevelType.TravelAdvanture, (int)value);
    }

    /// <inheritdoc cref="To(ChallengeLevel)"/>
    [CLSCompliant(false)]
    public static void To(SkinLevel value)
    {
        UIMgr.EnterMainMenu();
        UIMgr.EnterGame(LevelType.SkinLevel, (int)value);
    }
}

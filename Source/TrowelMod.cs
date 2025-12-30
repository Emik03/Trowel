// SPDX-License-Identifier: MPL-2.0
[assembly: CLSCompliant(true)]
[assembly: MelonInfo(typeof(TrowelMod), nameof(Trowel), "0.1", nameof(Emik))]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace Trowel;

extern alias core;
using System;
using System.Reflection;
using Image = UnityEngine.UI.Image;
using KeyCode = core::UnityEngine.KeyCode;
using ListCardUI = Il2CppSystem.Collections.Generic.List<CardUI>;
using Object = core::UnityEngine.Object;
using Screen = core::UnityEngine.Screen;
using SpriteRenderer = core::UnityEngine.SpriteRenderer;
using Time = core::UnityEngine.Time;
using static core::UnityEngine.KeyCode;

/// <summary>The keybind mod.</summary>
[CLSCompliant(false)]
public sealed class TrowelMod : MelonMod
{
    bool? _forceOdyssey;

    float _gloveFlash;

    int _last;

    /// <summary>Invoked when <see cref="Mouse.PutDownItem"/> is called.</summary>
    public static event Action? OnPutDownItem;

    static bool Disabled => GameAPP.theGameStatus is not GameStatus.InGame;

    bool IsBuffering
    {
        get =>
            field &&
            Cards() is { Count: var count } cards &&
            _last < count &&
            cards[_last] is { isAvailable: true };
        set;
    }

    // ReSharper disable NullableWarningSuppressionIsUsed
    MelonPreferences_Entry<bool> AlwaysSafe { get; set; } = null!;

    MelonPreferences_Entry<bool> AllowResolutionChanges { get; set; } = null!;

    MelonPreferences_Entry<bool> ColorSlots { get; set; } = null!;

    MelonPreferences_Entry<bool> EnableMouseScroll { get; set; } = null!;

    MelonPreferences_Entry<bool> InvertMouseScroll { get; set; } = null!;

    MelonPreferences_Entry<int> MinimalSeedDurability { get; set; } = null!;

    MelonPreferences_Entry<bool[]> OdysseyAdvancedUpgrades { get; set; } = null!;

    MelonPreferences_Entry<bool[]> OdysseyDebuff { get; set; } = null!;

    MelonPreferences_Entry<bool[]> OdysseyUnlockPlant { get; set; } = null!;

    MelonPreferences_Entry<int[]> OdysseyUltimateUpgrades { get; set; } = null!;

    MelonPreferences_Entry<int[]> OdysseyAdvancedUnlockRound { get; set; } = null!;

    MelonPreferences_Entry<KeyCode[]> Cancel { get; set; } = null!;

    MelonPreferences_Entry<KeyCode[]> Odyssey { get; set; } = null!;

    MelonPreferences_Entry<KeyCode[][]> Slots { get; set; } = null!;

    MelonPreferences_Entry<KeyCode[][]> Shift { get; set; } = null!;

    /// <inheritdoc />
    // ReSharper restore NullableWarningSuppressionIsUsed
    public override void OnInitializeMelon()
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var category = MelonPreferences.CreateCategory(nameof(Trowel));

        AllowResolutionChanges = category.CreateEntry(nameof(AllowResolutionChanges), true);

        AlwaysSafe = category.CreateEntry(nameof(AlwaysSafe), false);
        ColorSlots = category.CreateEntry(nameof(ColorSlots), true);
        EnableMouseScroll = category.CreateEntry(nameof(EnableMouseScroll), true);
        InvertMouseScroll = category.CreateEntry(nameof(InvertMouseScroll), false);
        MinimalSeedDurability = category.CreateEntry(nameof(MinimalSeedDurability), 0);

        OdysseyDebuff = category.CreateEntry<bool[]>(nameof(OdysseyDebuff), []);
        OdysseyUnlockPlant = category.CreateEntry<bool[]>(nameof(OdysseyUnlockPlant), []);
        OdysseyAdvancedUpgrades = category.CreateEntry<bool[]>(nameof(OdysseyAdvancedUpgrades), []);
        OdysseyUltimateUpgrades = category.CreateEntry<int[]>(nameof(OdysseyUltimateUpgrades), []);
        OdysseyAdvancedUnlockRound = category.CreateEntry<int[]>(nameof(OdysseyAdvancedUnlockRound), []);

        Odyssey = category.CreateEntry<KeyCode[]>(nameof(Odyssey), [O]);
        Cancel = category.CreateEntry<KeyCode[]>(nameof(Cancel), [Alpha5]);
        Shift = category.CreateEntry<KeyCode[][]>(nameof(Shift), [[LeftControl, LeftShift, Tab]]);
        Slots = category.CreateEntry<KeyCode[][]>(nameof(Slots), [[A], [S], [D], [F], [Z], [X], [C]]);

        OnPutDownItem += AcknowledgeItemWasPutDown;
        var putDownItemMethod = typeof(Mouse).GetMethod(nameof(Mouse.PutDownItem), Flags, null, [], null);

        var allowMethod = typeof(Screen).GetMethod(
            nameof(Screen.SetResolution),
            Flags,
            null,
            [typeof(int), typeof(int), typeof(bool)],
            null
        );

        HarmonyInstance.Patch(putDownItemMethod, new(((Delegate)PutDownItem).Method));
        HarmonyInstance.Patch(allowMethod, new(((Delegate)AllowResolutionToChange).Method));
    }

    void AcknowledgeItemWasPutDown() => IsBuffering = false;

    /// <inheritdoc />
    public override void OnDeinitializeMelon() => OnPutDownItem -= AcknowledgeItemWasPutDown;

    /// <inheritdoc />
    public override void OnUpdate()
    {
        SetOdysseyState();

        if (Disabled || Mouse.Instance is var mouse && !mouse)
        {
            IsBuffering = false;
            return;
        }

        if (mouse.theItemOnMouse && Array.Exists(Cancel.Value, Input.GetKeyDown))
            mouse.PutDownItem();

        if (Cards() is not { } cards)
            return;

        SetDurabilityAndSafety(cards);
        SetColor(cards);
        BufferKeys(mouse, cards);
        BufferScrollWheel(mouse, cards);

        if (IsBuffering)
            SelectBufferedCard(mouse, cards);
    }

    // ReSharper disable SuggestBaseTypeForParameter
    static void ReadPreferences<T>(MelonPreferences_Entry<T[]> entry, Il2CppStructArray<T> list)
        where T : unmanaged
    {
        for (int i = 0, length = Math.Min(entry.Value.Length, list.Count); i < length; i++)
            list[i] = entry.Value[i];
    }

    static void WritePreferences<T>(MelonPreferences_Entry<T[]> entry, Il2CppStructArray<T> list)
        where T : unmanaged
    {
        if (entry.Value.Length < list.Count)
            entry.Value = new T[list.Count];

        for (var i = 0; i < list.Count; i++)
            entry.Value[i] = list[i];
    }

    static bool AllowResolutionToChange(int width, int height, bool fullscreen) =>
        Melon<TrowelMod>.Instance.AllowResolutionChanges.Value && (Disabled || Cards() is []);

    static bool PutDownItem()
    {
        OnPutDownItem?.Invoke();
        return true;
    }

    static ListCardUI? Cards() =>
        ConveyManager.Instance is var conveyor && conveyor ? conveyor.cardsOnBelt :
        InGameUI_IZ.Instance is var iZombie && iZombie ? iZombie.Cards :
        InGameUI.Instance is var ui && ui ? ui.Cards : null;

    void Buffer(ListCardUI cards, int i) =>
        (IsBuffering, _last) = (true, (i % cards.Count + cards.Count) % cards.Count);

    void BufferKeys(Mouse mouse, ListCardUI cards)
    {
        void SelectIfDown(Mouse mouse, ListCardUI cards, int i, int offset)
        {
            if (!Array.Exists(Slots.Value[i], Input.GetKeyDown))
                return;

            if (i + offset is var iOffset && (uint)iOffset >= (uint)cards.Count)
                return;

            if (mouse.theItemOnMouse)
                mouse.PutDownItem();

            Buffer(cards, iOffset);
        }

        int ShiftOffset()
        {
            const int BitsInByte = 8, MaxInt32LeftShiftWithoutBeingNegative = BitsInByte * sizeof(int) - 2;

            var length = Math.Min(Shift.Value.Length, MaxInt32LeftShiftWithoutBeingNegative);
            var ret = 0;

            for (var i = 0; i < length; i++)
                if (Array.Exists(Shift.Value[i], Input.GetKey))
                    ret ^= 1 << i;

            return ret;
        }

        var offset = ShiftOffset() * Slots.Value.Length;

        for (var i = 0; i < Slots.Value.Length; i++)
            SelectIfDown(mouse, cards, i, offset);
    }

    void BufferScrollWheel(Mouse mouse, ListCardUI cards)
    {
        if (!EnableMouseScroll.Value || Input.mouseScrollDelta is var delta && delta == default)
            return;

        var offset = (delta.x is 0 ? delta.y > 0 ^ InvertMouseScroll.Value : delta.x > 0) ? 1 : -1;

        for (var i = 0; i < cards.Count; i++)
            if (cards[i] == mouse.theCardOnMouse)
            {
                mouse.PutDownItem();
                Buffer(cards, i + offset);
                return;
            }

        if (cards is not [])
            Buffer(cards, _last + offset);
    }

    void SelectBufferedCard(Mouse mouse, ListCardUI cards)
    {
        if (cards[_last] is not { isAvailable: true } card)
            return;

        IsBuffering = false;
        var needsSpriteUpdate = mouse.mouseItemType is MouseItemType.Plant_preview or MouseItemType.Zombie_preview;
        mouse.PutDownItem();

        if (card.TryCast<IZECard>() is { } zombie)
            mouse.ClickZombieCard(zombie);
        else
            mouse.ClickOnCard(card);

        if (!needsSpriteUpdate)
            return;

        var previewGameObject = mouse.preview;

        if (!previewGameObject)
            return;

        var previewRenderer = previewGameObject.GetComponent<SpriteRenderer>();

        if (!previewRenderer)
            return;

        var itemGameObject = mouse.theItemOnMouse;

        if (!itemGameObject)
            return;

        var itemRenderer = itemGameObject.GetComponent<SpriteRenderer>();

        if (!itemRenderer)
            return;

        previewRenderer.sprite = itemRenderer.sprite;
    }

    void SetDurabilityAndSafety(ListCardUI cards)
    {
        void SetData(TreasureCardData data, ListCardUI cards)
        {
            if (AlwaysSafe.Value)
                TreasureData.treasureCards.Add(data);

            if (MinimalSeedDurability.Value <= 0)
                return;

            data.durability = data.maxDurability = Math.Max(data.maxDurability, MinimalSeedDurability.Value);

            foreach (var card in cards)
                if (card.data.Pointer == data.Pointer)
                    card.Durability = card.MaxDurability = data.maxDurability;
        }

        if (!TreasureManager.Instance)
            return;

        foreach (var data in TreasureManager.Instance.cardData)
            if (!TreasureData.treasureCards.Contains(data))
                SetData(data, cards);
    }

    void SetColor(ListCardUI cards)
    {
        const float ShadeIntensity = 2;

        if (!ColorSlots.Value)
            return;

        for (var i = 0; i < cards.Count && cards[i].GetComponent<Image>() is var image; i++)
            if (image && image.color is { g: var g } color)
                image.color = color with
                {
                    g = (i / Slots.Value.Length % 2) switch
                    {
                        0 when g <= 1 / ShadeIntensity => g * ShadeIntensity,
                        1 when g > 1 / ShadeIntensity => g / ShadeIntensity,
                        _ => g,
                    },
                };
    }

    void SetOdysseyState()
    {
        void FlashGlove()
        {
            if (_gloveFlash is not 0 && Glove.Instance is var g && g && g.GetComponent<Image>() is var i && i)
                i.color = i.color with
                {
                    r = _forceOdyssey is null or true ? 1 - _gloveFlash : 1,
                    g = _forceOdyssey is false or true ? 1 - _gloveFlash : 1,
                };
        }

        const float DrainSpeed = 4;

        if (!TravelLookMenu.Instance && Array.Exists(Odyssey.Value, Input.GetKeyDown))
            (_forceOdyssey, _gloveFlash) = (_forceOdyssey switch
            {
                null => true,
                true => false,
                false => null,
            }, 1);

        FlashGlove();
        _gloveFlash = Math.Max(_gloveFlash - Time.deltaTime * DrainSpeed, 0);

        if (_forceOdyssey is not { } forceOdyssey)
            return;

        var travel = GetOrAddTravel();

        if (Board.Instance is var board && !board)
            return;

        if (!board.isIZ)
            board.isEveStarted = forceOdyssey && Disabled;

        board.boardTag = board.boardTag with
        {
            isFreeCardSelect = forceOdyssey,
            enableTravelBuff = forceOdyssey,
            enableTravelPlant = forceOdyssey,
        };

        if (!forceOdyssey)
            Object.Destroy(travel);
    }

    TravelMgr? GetOrAddTravel()
    {
        TravelMgr Read(TravelMgr travel)
        {
            ReadPreferences(OdysseyDebuff, travel.debuff);
            ReadPreferences(OdysseyUnlockPlant, travel.unlockPlant);
            ReadPreferences(OdysseyAdvancedUpgrades, travel.advancedUpgrades);
            ReadPreferences(OdysseyUltimateUpgrades, travel.ultimateUpgrades);
            return travel;
        }

        if (TravelMgr.Instance is var travel && !travel)
            return _forceOdyssey is true && GameAPP.Instance is var game && game
                ? Read(game.gameObject.AddComponent<TravelMgr>())
                : null;

        if (!TravelLookMenu.Instance)
            return Read(travel);

        if (!Array.Exists(Odyssey.Value, Input.GetKeyDown))
            return travel;

        WritePreferences(OdysseyDebuff, travel.debuff);
        WritePreferences(OdysseyUnlockPlant, travel.unlockPlant);
        WritePreferences(OdysseyAdvancedUpgrades, travel.advancedUpgrades);
        WritePreferences(OdysseyUltimateUpgrades, travel.ultimateUpgrades);
        return travel;
    }
}

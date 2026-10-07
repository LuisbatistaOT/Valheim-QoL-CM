using ValheimQoLCM.Core;
using Xunit;

namespace ValheimQoLCM.Core.Tests;

public class CoreRulesTests
{
    [Fact]
    public void Admin_can_open_the_panel()
    {
        Assert.True(AdminGate.CanOpenPanel(true));
    }

    [Fact]
    public void Non_admin_cannot_open_the_panel()
    {
        Assert.False(AdminGate.CanOpenPanel(false));
        Assert.False(AdminGate.CanMutate(false));
    }

    [Fact]
    public void Turning_god_off_leaves_fly_unchanged()
    {
        var modes = new ModeToggles();
        Assert.True(modes.Set(PlayerMode.God, true).Ok);
        Assert.True(modes.Set(PlayerMode.God, false).Ok);

        Assert.False(modes.God);
        Assert.False(modes.Fly);
        Assert.False(modes.Creative);
        Assert.False(modes.FreeCam);
    }

    [Fact]
    public void Dead_character_rejects_a_mode_toggle()
    {
        var modes = new ModeToggles { CharacterIsDead = true };

        var result = modes.Set(PlayerMode.God, true);

        Assert.False(result.Ok);
        Assert.False(modes.God);
        Assert.Equal("Character is dead.", result.Error);
    }

    [Fact]
    public void Teleport_accepts_a_connected_player()
    {
        var result = TeleportSelection.Select("Ari", stillConnected: true, isSelf: false);

        Assert.True(result.Ok);
        Assert.Equal("Ari", result.Data);
    }

    [Fact]
    public void Teleport_rejects_a_disconnected_player()
    {
        var result = TeleportSelection.Select("Ari", stillConnected: false, isSelf: false);

        Assert.False(result.Ok);
        Assert.Equal("Player is not connected.", result.Error);
    }

    [Fact]
    public void Spawn_rejects_quantity_zero()
    {
        var result = SpawnValidation.Validate("Wood", 0, 1, 1, true);

        Assert.False(result.Ok);
        Assert.Equal("Invalid quantity.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Spawn_accepts_the_chosen_quantity()
    {
        var result = SpawnValidation.Validate("Wood", 5, 1, 1, true);

        Assert.True(result.Ok);
        Assert.Equal(5, result.Data.Quantity);
        Assert.Equal("Wood", result.Data.Prefab);
    }

    [Fact]
    public void Spawn_rejects_an_unknown_item()
    {
        var result = SpawnValidation.Validate("NotAnItem", 1, 1, 1, false);

        Assert.False(result.Ok);
        Assert.Equal("Unknown item.", result.Error);
    }

    [Fact]
    public void Death_removes_the_configured_percent()
    {
        Assert.Equal(20f, SkillLoss.Apply(40f, 50f));
        Assert.True(SkillLoss.LeavesGearUntouched);
    }

    [Fact]
    public void Out_of_range_percent_is_clamped()
    {
        Assert.Equal(100f, SkillLoss.ClampPercent(140f));
        Assert.Equal(40f, SkillLoss.Apply(40f, 0f));
        Assert.Equal(0f, SkillLoss.Apply(40f, 100f));
    }

    [Fact]
    public void Grant_admin_prefers_the_connection_id()
    {
        var result = SteamId.Resolve("76561198000000000", "76561198000000001");

        Assert.True(result.Ok);
        Assert.Equal("76561198000000000", result.Data);
    }

    [Fact]
    public void Missing_connection_id_uses_the_typed_fallback()
    {
        var result = SteamId.Resolve(null, "76561198000000001");

        Assert.True(result.Ok);
        Assert.Equal("76561198000000001", result.Data);
    }

    [Fact]
    public void An_item_without_an_icon_cannot_be_picked_up()
    {
        Assert.False(ItemListing.CanPickUp(0));
        Assert.True(ItemListing.CanPickUp(1));
    }

    [Fact]
    public void A_shared_item_name_keeps_the_prefab()
    {
        Assert.Equal("Finewood Bow (BowFineWood)", ItemListing.RowLabel("Finewood Bow", "BowFineWood", true));
        Assert.Equal("Wood", ItemListing.RowLabel("Wood", "Wood", false));
        Assert.Equal("BowVisual", ItemListing.RowLabel("$item_bow", "BowVisual", false));
    }

    [Fact]
    public void A_bad_typed_id_is_refused()
    {
        var result = SteamId.Resolve(null, "not-a-steam-id");

        Assert.False(result.Ok);
        Assert.Equal("Steam ID is not valid.", result.Error);
    }

    [Fact]
    public void Ghost_toggle_leaves_the_other_modes_alone()
    {
        var modes = new ModeToggles();

        Assert.True(modes.Set(PlayerMode.Ghost, true).Ok);
        Assert.True(modes.Set(PlayerMode.God, false).Ok);

        Assert.True(modes.Ghost);
        Assert.False(modes.God);
        Assert.False(modes.Fly);
        Assert.False(modes.Creative);
        Assert.False(modes.FreeCam);
    }

    [Fact]
    public void Dead_character_rejects_ghost()
    {
        var modes = new ModeToggles { CharacterIsDead = true };

        var result = modes.Set(PlayerMode.Ghost, true);

        Assert.False(result.Ok);
        Assert.False(modes.Ghost);
        Assert.Equal("Character is dead.", result.Error);
    }

    [Fact]
    public void Nearby_action_rejects_a_non_admin_and_a_dead_character()
    {
        var stranger = NearbyActions.Begin(false, false);
        var dead = NearbyActions.Begin(true, true);

        Assert.False(stranger.Ok);
        Assert.Equal("Admins only.", stranger.Error);
        Assert.False(dead.Ok);
        Assert.Equal("Character is dead.", dead.Error);
        Assert.True(NearbyActions.Begin(true, false).Ok);
    }

    [Fact]
    public void Tame_selects_a_tameable_creature_that_is_not_a_player()
    {
        Assert.True(NearbyActions.IsTameTarget(isPlayer: false, hasTameable: true));
        Assert.False(NearbyActions.IsTameTarget(isPlayer: true, hasTameable: true));
        Assert.False(NearbyActions.IsTameTarget(isPlayer: false, hasTameable: false));
    }

    [Fact]
    public void Kill_selects_an_untamed_enemy_inside_1000()
    {
        Assert.Equal(20f, NearbyActions.TameRadius);
        Assert.Equal(1000f, NearbyActions.KillRadius);
        Assert.True(NearbyActions.IsKillTarget(false, false, false, 1000f));
        Assert.False(NearbyActions.IsKillTarget(true, false, false, 1f));
        Assert.False(NearbyActions.IsKillTarget(false, true, false, 1f));
        Assert.False(NearbyActions.IsKillTarget(false, false, true, 1f));
        Assert.False(NearbyActions.IsKillTarget(false, false, false, 1000.1f));
    }

    [Fact]
    public void Nearby_messages_name_the_count_or_the_empty_area()
    {
        Assert.Equal("Tamed 2.", NearbyActions.TameMessage(2));
        Assert.Equal("No tameable animal was nearby.", NearbyActions.TameMessage(0));
        Assert.Equal("Killed 4.", NearbyActions.KillMessage(4));
        Assert.Equal("No enemy was nearby.", NearbyActions.KillMessage(0));
    }

    [Fact]
    public void Legacy_panel_hotkeys_are_replaced_by_plus()
    {
        Assert.Equal("KeypadPlus", PanelHotkey.DefaultKey);
        Assert.True(PanelHotkey.IsLegacyDefault("BackQuote", System.Array.Empty<string>()));
        Assert.True(PanelHotkey.IsLegacyDefault("Tab", new[] { "LeftControl" }));
        Assert.True(PanelHotkey.IsLegacyDefault("Plus", System.Array.Empty<string>()));
        Assert.False(PanelHotkey.IsLegacyDefault("KeypadPlus", System.Array.Empty<string>()));
        Assert.False(PanelHotkey.IsLegacyDefault("F5", System.Array.Empty<string>()));
        Assert.False(PanelHotkey.IsLegacyDefault("BackQuote", new[] { "LeftControl" }));
        Assert.False(PanelHotkey.IsLegacyDefault("Tab", new[] { "LeftShift" }));
        Assert.True(PanelHotkey.IsPlusBinding("KeypadPlus", System.Array.Empty<string>()));
        Assert.True(PanelHotkey.IsPlusBinding("Plus", System.Array.Empty<string>()));
        Assert.False(PanelHotkey.IsPlusBinding("F5", System.Array.Empty<string>()));
        Assert.False(PanelHotkey.IsPlusBinding("KeypadPlus", new[] { "LeftShift" }));
    }

    [Fact]
    public void Saved_skill_loss_of_zero_beats_the_default()
    {
        Assert.Equal((float?)0f, SavedSkillLoss.Parse("0"));
        Assert.Null(SavedSkillLoss.Parse(null));
        Assert.Null(SavedSkillLoss.Parse(" "));
        Assert.Null(SavedSkillLoss.Parse("nope"));
        Assert.Equal((float?)5f, SavedSkillLoss.Parse("5"));
        Assert.Equal(0f, SavedSkillLoss.Choose(0f, 5f));
        Assert.Equal(5f, SavedSkillLoss.Choose(null, 5f));
        Assert.Equal("0", SavedSkillLoss.Format(0f));
    }

    [Fact]
    public void Kill_debug_line_names_position_radius_seen_and_killed()
    {
        Assert.Equal(
            "Kill enemies at 10, 20, 30 radius 1000 seen 8 killed 0.",
            NearbyActions.KillDebug(10f, 20f, 30f, 8, 0));
    }
}

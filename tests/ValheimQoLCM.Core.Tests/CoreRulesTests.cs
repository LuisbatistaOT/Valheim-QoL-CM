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
    public void A_bad_typed_id_is_refused()
    {
        var result = SteamId.Resolve(null, "not-a-steam-id");

        Assert.False(result.Ok);
        Assert.Equal("Steam ID is not valid.", result.Error);
    }
}

using System;
using System.Linq;
using ValheimQoLCM.Core;
using Xunit;

namespace ValheimQoLCM.Core.Tests;

public class PickFilterTests
{
    private static bool Known(string prefab) => prefab != "Blackwood";

    [Fact]
    public void Preset_lists_are_non_empty_distinct_and_do_not_overlap()
    {
        var wood = PickFilter.Preset(PickFilter.Woodcutting);
        var mining = PickFilter.Preset(PickFilter.Mining);
        var farming = PickFilter.Preset(PickFilter.Farming);

        Assert.NotEmpty(wood);
        Assert.NotEmpty(mining);
        Assert.NotEmpty(farming);
        Assert.Equal(wood.Count, wood.Distinct(StringComparer.Ordinal).Count());
        Assert.Empty(wood.Intersect(mining, StringComparer.Ordinal));
        Assert.Empty(wood.Intersect(farming, StringComparer.Ordinal));
        Assert.Empty(mining.Intersect(farming, StringComparer.Ordinal));
        Assert.Empty(PickFilter.Preset(PickFilter.Custom));
        Assert.Empty(PickFilter.Preset(PickFilter.PickAll));
    }

    [Fact]
    public void Pick_all_allows_everything_and_a_list_allows_only_its_items()
    {
        Assert.True(PickFilter.Allows(PickFilterDraft.PickAll, "Feathers"));
        Assert.True(PickFilter.Allows(PickFilterDraft.PickAll, null));

        var wood = new PickFilterDraft(true, new[] { "Wood", "FineWood" });
        Assert.True(PickFilter.Allows(wood, "Wood"));
        Assert.False(PickFilter.Allows(wood, "Feathers"));
        Assert.False(PickFilter.Allows(wood, null));
    }

    [Fact]
    public void Draft_sorts_trims_and_dedupes_items()
    {
        var draft = new PickFilterDraft(true, new[] { " Wood", "FineWood", "Wood", "", "  " });

        Assert.Equal(new[] { "FineWood", "Wood" }, draft.Items);
        Assert.True(draft.Contains("Wood"));
        Assert.False(draft.Contains("Stone"));
    }

    [Fact]
    public void Adding_to_pick_all_turns_the_filter_on_and_removing_keeps_it_on()
    {
        var added = PickFilterDraft.PickAll.Add("Wood");
        Assert.True(added.On);
        Assert.Equal(new[] { "Wood" }, added.Items);

        var removed = added.Remove("Wood");
        Assert.True(removed.On);
        Assert.Empty(removed.Items);
        Assert.False(PickFilter.CanApply(removed));
        Assert.True(PickFilter.CanApply(PickFilterDraft.PickAll));
        Assert.True(PickFilter.CanApply(added));
    }

    [Fact]
    public void Match_names_each_preset_custom_and_pick_all()
    {
        Assert.Equal(PickFilter.PickAll, PickFilter.MatchPreset(PickFilterDraft.PickAll));
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting))));
        Assert.Equal(PickFilter.Mining, PickFilter.MatchPreset(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining))));
        Assert.Equal(PickFilter.Farming, PickFilter.MatchPreset(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Farming))));

        var miningWithoutStone = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining).Where(p => p != "Stone"));
        Assert.Equal(PickFilter.Custom, PickFilter.MatchPreset(miningWithoutStone));
        Assert.Equal(PickFilter.Custom, PickFilter.MatchPreset(new PickFilterDraft(true, Array.Empty<string>())));
    }

    [Fact]
    public void A_prefab_the_game_lacks_still_lets_the_preset_match()
    {
        var staged = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting, Known));

        Assert.DoesNotContain("Blackwood", staged.Items);
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(staged, Known));
        Assert.Equal(PickFilter.Custom, PickFilter.MatchPreset(staged));

        var fromFile = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting));
        Assert.True(fromFile.Known(Known).SameAs(staged));
    }

    [Fact]
    public void Same_as_compares_on_flag_and_items()
    {
        var a = new PickFilterDraft(true, new[] { "Wood" });
        Assert.True(a.SameAs(new PickFilterDraft(true, new[] { "Wood" })));
        Assert.False(a.SameAs(new PickFilterDraft(false, new[] { "Wood" })));
        Assert.False(a.SameAs(new PickFilterDraft(true, new[] { "Stone" })));
        Assert.False(a.SameAs(null!));
    }

    [Fact]
    public void Log_line_and_count_text()
    {
        Assert.Equal("Pick filter: Pick all.", PickFilter.LogLine(PickFilterDraft.PickAll));
        Assert.Equal("Pick filter: Custom, 1 item.", PickFilter.LogLine(new PickFilterDraft(true, new[] { "Wood" })));
        var mining = new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining));
        Assert.Equal("Pick filter: Mining, " + mining.Items.Count + " items.", PickFilter.LogLine(mining));
        Assert.Equal("2 items", PickFilter.CountText(2));
    }

    [Fact]
    public void Parse_of_missing_or_garbage_text_is_pick_all_without_custom()
    {
        foreach (string? text in new[] { null, "", "   ", "nonsense\r\nmore", "mode list" })
        {
            var state = PickFilterState.Parse(text);
            Assert.False(state.Applied.On);
            Assert.Empty(state.Applied.Items);
            Assert.Null(state.Custom);
        }
    }

    [Fact]
    public void Format_then_parse_round_trips()
    {
        var state = new PickFilterState(new PickFilterDraft(true, new[] { "Wood", "FineWood" }), new[] { "Stone", "IronScrap" });

        string text = PickFilterState.Format(state);
        var back = PickFilterState.Parse(text);

        Assert.Equal("mode list\nlist FineWood Wood\ncustom IronScrap Stone", text);
        Assert.True(back.Applied.SameAs(state.Applied));
        Assert.Equal(new[] { "IronScrap", "Stone" }, back.Custom);

        var off = PickFilterState.Parse(PickFilterState.Format(PickFilterState.Default));
        Assert.False(off.Applied.On);
        Assert.Null(off.Custom);
    }

    [Fact]
    public void Parse_accepts_windows_line_endings_and_extra_spaces()
    {
        var state = PickFilterState.Parse("mode  list\r\nlist  Wood   FineWood \r\ncustom Stone\r\n");

        Assert.True(state.Applied.On);
        Assert.Equal(new[] { "FineWood", "Wood" }, state.Applied.Items);
        Assert.Equal(new[] { "Stone" }, state.Custom);
    }

    [Fact]
    public void Apply_saves_custom_only_for_a_list_that_equals_no_preset()
    {
        var state = PickFilterState.Default;

        var mining = state.Apply(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining)), null);
        Assert.Null(mining.Custom);

        var custom = mining.Apply(new PickFilterDraft(true, new[] { "Stone", "IronScrap" }), null);
        Assert.Equal(new[] { "IronScrap", "Stone" }, custom.Custom);

        var backToPreset = custom.Apply(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting)), null);
        Assert.Equal(new[] { "IronScrap", "Stone" }, backToPreset.Custom);
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(backToPreset.Applied));

        var pickAll = backToPreset.Apply(PickFilterDraft.PickAll, null);
        Assert.Equal(new[] { "IronScrap", "Stone" }, pickAll.Custom);
        Assert.False(pickAll.Applied.On);
    }

    [Fact]
    public void Stage_maps_each_button_to_a_draft()
    {
        var mining = PickFilter.Stage(PickFilter.Mining, null, null);
        Assert.True(mining.Ok);
        Assert.True(mining.Data.On);
        Assert.Equal(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Mining)).Items, mining.Data.Items);

        var woodcutting = PickFilter.Stage(PickFilter.Woodcutting, null, null);
        Assert.True(woodcutting.Ok);
        Assert.True(woodcutting.Data.On);
        Assert.Equal(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting)).Items, woodcutting.Data.Items);

        var farming = PickFilter.Stage(PickFilter.Farming, null, null);
        Assert.True(farming.Ok);
        Assert.True(farming.Data.On);
        Assert.Equal(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Farming)).Items, farming.Data.Items);

        var pickAll = PickFilter.Stage(PickFilter.PickAll, new[] { "Wood" }, null);
        Assert.True(pickAll.Ok);
        Assert.False(pickAll.Data.On);
        Assert.Empty(pickAll.Data.Items);

        var custom = PickFilter.Stage(PickFilter.Custom, new[] { "Stone", "Blackwood" }, Known);
        Assert.True(custom.Ok);
        Assert.Equal(new[] { "Stone" }, custom.Data.Items);

        var noCustom = PickFilter.Stage(PickFilter.Custom, null, null);
        Assert.False(noCustom.Ok);
        Assert.Equal(PickFilter.NoCustomMessage, noCustom.Error);

        var unknown = PickFilter.Stage("Nonsense", null, null);
        Assert.False(unknown.Ok);
    }

    [Fact]
    public void Presets_have_the_spec_lists()
    {
        Assert.Equal(new[] { "Wood", "FineWood", "RoundLog", "ElderBark", "YggdrasilWood", "Blackwood" }, PickFilter.Preset(PickFilter.Woodcutting));
        Assert.Equal(new[] { "Stone", "CopperOre", "TinOre", "CopperScrap", "IronScrap", "SilverOre", "BlackMetalScrap", "FlametalOre", "FlametalOreNew", "Obsidian", "Chitin", "BlackMarble", "Softtissue", "Grausten" }, PickFilter.Preset(PickFilter.Mining));
        Assert.Equal(new[] { "Carrot", "CarrotSeeds", "Turnip", "TurnipSeeds", "Onion", "OnionSeeds", "Barley", "Flax", "JotunPuffs", "Magecap", "Fiddlehead", "Vineberry", "SmokePuff" }, PickFilter.Preset(PickFilter.Farming));
    }

    [Fact]
    public void Apply_with_an_empty_on_draft_keeps_custom()
    {
        var state = PickFilterState.Default.Apply(new PickFilterDraft(true, new[] { "Feathers", "Stone" }), null);

        var after = state.Apply(new PickFilterDraft(true, null), null);

        Assert.Equal(new[] { "Feathers", "Stone" }, after.Custom);
    }

    [Fact]
    public void Parse_mode_is_case_insensitive()
    {
        Assert.True(PickFilterState.Parse("mode LIST\nlist Wood\n").Applied.On);
    }

    [Fact]
    public void Format_and_Parse_round_trip_pick_all_with_custom()
    {
        var state = PickFilterState.Default.Apply(new PickFilterDraft(true, new[] { "Feathers", "Stone" }), null)
            .Apply(PickFilterDraft.PickAll, null);

        var parsed = PickFilterState.Parse(PickFilterState.Format(state));

        Assert.False(parsed.Applied.On);
        Assert.Equal(new[] { "Feathers", "Stone" }, parsed.Custom);
        Assert.Equal(PickFilterState.Format(state), PickFilterState.Format(parsed));
    }

    [Fact]
    public void Apply_keeps_a_preset_missing_one_item_as_a_preset()
    {
        Func<string, bool> isKnown = name => name != "Blackwood";
        var state = PickFilterState.Default.Apply(new PickFilterDraft(true, new[] { "Feathers" }), null);

        var after = state.Apply(new PickFilterDraft(true, PickFilter.Preset(PickFilter.Woodcutting, isKnown)), isKnown);

        Assert.Equal(new[] { "Feathers" }, after.Custom);
        Assert.Equal(PickFilter.Woodcutting, PickFilter.MatchPreset(after.Applied, isKnown));
    }

    [Fact]
    public void Stage_custom_with_no_known_item_fails()
    {
        var result = PickFilter.Stage(PickFilter.Custom, new[] { "Gone" }, _ => false);

        Assert.False(result.Ok);
        Assert.Equal(PickFilter.NoCustomMessage, result.Error);
    }

    [Fact]
    public void Apply_is_enabled_only_for_an_applicable_change()
    {
        var applied = new PickFilterDraft(true, new[] { "Wood" });
        Assert.False(PickFilter.ApplyEnabled(applied, applied));
        Assert.True(PickFilter.ApplyEnabled(new PickFilterDraft(true, new[] { "Stone" }), applied));
        Assert.True(PickFilter.ApplyEnabled(PickFilterDraft.PickAll, applied));
        Assert.False(PickFilter.ApplyEnabled(new PickFilterDraft(true, System.Array.Empty<string>()), applied));
    }
}

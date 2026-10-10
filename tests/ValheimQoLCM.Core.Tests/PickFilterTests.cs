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
}

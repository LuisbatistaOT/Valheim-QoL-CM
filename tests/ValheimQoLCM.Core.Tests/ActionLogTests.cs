using System;
using System.Linq;
using ValheimQoLCM.Core;
using Xunit;

namespace ValheimQoLCM.Core.Tests;

public class ActionLogTests
{
    public ActionLogTests()
    {
        ActionLog.Clear();
    }

    [Fact]
    public void Add_stamps_local_time_and_keeps_oldest_first()
    {
        var ten = new DateTime(2026, 10, 10, 22, 24, 14);
        var eleven = ten.AddSeconds(1);

        Assert.True(ActionLog.Add(ten, "God is on."));
        Assert.True(ActionLog.Add(eleven, "God is off."));

        Assert.Equal(new[] { "[22:24:14] God is on.", "[22:24:15] God is off." }, ActionLog.Entries.ToArray());
        Assert.Equal(2, ActionLog.Count);
    }

    [Fact]
    public void Add_rejects_blank_messages()
    {
        Assert.False(ActionLog.Add(DateTime.Now, null));
        Assert.False(ActionLog.Add(DateTime.Now, ""));
        Assert.False(ActionLog.Add(DateTime.Now, "   "));
        Assert.Equal(0, ActionLog.Count);
    }

    [Fact]
    public void Add_drops_the_oldest_past_forty()
    {
        var start = new DateTime(2026, 10, 10, 0, 0, 0);
        for (int i = 0; i < ActionLog.Capacity + 1; i++)
        {
            ActionLog.Add(start.AddSeconds(i), "line " + i);
        }

        Assert.Equal(ActionLog.Capacity, ActionLog.Count);
        Assert.StartsWith("[00:00:01] line 1", ActionLog.Entries[0]);
        Assert.EndsWith("line 40", ActionLog.Entries[ActionLog.Count - 1]);
    }

    [Fact]
    public void Clear_empties_the_buffer()
    {
        ActionLog.Add(new DateTime(2026, 10, 10, 1, 2, 3), "God is on.");
        ActionLog.Clear();

        Assert.Equal(0, ActionLog.Count);
        Assert.Empty(ActionLog.Entries);
    }

    [Fact]
    public void Format_is_24_hour_and_zero_padded()
    {
        Assert.Equal("[00:05:09] Log cleared.", ActionLog.Format(new DateTime(2026, 10, 10, 0, 5, 9), ActionLog.ClearedMessage));
    }
}

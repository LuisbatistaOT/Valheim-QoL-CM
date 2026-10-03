using System;
using HarmonyLib;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Replaces vanilla death skill loss with the synced percent.</summary>
public static class DeathPenaltyManager
{
    private static int _deathDepth;

    /// <summary>Asks the host to store a new skill-loss percent.</summary>
    public static ActionResult<float> RequestPercent(float percent)
    {
        if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
        {
            return ActionResult<float>.Fail("Admins only.");
        }

        var clamped = SkillLoss.ClampPercent(percent);
        Plugin.Send("percent", package => package.Write(clamped));
        return ActionResult<float>.Success(clamped);
    }

    /// <summary>Host-side write of the synced percent. The next death uses it.</summary>
    public static void ApplyPercent(long sender, float percent)
    {
        var clamped = SkillLoss.ClampPercent(percent);
        if (Plugin.SkillLossEntry != null)
        {
            Plugin.SkillLossEntry.Value = clamped;
        }

        Plugin.Reply(sender, "Skill loss is " + clamped.ToString("0") + "%.");
    }

    private static void EnterDeath()
    {
        _deathDepth++;
    }

    private static void ExitDeath()
    {
        _deathDepth = Math.Max(0, _deathDepth - 1);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class PlayerDeathScope
    {
        private static void Prefix()
        {
            EnterDeath();
        }

        private static Exception Finalizer(Exception __exception)
        {
            ExitDeath();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
    private static class SkillsDeathScope
    {
        private static void Prefix()
        {
            EnterDeath();
        }

        private static Exception Finalizer(Exception __exception)
        {
            ExitDeath();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
    private static class LowerAllSkillsPatch
    {
        private static void Prefix(ref float factor)
        {
            if (_deathDepth <= 0)
            {
                return;
            }

            factor = SkillLoss.ToFactor(Plugin.SkillLossPercent);
        }
    }
}

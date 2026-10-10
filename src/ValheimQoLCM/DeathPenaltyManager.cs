using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Skill = Skills.Skill;
using SkillType = Skills.SkillType;

namespace ValheimQoLCM;

public static class DeathPenaltyManager
{
	private sealed class SavedSkill
	{
		public SkillType Type { get; }

		public Skill Skill { get; }

		public float Level { get; }

		public float Accumulator { get; }

		public SavedSkill(SkillType type, Skill skill, float level, float accumulator)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			Type = type;
			Skill = skill;
			Level = level;
			Accumulator = accumulator;
		}
	}

	[HarmonyPatch(typeof(Player), "OnDeath")]
	private static class PlayerDeathScope
	{
		private static void Prefix(Player __instance)
		{
			EnterDeath();
			RememberSkills(__instance);
		}

		private static void Postfix(Player __instance)
		{
			RestoreSkills(__instance);
		}

		private static Exception Finalizer(Player __instance, Exception __exception)
		{
			RestoreSkills(__instance);
			ExitDeath();
			return __exception;
		}
	}

	[HarmonyPatch(typeof(Skills), "OnDeath")]
	private static class SkillsDeathScope
	{
		private static float _savedFactor;

		private static bool _factorSaved;

		private static void Prefix(Skills __instance)
		{
			EnterDeath();
			if (!((Object)(object)__instance == (Object)null) && KeepSkills())
			{
				_savedFactor = __instance.m_DeathLowerFactor;
				_factorSaved = true;
				__instance.m_DeathLowerFactor = 0f;
			}
		}

		private static void Postfix(Skills __instance)
		{
			RestoreFactor(__instance);
		}

		private static Exception Finalizer(Skills __instance, Exception __exception)
		{
			RestoreFactor(__instance);
			ExitDeath();
			return __exception;
		}

		private static void RestoreFactor(Skills skills)
		{
			if (_factorSaved && !((Object)(object)skills == (Object)null))
			{
				skills.m_DeathLowerFactor = _savedFactor;
				_factorSaved = false;
			}
		}
	}

	[HarmonyPatch(typeof(Skills), "LowerAllSkills")]
	private static class LowerAllSkillsPatch
	{
		private static void Prefix(ref float factor)
		{
			if (KeepSkills())
			{
				factor = 0f;
			}
		}
	}

	[HarmonyPatch(typeof(Skills), "Clear")]
	private static class ClearSkillsPatch
	{
		private static bool Prefix()
		{
			return _deathDepth <= 0 || !KeepSkills();
		}
	}

	private static int _deathDepth;

	private static readonly List<SavedSkill> SavedSkills = new List<SavedSkill>();

	private static void EnterDeath()
	{
		_deathDepth++;
	}

	private static void ExitDeath()
	{
		_deathDepth = Math.Max(0, _deathDepth - 1);
	}

	private static bool KeepSkills()
	{
		if (Plugin.OverwriteApplied)
		{
			return true;
		}
		return (Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer() && PluginStorage.ReadOverwrite();
	}

	private static void RememberSkills(Player player)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		SavedSkills.Clear();
		if (!KeepSkills() || (Object)(object)player == (Object)null)
		{
			PluginStorage.Debug("Skill overwrite death vanilla.");
			return;
		}
		Skills skills = ((Character)player).GetSkills();
		if ((Object)(object)skills == (Object)null || skills.m_skillData == null)
		{
			PluginStorage.Debug("Skill overwrite death vanilla.");
			return;
		}
		foreach (KeyValuePair<SkillType, Skill> skillDatum in skills.m_skillData)
		{
			if (skillDatum.Value != null)
			{
				SavedSkills.Add(new SavedSkill(skillDatum.Key, skillDatum.Value, skillDatum.Value.m_level, skillDatum.Value.m_accumulator));
			}
		}
		PluginStorage.Debug("Skill overwrite death kept.");
	}

	private static void RestoreSkills(Player player)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (SavedSkills.Count == 0)
		{
			return;
		}
		Skills val = (((Object)(object)player != (Object)null) ? ((Character)player).GetSkills() : null);
		if ((Object)(object)val != (Object)null && val.m_skillData != null)
		{
			foreach (SavedSkill savedSkill in SavedSkills)
			{
				Skill val2 = null;
				if (val.m_skillData.ContainsKey(savedSkill.Type))
				{
					val2 = val.m_skillData[savedSkill.Type];
				}
				if (val2 == null)
				{
					val2 = savedSkill.Skill;
					val.m_skillData[savedSkill.Type] = val2;
				}
				val2.m_level = savedSkill.Level;
				val2.m_accumulator = savedSkill.Accumulator;
			}
		}
		SavedSkills.Clear();
	}
}

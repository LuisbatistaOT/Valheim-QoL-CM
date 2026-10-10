using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Jotunn;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public static class WorldModifierHost
{
	public static ActionResult<WorldModifierDraft> ReadApplied()
	{
		if ((Object)(object)ZoneSystem.instance == (Object)null)
		{
			return ActionResult<WorldModifierDraft>.Fail("World is not loaded.");
		}
		return ActionResult<WorldModifierDraft>.Success(ModifierRules.FromKeys(ZoneSystem.instance.GetGlobalKeys(), Plugin.OverwriteApplied));
	}

	public static ActionResult<WorldModifierDraft> RequestApply(WorldModifierDraft staged)
	{
		if (!AdminGate.CanMutate(Plugin.LocalIsAdmin()))
		{
			return ActionResult<WorldModifierDraft>.Fail("Admins only.");
		}
		ActionResult<WorldModifierDraft> actionResult = ModifierRules.Parse(staged.Combat, staged.Death, staged.Resources, staged.Raids, staged.Portals, staged.Overwrite);
		if (!actionResult.Ok)
		{
			return actionResult;
		}
		Plugin.Send("world", delegate(ZPackage package)
		{
			package.Write(staged.Combat);
			package.Write(staged.Death);
			package.Write(staged.Resources);
			package.Write(staged.Raids);
			package.Write(staged.Portals);
			package.Write(staged.Overwrite);
		});
		return actionResult;
	}

	public static void Apply(long sender, string combat, string death, string resources, string raids, string portals, bool overwrite)
	{
		if ((Object)(object)ZoneSystem.instance == (Object)null)
		{
			Plugin.Reply(sender, "World is not loaded.");
			return;
		}
		ActionResult<WorldModifierDraft> actionResult = ModifierRules.Parse(combat, death, resources, raids, portals, overwrite);
		if (!actionResult.Ok)
		{
			Plugin.Reply(sender, actionResult.Error ?? "Unknown world modifier.");
			return;
		}
		ZoneSystem instance = ZoneSystem.instance;
		List<string> keys = new List<string>(instance.GetGlobalKeys());
		bool overwriteApplied = Plugin.OverwriteApplied;
		try
		{
			WriteKeys(instance, ModifierRules.KeysToWrite(actionResult.Data));
			Plugin.SetOverwrite(actionResult.Data.Overwrite, persist: true);
			Plugin.Reply(sender, ModifierRules.SuccessMessage(actionResult.Data));
		}
		catch (Exception ex)
		{
			Logger.LogError((object)ex);
			try
			{
				WriteKeys(instance, ManagedOnly(keys));
				Plugin.SetOverwrite(overwriteApplied, persist: true);
				Plugin.Reply(sender, "World modifiers failed.");
			}
			catch (Exception ex2)
			{
				Logger.LogError((object)ex2);
				WorldModifierDraft draft = ModifierRules.FromKeys(instance.GetGlobalKeys(), Plugin.OverwriteApplied);
				Plugin.Reply(sender, ModifierRules.FailedRestoreMessage(draft));
			}
		}
	}

	private static void WriteKeys(ZoneSystem zone, IReadOnlyList<string> keys)
	{
		MethodInfo methodInfo = AccessTools.Method(typeof(ZoneSystem), "GlobalKeyRemove", new Type[2]
		{
			typeof(string),
			typeof(bool)
		}, (Type[])null);
		MethodInfo methodInfo2 = AccessTools.Method(typeof(ZoneSystem), "GlobalKeyAdd", new Type[2]
		{
			typeof(string),
			typeof(bool)
		}, (Type[])null);
		if (methodInfo == null || methodInfo2 == null)
		{
			throw new MissingMethodException("ZoneSystem global key methods are missing.");
		}
		List<string> list = new List<string>(zone.GetGlobalKeys());
		foreach (string item in list)
		{
			if (ModifierRules.IsManagedKey(item))
			{
				methodInfo.Invoke(zone, new object[2] { item, true });
			}
		}
		foreach (string key in keys)
		{
			methodInfo2.Invoke(zone, new object[2] { key, true });
		}
		zone.UpdateWorldRates();
		MethodInfo methodInfo3 = AccessTools.Method(typeof(ZoneSystem), "SendGlobalKeys", new Type[1] { typeof(long) }, (Type[])null);
		if (methodInfo3 != null)
		{
			methodInfo3.Invoke(zone, new object[1] { 0L });
		}
	}

	private static List<string> ManagedOnly(List<string> keys)
	{
		List<string> list = new List<string>();
		foreach (string key in keys)
		{
			if (ModifierRules.IsManagedKey(key))
			{
				list.Add(key);
			}
		}
		return list;
	}
}

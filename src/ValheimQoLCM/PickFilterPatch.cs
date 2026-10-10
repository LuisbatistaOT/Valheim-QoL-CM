using HarmonyLib;

namespace ValheimQoLCM;

/// <summary>True only while <c>Player.AutoPickup</c> is on the stack. The E key never sets it.</summary>
internal static class PickFilterScope
{
	public static bool InAutoPickup;
}

[HarmonyPatch(typeof(Player), "AutoPickup")]
internal static class AutoPickupScopePatch
{
	[HarmonyPrefix]
	private static void Enter()
	{
		PickFilterScope.InAutoPickup = true;
	}

	[HarmonyFinalizer]
	private static void Leave()
	{
		PickFilterScope.InAutoPickup = false;
	}
}

[HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.CanPickup))]
internal static class CanPickupFilterPatch
{
	[HarmonyPrefix]
	private static bool SkipFiltered(ItemDrop __instance, ref bool __result)
	{
		if (!PickFilterScope.InAutoPickup)
		{
			return true;
		}
		if (PickFilterService.Allows(PrefabName(__instance)))
		{
			return true;
		}
		__result = false;
		return false;
	}

	private static string? PrefabName(ItemDrop drop)
	{
		ItemDrop.ItemData data = drop.m_itemData;
		if (data != null && data.m_dropPrefab != null)
		{
			return data.m_dropPrefab.name;
		}
		return Utils.GetPrefabName(drop.gameObject);
	}
}

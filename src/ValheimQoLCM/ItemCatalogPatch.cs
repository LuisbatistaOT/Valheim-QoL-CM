using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using HarmonyLib;

namespace ValheimQoLCM;

[HarmonyPatch(typeof(ObjectDB), "Awake")]
internal static class ItemCatalogPatch
{
	[HarmonyPostfix]
	private static void AfterItemsLoad()
	{
		ItemCatalog.Rebuild();
	}
}

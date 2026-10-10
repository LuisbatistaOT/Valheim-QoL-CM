using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
namespace ValheimQoLCM;

public sealed class SpawnableItem
{
	public string Prefab { get; }

	public string Label { get; }

	public int MaxStack { get; }

	public int MaxQuality { get; }

	public SpawnableItem(string prefab, string label, int maxStack, int maxQuality)
	{
		Prefab = prefab;
		Label = label;
		MaxStack = maxStack;
		MaxQuality = maxQuality;
	}
}

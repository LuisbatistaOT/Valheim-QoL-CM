using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
namespace ValheimQoLCM;

public sealed class ConnectedPlayer
{
	public string Name { get; }

	public string? SteamId { get; }

	public bool IsSelf { get; }

	public ConnectedPlayer(string name, string? steamId, bool isSelf)
	{
		Name = name;
		SteamId = steamId;
		IsSelf = isSelf;
	}
}

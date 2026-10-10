namespace ValheimQoLCM.Core;

public sealed class WorldModifierDraft
{
	public string Combat { get; }

	public string Death { get; }

	public string Resources { get; }

	public string Raids { get; }

	public string Portals { get; }

	public bool Overwrite { get; }

	public string PresetName => WorldModifiers.MatchPreset(this);

	public WorldModifierDraft(string combat, string death, string resources, string raids, string portals, bool overwrite)
	{
		Combat = combat;
		Death = death;
		Resources = resources;
		Raids = raids;
		Portals = portals;
		Overwrite = overwrite;
	}

	public WorldModifierDraft ChoosePreset(string name)
	{
		WorldModifierDraft worldModifierDraft = WorldModifiers.Preset(name);
		return new WorldModifierDraft(worldModifierDraft.Combat, worldModifierDraft.Death, worldModifierDraft.Resources, worldModifierDraft.Raids, worldModifierDraft.Portals, overwrite: false);
	}

	public WorldModifierDraft MoveResources(string stop)
	{
		return new WorldModifierDraft(Combat, Death, stop, Raids, Portals, overwrite: false);
	}

	public WorldModifierDraft MoveCombat(string stop)
	{
		return new WorldModifierDraft(stop, Death, Resources, Raids, Portals, overwrite: false);
	}

	public WorldModifierDraft MoveDeath(string stop)
	{
		return new WorldModifierDraft(Combat, stop, Resources, Raids, Portals, overwrite: false);
	}

	public WorldModifierDraft MoveRaids(string stop)
	{
		return new WorldModifierDraft(Combat, Death, Resources, stop, Portals, overwrite: false);
	}

	public WorldModifierDraft MovePortals(string stop)
	{
		return new WorldModifierDraft(Combat, Death, Resources, Raids, stop, overwrite: false);
	}

	public WorldModifierDraft WithOverwrite(bool overwrite)
	{
		return new WorldModifierDraft(Combat, Death, Resources, Raids, Portals, overwrite);
	}

	public bool SameAs(WorldModifierDraft other)
	{
		return other != null && SameSteps(other) && Overwrite == other.Overwrite;
	}

	public bool SameSteps(WorldModifierDraft other)
	{
		return other != null && Combat == other.Combat && Death == other.Death && Resources == other.Resources && Raids == other.Raids && Portals == other.Portals;
	}
}

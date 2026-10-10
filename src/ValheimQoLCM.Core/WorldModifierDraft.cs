namespace ValheimQoLCM.Core;

/// <summary>
/// The five World-tab steps plus the skill-loss overwrite flag. Immutable; every change returns a new draft.
/// Moving a step or choosing a preset turns the staged overwrite off, so an admin re-arms it on purpose.
/// </summary>
public sealed class WorldModifierDraft
{
	/// <summary>Combat step name.</summary>
	public string Combat { get; }

	/// <summary>Death penalty step name.</summary>
	public string Death { get; }

	/// <summary>Resources step name.</summary>
	public string Resources { get; }

	/// <summary>Raids step name.</summary>
	public string Raids { get; }

	/// <summary>Portals step name.</summary>
	public string Portals { get; }

	/// <summary>Skill-loss overwrite staged with these steps.</summary>
	public bool Overwrite { get; }

	/// <summary>The preset these steps match, or <c>Custom</c>.</summary>
	public string PresetName => WorldModifiers.MatchPreset(this);

	/// <summary>Create a draft from five step names and the overwrite flag. Names are not validated here; see <see cref="WorldModifiers.Parse"/>.</summary>
	public WorldModifierDraft(string combat, string death, string resources, string raids, string portals, bool overwrite)
	{
		Combat = combat;
		Death = death;
		Resources = resources;
		Raids = raids;
		Portals = portals;
		Overwrite = overwrite;
	}

	/// <summary>All five steps from a preset, overwrite off.</summary>
	public WorldModifierDraft ChoosePreset(string name)
	{
		WorldModifierDraft worldModifierDraft = WorldModifiers.Preset(name);
		return new WorldModifierDraft(worldModifierDraft.Combat, worldModifierDraft.Death, worldModifierDraft.Resources, worldModifierDraft.Raids, worldModifierDraft.Portals, overwrite: false);
	}

	/// <summary>Same draft with a new Resources step, overwrite off.</summary>
	public WorldModifierDraft MoveResources(string stop)
	{
		return new WorldModifierDraft(Combat, Death, stop, Raids, Portals, overwrite: false);
	}

	/// <summary>Same draft with a new Combat step, overwrite off.</summary>
	public WorldModifierDraft MoveCombat(string stop)
	{
		return new WorldModifierDraft(stop, Death, Resources, Raids, Portals, overwrite: false);
	}

	/// <summary>Same draft with a new Death penalty step, overwrite off.</summary>
	public WorldModifierDraft MoveDeath(string stop)
	{
		return new WorldModifierDraft(Combat, stop, Resources, Raids, Portals, overwrite: false);
	}

	/// <summary>Same draft with a new Raids step, overwrite off.</summary>
	public WorldModifierDraft MoveRaids(string stop)
	{
		return new WorldModifierDraft(Combat, Death, Resources, stop, Portals, overwrite: false);
	}

	/// <summary>Same draft with a new Portals step, overwrite off.</summary>
	public WorldModifierDraft MovePortals(string stop)
	{
		return new WorldModifierDraft(Combat, Death, Resources, Raids, stop, overwrite: false);
	}

	/// <summary>Same steps with the overwrite flag set as given.</summary>
	public WorldModifierDraft WithOverwrite(bool overwrite)
	{
		return new WorldModifierDraft(Combat, Death, Resources, Raids, Portals, overwrite);
	}

	/// <summary>Same steps and the same overwrite flag. Apply stays gray while this is true against the world.</summary>
	public bool SameAs(WorldModifierDraft other)
	{
		return other != null && SameSteps(other) && Overwrite == other.Overwrite;
	}

	/// <summary>Same five steps, ignoring the overwrite flag.</summary>
	public bool SameSteps(WorldModifierDraft other)
	{
		return other != null && Combat == other.Combat && Death == other.Death && Resources == other.Resources && Raids == other.Raids && Portals == other.Portals;
	}
}

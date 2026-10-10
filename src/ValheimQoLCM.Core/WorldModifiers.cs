using System;
using System.Collections.Generic;
using System.Linq;

namespace ValheimQoLCM.Core;

/// <summary>
/// The World tab's rules: step names per family, the preset table, the global keys each step
/// writes, the parse back from a world's keys, and where a stepped slider mark sits.
/// Plugin code refers to this type as <c>ModifierRules</c> because Valheim has its own <c>WorldModifiers</c>.
/// </summary>
public static class WorldModifiers
{
	/// <summary>Console message for a step name outside its family.</summary>
	public const string UnknownStop = "Unknown world modifier.";

	/// <summary>Console message when there is no <c>ZoneSystem</c> yet.</summary>
	public const string WorldNotLoaded = "World is not loaded.";

	/// <summary>Console message when writing keys threw and the previous keys were restored.</summary>
	public const string ApplyFailed = "World modifiers failed.";

	private static readonly string[] CombatStops = new string[5] { "Very easy", "Easy", "Normal", "Hard", "Very hard" };

	private static readonly string[] DeathStops = new string[6] { "Casual", "Very easy", "Easy", "Normal", "Hard", "Hardcore" };

	private static readonly string[] ResourceStops = new string[6] { "Much less", "Less", "Normal", "More", "Much more", "Most" };

	private static readonly string[] RaidStops = new string[6] { "None", "Much less", "Less", "Normal", "More", "Much more" };

	private static readonly string[] PortalStops = new string[4] { "Casual", "Normal", "Hard", "Very hard" };

	private static readonly string[] CombatKeys = new string[4] { "playerdamage", "enemydamage", "enemyspeedsize", "enemyleveluprate" };

	private static readonly string[] DeathKeys = new string[5] { "deathkeepequip", "skillreductionrate", "deathdeleteunequipped", "deathdeleteitems", "deathskillsreset" };

	private static readonly string[] ResourceKeys = new string[1] { "resourcerate" };

	private static readonly string[] RaidKeys = new string[1] { "eventrate" };

	private static readonly string[] PortalKeys = new string[3] { "teleportall", "nobossportals", "noportals" };

	private static readonly Dictionary<string, string[]> Bundles = new Dictionary<string, string[]>(StringComparer.Ordinal)
	{
		["Combat\nVery easy"] = new string[3] { "PlayerDamage 125", "EnemyDamage 50", "EnemySpeedSize 90" },
		["Combat\nEasy"] = new string[3] { "PlayerDamage 110", "EnemyDamage 75", "EnemySpeedSize 95" },
		["Combat\nNormal"] = new string[0],
		["Combat\nHard"] = new string[4] { "PlayerDamage 85", "EnemyDamage 150", "EnemySpeedSize 110", "EnemyLevelUpRate 120" },
		["Combat\nVery hard"] = new string[4] { "PlayerDamage 70", "EnemyDamage 200", "EnemySpeedSize 120", "EnemyLevelUpRate 140" },
		["Death\nCasual"] = new string[2] { "DeathKeepEquip", "SkillReductionRate 15" },
		["Death\nVery easy"] = new string[1] { "SkillReductionRate 15" },
		["Death\nEasy"] = new string[1] { "SkillReductionRate 50" },
		["Death\nNormal"] = new string[0],
		["Death\nHard"] = new string[2] { "DeathDeleteUnequipped", "SkillReductionRate 150" },
		["Death\nHardcore"] = new string[2] { "DeathDeleteItems", "DeathSkillsReset" },
		["Resources\nMuch less"] = new string[1] { "ResourceRate 50" },
		["Resources\nLess"] = new string[1] { "ResourceRate 75" },
		["Resources\nNormal"] = new string[0],
		["Resources\nMore"] = new string[1] { "ResourceRate 150" },
		["Resources\nMuch more"] = new string[1] { "ResourceRate 200" },
		["Resources\nMost"] = new string[1] { "ResourceRate 300" },
		["Raids\nNone"] = new string[1] { "EventRate 0" },
		["Raids\nMuch less"] = new string[1] { "EventRate 200" },
		["Raids\nLess"] = new string[1] { "EventRate 150" },
		["Raids\nNormal"] = new string[0],
		["Raids\nMore"] = new string[1] { "EventRate 60" },
		["Raids\nMuch more"] = new string[1] { "EventRate 30" },
		["Portals\nCasual"] = new string[1] { "TeleportAll" },
		["Portals\nNormal"] = new string[0],
		["Portals\nHard"] = new string[1] { "NoBossPortals" },
		["Portals\nVery hard"] = new string[1] { "NoPortals" }
	};

	/// <summary>Every family on Normal, overwrite off. What a world with no managed keys reads as.</summary>
	public static WorldModifierDraft Normal()
	{
		return Preset("Normal");
	}

	/// <summary>The five steps of a named preset, overwrite off. An unknown name is Normal.</summary>
	public static WorldModifierDraft Preset(string name)
	{
		return name switch
		{
			"Casual" => new WorldModifierDraft("Very easy", "Casual", "More", "None", "Casual", overwrite: false), 
			"Easy" => new WorldModifierDraft("Easy", "Normal", "Normal", "Less", "Normal", overwrite: false), 
			"Hard" => new WorldModifierDraft("Hard", "Normal", "Normal", "More", "Normal", overwrite: false), 
			"Hardcore" => new WorldModifierDraft("Very hard", "Hardcore", "Normal", "More", "Hard", overwrite: false), 
			_ => new WorldModifierDraft("Normal", "Normal", "Normal", "Normal", "Normal", overwrite: false), 
		};
	}

	/// <summary>A draft from five step names, or <see cref="UnknownStop"/> when any name is outside its family.</summary>
	public static ActionResult<WorldModifierDraft> Parse(string combat, string death, string resources, string raids, string portals, bool overwrite)
	{
		if (!Known(CombatStops, combat) || !Known(DeathStops, death) || !Known(ResourceStops, resources) || !Known(RaidStops, raids) || !Known(PortalStops, portals))
		{
			return ActionResult<WorldModifierDraft>.Fail("Unknown world modifier.");
		}
		return ActionResult<WorldModifierDraft>.Success(new WorldModifierDraft(combat, death, resources, raids, portals, overwrite));
	}

	/// <summary>
	/// The steps a world's global keys describe. Only managed keys are read. A family whose keys
	/// match no step, or that has none, reads as Normal.
	/// </summary>
	public static WorldModifierDraft FromKeys(IReadOnlyList<string> keys, bool overwrite)
	{
		List<string> list = new List<string>();
		if (keys != null)
		{
			foreach (string key in keys)
			{
				if (IsManagedKey(key))
				{
					list.Add(Canon(key));
				}
			}
		}
		return new WorldModifierDraft(MatchFamily("Combat", CombatStops, list), MatchFamily("Death", DeathStops, list), MatchFamily("Resources", ResourceStops, list), MatchFamily("Raids", RaidStops, list), MatchFamily("Portals", PortalStops, list), overwrite);
	}

	/// <summary>The global keys that put a world on these steps. Normal contributes none.</summary>
	public static IReadOnlyList<string> KeysToWrite(WorldModifierDraft draft)
	{
		List<string> list = new List<string>();
		list.AddRange(Bundle("Combat", draft.Combat));
		list.AddRange(Bundle("Death", draft.Death));
		list.AddRange(Bundle("Resources", draft.Resources));
		list.AddRange(Bundle("Raids", draft.Raids));
		list.AddRange(Bundle("Portals", draft.Portals));
		return list;
	}

	/// <summary>True for a key one of the five families owns. Apply removes and rewrites only these.</summary>
	public static bool IsManagedKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}
		string value = Canon(key).Split(new char[1] { ' ' })[0];
		return Contains(CombatKeys, value) || Contains(DeathKeys, value) || Contains(ResourceKeys, value) || Contains(RaidKeys, value) || Contains(PortalKeys, value);
	}

	/// <summary>The preset whose five steps equal the draft's, or <c>Custom</c>.</summary>
	public static string MatchPreset(WorldModifierDraft draft)
	{
		string[] array = new string[5] { "Normal", "Casual", "Easy", "Hard", "Hardcore" };
		foreach (string text in array)
		{
			if (draft.SameSteps(Preset(text)))
			{
				return text;
			}
		}
		return "Custom";
	}

	/// <summary>Console and action-log line after a successful Apply.</summary>
	public static string SuccessMessage(WorldModifierDraft draft)
	{
		return Line("World modifiers:", draft);
	}

	/// <summary>Console line when Apply failed and the restore also failed, naming what the world now holds.</summary>
	public static string FailedRestoreMessage(WorldModifierDraft draft)
	{
		return Line("World modifiers failed.", draft);
	}

	/// <summary>Step names for a family, left to right. An unknown family name returns the Portals list.</summary>
	public static IReadOnlyList<string> Stops(string modifier)
	{
		return modifier switch
		{
			"Combat" => CombatStops, 
			"Death" => DeathStops, 
			"Resources" => ResourceStops, 
			"Raids" => RaidStops, 
			_ => PortalStops, 
		};
	}

	/// <summary>
	/// Index of a step name. An unknown name is the leftmost stop.
	/// </summary>
	public static int StopIndex(string modifier, string stop)
	{
		IReadOnlyList<string> stops = Stops(modifier);
		for (int i = 0; i < stops.Count; i++)
		{
			if (stops[i] == stop)
			{
				return i;
			}
		}
		return 0;
	}

	/// <summary>
	/// Horizontal fraction of a step. The leftmost stop is 0.
	/// </summary>
	public static float StopFraction(int index, int count)
	{
		int max = (count > 1) ? (count - 1) : 0;
		int clamped = (index < 0) ? 0 : ((index > max) ? max : index);
		return (max <= 0) ? 0f : ((float)clamped / (float)max);
	}

	/// <summary>
	/// True when the slider value or the handle anchor is not on the saved step.
	/// A matching value with the handle still at the left still needs a place.
	/// </summary>
	public static bool HandleNeedsPlace(int sliderValue, float anchorX, int index, int count)
	{
		int max = (count > 1) ? (count - 1) : 0;
		int clamped = (index < 0) ? 0 : ((index > max) ? max : index);
		if (sliderValue != clamped)
		{
			return true;
		}
		return Math.Abs(anchorX - StopFraction(clamped, count)) > 0.04f;
	}

	private static string Line(string prefix, WorldModifierDraft draft)
	{
		return prefix + " Combat " + draft.Combat + ", Death penalty " + draft.Death + ", Resources " + draft.Resources + ", Raids " + draft.Raids + ", Portals " + draft.Portals + ". Skill loss overwrite " + (draft.Overwrite ? "on" : "off") + ".";
	}

	private static string MatchFamily(string family, string[] stops, List<string> managed)
	{
		foreach (string text in stops)
		{
			if (SameSet(Bundle(family, text), managed.Where((string key) => FamilyOwns(family, key)).ToList()))
			{
				return text;
			}
		}
		return "Normal";
	}

	private static bool FamilyOwns(string family, string key)
	{
		string value = key.Split(new char[1] { ' ' })[0];
		return family switch
		{
			"Combat" => Contains(CombatKeys, value), 
			"Death" => Contains(DeathKeys, value), 
			"Resources" => Contains(ResourceKeys, value), 
			"Raids" => Contains(RaidKeys, value), 
			_ => Contains(PortalKeys, value), 
		};
	}

	private static string[] Bundle(string family, string stop)
	{
		string[] value;
		return Bundles.TryGetValue(family + "\n" + stop, out value) ? value : new string[0];
	}

	private static bool SameSet(string[] expected, List<string> actual)
	{
		if (expected.Length != actual.Count)
		{
			return false;
		}
		foreach (string key in expected)
		{
			if (!actual.Contains(Canon(key)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool Known(string[] stops, string stop)
	{
		return Contains(stops, stop);
	}

	private static bool Contains(string[] values, string value)
	{
		return Array.IndexOf(values, value) >= 0;
	}

	private static string Canon(string key)
	{
		string[] array = key.Trim().Split(new char[1] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
		string text = array[0].ToLowerInvariant();
		return (array.Length == 1) ? text : (text + " " + array[1].Trim());
	}
}

using Object = UnityEngine.Object;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Managers;
using UnityEngine;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public static class ConsoleManager
{
	private static readonly KeyboardShortcut OpenKey = new KeyboardShortcut((KeyCode)270, Array.Empty<KeyCode>());

	private static Plugin _plugin = null;

	private static ConfigEntry<KeyboardShortcut> _toggle = null;

	private static ConfigEntry<bool> _fly = null;

	private static ConsoleView _view = null;

	private static bool _open;

	private static float _nextRefresh;

	private static bool _loggedWaiting;

	private static bool _flyApplied;

	public static void Create(Plugin plugin)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		_plugin = plugin;
		_toggle = ((BaseUnityPlugin)plugin).Config.Bind<KeyboardShortcut>("Input", "TogglePanel", OpenKey, "Open or close the QoL panel. Every player gets the Pick tab; admins get the rest. Default is numpad + or Shift and the =/+ key.");
		KeyboardShortcut value = _toggle.Value;
		if (PanelHotkey.IsLegacyDefault(((object)value.MainKey/*cast due to .constrained prefix*/).ToString(), (IReadOnlyCollection<string>)(object)ModifierNames(_toggle.Value)))
		{
			_toggle.Value = OpenKey;
		}
		_fly = ((BaseUnityPlugin)plugin).Config.Bind<bool>("Cheats", "Fly", false, "Fly mode on this character. Stays off until an admin turns it on.");
		Logger.LogInfo((object)"QoL panel hotkey is numpad + or Shift and the =/+ key.");
		if (!GUIManager.IsHeadless())
		{
			InputManager.Instance.AddButton("valheim.qol.cm", new ButtonConfig
			{
				Name = "ToggleQoLPanel",
				ShortcutConfig = _toggle,
				ActiveInCustomGUI = true,
				Hint = "QoL panel",
				BlockOtherInputs = false
			});
			_view = new ConsoleView();
			GUIManager.OnCustomGUIAvailable += Build;
			Build();
		}
	}

	public static void Show(string message)
	{
		PluginStorage.Action(message);
		if (_view != null)
		{
			_view.SetStatus(message);
		}
		else if ((Object)(object)_plugin != (Object)null)
		{
			Logger.LogInfo((object)message);
		}
	}

	public static void RememberFly(bool enabled)
	{
		if (_fly != null)
		{
			_fly.Value = enabled;
		}
	}

	private static void ApplySavedFly()
	{
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			_flyApplied = false;
		}
		else if (!_flyApplied && _fly != null && !((Character)localPlayer).IsDead() && Plugin.LocalIsAdmin())
		{
			_flyApplied = true;
			GameplayModifiers.Set(PlayerMode.Fly, SavedFly.Choose(_fly.Value));
			if (_view != null)
			{
				_view.RefreshModes();
			}
		}
	}

	private static void Build()
	{
		if ((Object)(object)GUIManager.CustomGUIFront == (Object)null || _view == null)
		{
			return;
		}
		try
		{
			_view.Build(GUIManager.CustomGUIFront.transform);
		}
		finally
		{
			if (_view != null && _view.IsBuilt)
			{
				_view.SetVisible(_open);
			}
		}
		if (_open)
		{
			GUIManager.BlockInput(true);
		}
	}

	internal static void SyncWorldMarks()
	{
		if (_open && _view != null)
		{
			_view.SyncWorldMarks();
		}
	}

	internal static void Tick()
	{
		if (_view == null)
		{
			return;
		}
		EnsureBuilt();
		ApplySavedFly();
		if (!WasPressed())
		{
			RefreshWhileOpen();
		}
		else if (_open || (_view != null && _view.IsVisible))
		{
			SetOpen(open: false);
		}
		else if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			if (!_loggedWaiting)
			{
				_loggedWaiting = true;
				Logger.LogInfo((object)"QoL panel waits until a character is in the world.");
			}
		}
		else
		{
			_view.SetAdmin(Plugin.LocalIsAdmin());
			if (!_view.IsBuilt)
			{
				Logger.LogWarning((object)"QoL panel could not be created.");
			}
			else
			{
				SetOpen(!_open);
			}
		}
	}

	private static void SetOpen(bool open)
	{
		_open = open;
		if (open)
		{
			_view.SetAdmin(Plugin.LocalIsAdmin());
		}
		_view.SetVisible(open);
		GUIManager.BlockInput(open);
		Logger.LogInfo((object)(open ? "QoL panel opened." : "QoL panel closed."));
		if (open)
		{
			_view.BringToFront();
			Cursor.lockState = (CursorLockMode)0;
			Cursor.visible = true;
			_view.RefreshPlayers();
			_view.RefreshModes();
		}
	}

	private static void RefreshWhileOpen()
	{
		if (!_open || _view == null || !_view.IsBuilt)
		{
			return;
		}
		if (Input.GetKeyDown((KeyCode)27))
		{
			if (!_view.HideDropdown())
			{
				SetOpen(open: false);
			}
			return;
		}
		if (Time.unscaledTime < _nextRefresh)
		{
			return;
		}
		_nextRefresh = Time.unscaledTime + 1f;
		if (_view.SetAdmin(Plugin.LocalIsAdmin()))
		{
			_view.SetVisible(true);
			_view.BringToFront();
		}
		_view.RefreshPlayers();
		_view.RefreshModes();
	}

	private static void EnsureBuilt()
	{
		if (_view == null || _view.IsBuilt || (Object)(object)GUIManager.CustomGUIFront == (Object)null)
		{
			return;
		}
		try
		{
			_view.Build(GUIManager.CustomGUIFront.transform);
		}
		finally
		{
			if (_view != null && _view.IsBuilt)
			{
				_view.SetVisible(_open);
			}
		}
	}

	private static bool WasPressed()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		KeyboardShortcut value;
		if (_toggle != null)
		{
			value = _toggle.Value;
			if (PanelHotkey.IsPlusBinding(((object)value.MainKey/*cast due to .constrained prefix*/).ToString(), (IReadOnlyCollection<string>)(object)ModifierNames(_toggle.Value)))
			{
				return PlusPressed() || ZInput.GetButtonDown("ToggleQoLPanel");
			}
		}
		int result;
		if (_toggle != null)
		{
			value = _toggle.Value;
			if (value.IsDown())
			{
				result = 1;
				goto IL_0083;
			}
		}
		result = (ZInput.GetButtonDown("ToggleQoLPanel") ? 1 : 0);
		goto IL_0083;
		IL_0083:
		return (byte)result != 0;
	}

	private static bool PlusPressed()
	{
		if (Input.GetKeyDown((KeyCode)270) || Input.GetKeyDown((KeyCode)43))
		{
			return true;
		}
		return (Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303)) && Input.GetKeyDown((KeyCode)61);
	}

	private static string[] ModifierNames(KeyboardShortcut shortcut)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		foreach (KeyCode modifier in shortcut.Modifiers)
		{
			list.Add(((object)modifier/*cast due to .constrained prefix*/).ToString());
		}
		return list.ToArray();
	}
}

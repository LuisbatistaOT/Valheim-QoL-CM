using Object = UnityEngine.Object;
using FitMode = UnityEngine.UI.ContentSizeFitter.FitMode;
using Direction = UnityEngine.UI.Scrollbar.Direction;
using MovementType = UnityEngine.UI.ScrollRect.MovementType;
using ScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility;
using Mode = UnityEngine.UI.Navigation.Mode;
using ContentType = UnityEngine.UI.InputField.ContentType;
using Constraint = UnityEngine.UI.GridLayoutGroup.Constraint;
using Logger = Jotunn.Logger;
using ModifierRules = ValheimQoLCM.Core.WorldModifiers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Jotunn;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

public sealed class ConsoleView
{
	private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

	private static readonly Color SelectedRow = new Color(0.95f, 0.55f, 0.15f, 1f);

	private static readonly Color DisabledTint = new Color(0.566f, 0.566f, 0.566f, 0.5f);

	private const float PanelWidth = 640f;

	private const float PanelHeight = 720f;

	private const int MaxLogLines = 40;

	private const string CreditsUrl = "https://github.com/LuisbatistaOT/Valheim-QoL-CM";

	private static readonly string[] PageNames = new string[5] { "Cheats", "World", "Spawn", "Players", "Log" };

	private static readonly string[] PresetNames = new string[5] { "Normal", "Casual", "Easy", "Hard", "Hardcore" };

	private static readonly string[] ModifierNames = new string[5] { "Combat", "Death", "Resources", "Raids", "Portals" };

	private readonly Dictionary<PlayerMode, Text> _modeLabels = new Dictionary<PlayerMode, Text>();

	private readonly Dictionary<string, Button> _playerButtons = new Dictionary<string, Button>();

	private readonly Dictionary<string, Button> _itemButtons = new Dictionary<string, Button>();

	private readonly List<string> _logLines = new List<string>();

	private readonly List<Selectable> _playerActions = new List<Selectable>();

	private readonly List<Selectable> _spawnControls = new List<Selectable>();

	private readonly List<Button> _tabs = new List<Button>();

	private readonly List<Button> _presetButtons = new List<Button>();

	private readonly List<Slider> _modifierSliders = new List<Slider>();

	private readonly List<Text> _modifierReadouts = new List<Text>();

	private readonly List<string> _shownStops = new List<string>();

	private readonly List<RectTransform> _stepKnobs = new List<RectTransform>();

	private GameObject _root = null;

	private RectTransform _pagesRoot = null;

	private RectTransform _tabRow = null;

	private int _tab;

	private Text _console = null;

	private RectTransform _players = null;

	private RectTransform _items = null;

	private InputField _filter = null;

	private InputField _steamId = null;

	private InputField _quantityField = null;

	private InputField _qualityField = null;

	private GameObject _steamRow = null;

	private string? _selected;

	private bool _selectedIsSelf;

	private string? _item;

	private string _listedPlayers = string.Empty;

	private int _quantity = 1;

	private int _quality = 1;

	private bool _confirmGrant;

	private Button _overwriteButton = null;

	private Button _applyWorld = null;

	private Text _customLabel = null;

	private WorldModifierDraft _staged = ModifierRules.Normal();

	private WorldModifierDraft _applied = ModifierRules.Normal();

	private bool _paintingWorld;

	private bool _syncingSliders;

	private int _ignoreSliderFrame = -1;

	public bool IsBuilt => (Object)(object)_root != (Object)null;

	public bool IsVisible => (Object)(object)_root != (Object)null && _root.activeSelf;

	public void Build(Transform parent)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Expected O, but got Unknown
		if ((Object)(object)_root != (Object)null)
		{
			Object.Destroy((Object)(object)_root);
		}
		// A scene change destroys the old root before this runs. Clear the row
		// lists every time, or the new rows land behind the dead ones and every
		// paint loop stops at the dead five.
		_modeLabels.Clear();
		_playerButtons.Clear();
		_itemButtons.Clear();
		_playerActions.Clear();
		_spawnControls.Clear();
		_tabs.Clear();
		_presetButtons.Clear();
		_modifierSliders.Clear();
		_modifierReadouts.Clear();
		_shownStops.Clear();
		_stepKnobs.Clear();
		_root = GUIManager.Instance.CreateWoodpanel(parent, Center, Center, Vector2.zero, 640f, 720f, false);
		Text val = AddText("Valheim QoL - CM", _root.transform, 22, 420f, 32f);
		val.alignment = (TextAnchor)4;
		AnchorTopCenter(((Graphic)val).rectTransform, 0f, 8f, 420f, 32f);
		Text val2 = AddText("Version 1.8", _root.transform, 16, 120f, 24f);
		val2.alignment = (TextAnchor)5;
		AnchorTopRight(((Graphic)val2).rectTransform, 16f, 12f, 120f, 24f);
		_tabRow = AddTabRow();
		AddTab(_tabRow, "Cheats", 0);
		AddTab(_tabRow, "World", 1);
		AddTab(_tabRow, "Spawn", 2);
		AddTab(_tabRow, "Players", 3);
		AddTab(_tabRow, "Log", 4);
		RectTransform parent2 = (_pagesRoot = CreateStretch("Pages", _root.transform, 16f, 44f, 16f, 90f));
		RectTransform body = AddPage(parent2, "Cheats");
		BuildCheats(body);
		try
		{
			BuildWorld(AddPage(parent2, "World"));
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("QoL world tab failed: " + ex));
		}
		BuildItems(AddPage(parent2, "Spawn"));
		BuildPlayers(AddPage(parent2, "Players"));
		BuildLog(AddPage(parent2, "Log"));
		ShowTab(0);
		GameObject val3 = GUIManager.Instance.CreateButton("By Alfamud", _root.transform, Center, Center, Vector2.zero, 140f, 28f);
		StretchText(val3);
		AnchorBottomRight(val3.GetComponent<RectTransform>(), 16f, 8f, 140f, 28f);
		val3.GetComponent<Button>().onClick.AddListener(delegate
		{
			Application.OpenURL("https://github.com/LuisbatistaOT/Valheim-QoL-CM");
		});
		((Transform)_tabRow).SetAsLastSibling();
		PaintConsole();
		ApplyPlayerActions();
		ApplySpawnEnabled();
		_root.SetActive(false);
		RefreshItems();
	}

	public void BringToFront()
	{
		if ((Object)(object)_root != (Object)null)
		{
			_root.transform.SetAsLastSibling();
		}
	}

	public void SetVisible(bool visible)
	{
		if ((Object)(object)_root != (Object)null)
		{
			_root.SetActive(visible);
			if (visible)
			{
				ShowTab(_tab);
			}
		}
		if (visible)
		{
			_listedPlayers = string.Empty;
			_confirmGrant = false;
		}
	}

	public void SetStatus(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			_logLines.Add(message);
			while (_logLines.Count > 40)
			{
				_logLines.RemoveAt(0);
			}
			PaintConsole();
		}
	}

	public void RefreshPlayers()
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		if ((Object)(object)_players == (Object)null)
		{
			return;
		}
		IReadOnlyList<ConnectedPlayer> readOnlyList = AdminCommands.ListConnected();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ConnectedPlayer item in readOnlyList)
		{
			stringBuilder.Append(item.Name).Append('|').Append(item.SteamId)
				.Append(';');
		}
		string text = stringBuilder.ToString();
		if (text == _listedPlayers)
		{
			return;
		}
		_listedPlayers = text;
		Clear(_players);
		_playerButtons.Clear();
		bool flag = false;
		foreach (ConnectedPlayer item2 in readOnlyList)
		{
			ConnectedPlayer captured = item2;
			if (captured.Name == _selected)
			{
				flag = true;
			}
			string caption = (captured.IsSelf ? (captured.Name + " (you)") : captured.Name);
			Button val = AddRowButton(_players, caption);
			((UnityEvent)val.onClick).AddListener((UnityAction)delegate
			{
				Select(captured);
			});
			_playerButtons[captured.Name] = val;
		}
		if (!flag)
		{
			_confirmGrant = false;
			if ((Object)(object)_steamRow != (Object)null)
			{
				_steamRow.SetActive(false);
			}
			ConnectedPlayer connectedPlayer = null;
			foreach (ConnectedPlayer item3 in readOnlyList)
			{
				if (item3.IsSelf)
				{
					connectedPlayer = item3;
					break;
				}
			}
			if (connectedPlayer != null)
			{
				_selected = connectedPlayer.Name;
				_selectedIsSelf = true;
			}
			else
			{
				_selected = null;
				_selectedIsSelf = false;
			}
		}
		Highlight(_playerButtons, _selected);
		ApplyPlayerActions();
	}

	public void RefreshModes()
	{
		foreach (KeyValuePair<PlayerMode, Text> modeLabel in _modeLabels)
		{
			modeLabel.Value.text = modeLabel.Key.ToString() + ": " + (GameplayModifiers.IsEnabled(modeLabel.Key) ? "On" : "Off");
		}
	}

	private void BuildPlayers(RectTransform body)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		_players = AddScrollList((Transform)(object)body, alwaysShowBar: true, fasterItemScroll: false);
		_playerActions.Add((Selectable)(object)AddFlexButton((Transform)(object)body, "Bring me to player", (UnityAction)delegate
		{
			ShowFailure(AdminCommands.RequestBringMe(_selected));
		}, 0f));
		_playerActions.Add((Selectable)(object)AddFlexButton((Transform)(object)body, "Bring player to me", (UnityAction)delegate
		{
			ShowFailure(AdminCommands.RequestBringTarget(_selected));
		}, 0f));
		_playerActions.Add((Selectable)(object)AddFlexButton((Transform)(object)body, "Grant admin", new UnityAction(Grant), 0f));
		_steamId = AddField((Transform)(object)body, "Steam ID", 0f);
		_steamRow = ((Component)_steamId).gameObject;
		_steamRow.SetActive(false);
	}

	private void BuildCheats(RectTransform body)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		GameObject val = new GameObject("CheatGrid", new Type[3]
		{
			typeof(RectTransform),
			typeof(GridLayoutGroup),
			typeof(LayoutElement)
		});
		val.transform.SetParent((Transform)(object)body, false);
		GridLayoutGroup component = val.GetComponent<GridLayoutGroup>();
		component.constraint = (Constraint)1;
		component.constraintCount = 2;
		component.cellSize = new Vector2(200f, 34f);
		component.spacing = new Vector2(8f, 8f);
		((LayoutGroup)component).childAlignment = (TextAnchor)1;
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.preferredHeight = 118f;
		component2.minHeight = 118f;
		component2.flexibleWidth = 1f;
		AddModeButton(PlayerMode.God, "God", val.transform);
		AddModeButton(PlayerMode.Fly, "Fly", val.transform);
		AddModeButton(PlayerMode.Creative, "Creative", val.transform);
		AddModeButton(PlayerMode.FreeCam, "Free cam", val.transform);
		AddModeButton(PlayerMode.Ghost, "Ghost", val.transform);
		RectTransform parent = AddRow((Transform)(object)body, (TextAnchor)4);
		AddFlexButton((Transform)(object)parent, "Tame", (UnityAction)delegate
		{
			ShowFailure(NearbyCommands.RequestTame());
		}, 140f);
		AddFlexButton((Transform)(object)parent, "Kill enemies", (UnityAction)delegate
		{
			ShowFailure(NearbyCommands.RequestKillEnemies());
		}, 180f);
	}

	private void BuildWorld(RectTransform body)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Expected O, but got Unknown
		RectTransform parent = AddRow((Transform)(object)body, (TextAnchor)4);
		string[] presetNames = PresetNames;
		foreach (string text in presetNames)
		{
			string captured = text;
			_presetButtons.Add(AddFlexButton((Transform)(object)parent, captured, (UnityAction)delegate
			{
				StagePreset(captured);
			}, 108f));
		}
		_customLabel = AddLayoutText((Transform)(object)body, "Custom", 14, new Color(0.9f, 0.86f, 0.75f, 1f));
		string[] modifierNames = ModifierNames;
		foreach (string text2 in modifierNames)
		{
			string captured2 = text2;
			RectTransform val = AddRow((Transform)(object)body, (TextAnchor)3);
			AddFixedLabel((Transform)(object)val, ModifierCaption(captured2), 120f);
			Slider val2 = AddStepSlider((Transform)(object)val);
			IReadOnlyList<string> readOnlyList = ModifierRules.Stops(captured2);
			val2.minValue = 0f;
			val2.maxValue = readOnlyList.Count - 1;
			val2.wholeNumbers = true;
			((UnityEvent<float>)(object)val2.onValueChanged).AddListener((UnityAction<float>)delegate(float value)
			{
				StageStop(captured2, Mathf.RoundToInt(value));
			});
			_modifierSliders.Add(val2);
			_modifierReadouts.Add(AddReadout((Transform)(object)val, StopFor(captured2), 120f));
		}
		_overwriteButton = AddFlexButton((Transform)(object)body, "Overwrite Skill loss to 0%", new UnityAction(StageOverwrite), 280f);
		RectTransform parent2 = AddRow((Transform)(object)body, (TextAnchor)4);
		_applyWorld = AddFlexButton((Transform)(object)parent2, "Apply", new UnityAction(ApplyWorld), 180f);
		PaintWorld();
	}

	public void ShowWorld(WorldModifierDraft applied)
	{
		if (_staged.SameAs(_applied))
		{
			_applied = applied;
			_staged = applied;
			PaintWorld();
		}
	}

	private void LoadWorld()
	{
		ActionResult<WorldModifierDraft> actionResult = WorldModifierHost.ReadApplied();
		if (!actionResult.Ok)
		{
			if (actionResult.Error != null)
			{
				SetStatus(actionResult.Error);
			}
			return;
		}
		_applied = actionResult.Data;
		_staged = actionResult.Data;
		PluginStorage.Debug("World tab Combat " + _staged.Combat + ", Death penalty " + _staged.Death + ", Resources " + _staged.Resources + ", Raids " + _staged.Raids + ", Portals " + _staged.Portals + ". Keys " + ManagedKeyText() + ".");
		PaintWorld();
		PluginStorage.Debug("World readout " + ReadoutText() + ". Rows " + _modifierSliders.Count + ", marks " + MarkText() + ".");
	}

	private void StagePreset(string name)
	{
		_staged = _staged.ChoosePreset(name);
		PaintWorld();
	}

	private void StageStop(string modifier, int index)
	{
		if (_paintingWorld || _syncingSliders || Time.frameCount == _ignoreSliderFrame)
		{
			return;
		}
		IReadOnlyList<string> readOnlyList = ModifierRules.Stops(modifier);
		if (index >= 0 && index < readOnlyList.Count)
		{
			string stop = readOnlyList[index];
			switch (modifier)
			{
			case "Combat":
				_staged = _staged.MoveCombat(stop);
				break;
			case "Death":
				_staged = _staged.MoveDeath(stop);
				break;
			case "Resources":
				_staged = _staged.MoveResources(stop);
				break;
			case "Raids":
				_staged = _staged.MoveRaids(stop);
				break;
			default:
				_staged = _staged.MovePortals(stop);
				break;
			}
			PaintWorld();
		}
	}

	private void StageOverwrite()
	{
		_staged = _staged.WithOverwrite(!_staged.Overwrite);
		PaintWorld();
	}

	private void ApplyWorld()
	{
		ActionResult<WorldModifierDraft> actionResult = WorldModifierHost.RequestApply(_staged);
		if (!actionResult.Ok)
		{
			ShowFailure(actionResult);
			return;
		}
		ActionResult<WorldModifierDraft> actionResult2 = WorldModifierHost.ReadApplied();
		if (actionResult2.Ok && actionResult2.Data.SameAs(_staged))
		{
			_applied = actionResult2.Data;
			PaintWorld();
		}
	}

	private void PaintWorld()
	{
		if (_presetButtons.Count == 0)
		{
			return;
		}
		_paintingWorld = true;
		_syncingSliders = true;
		_ignoreSliderFrame = Time.frameCount;
		try
		{
			try
			{
				int num = ((_presetButtons.Count < PresetNames.Length) ? _presetButtons.Count : PresetNames.Length);
				for (int i = 0; i < num; i++)
				{
					PaintSelectable((Selectable)(object)_presetButtons[i], _staged.PresetName == PresetNames[i]);
				}
			}
			catch (Exception ex)
			{
				Logger.LogWarning((object)("QoL world preset failed: " + ex));
			}
			if ((Object)(object)_customLabel != (Object)null)
			{
				try
				{
					bool custom = _staged.PresetName == "Custom";
					_customLabel.text = custom ? "Custom" : string.Empty;
					((Component)_customLabel).gameObject.SetActive(custom);
				}
				catch (Exception ex2)
				{
					Logger.LogWarning((object)("QoL world custom label failed: " + ex2.Message));
				}
			}
			for (int j = 0; j < ModifierNames.Length && j < _modifierSliders.Count; j++)
			{
				IReadOnlyList<string> readOnlyList = ModifierRules.Stops(ModifierNames[j]);
				string stop = StopFor(ModifierNames[j]);
				int index = ModifierRules.StopIndex(ModifierNames[j], stop);
				try
				{
					ShowStop(j, stop);
				}
				catch (Exception ex3)
				{
					Logger.LogWarning((object)("QoL world name failed: " + ex3));
				}
				try
				{
					ApplySliderValue(_modifierSliders[j], index, readOnlyList.Count);
				}
				catch (Exception ex4)
				{
					Logger.LogWarning((object)("QoL world value failed: " + ex4));
				}
				try
				{
					PlaceStep(j, index, readOnlyList.Count);
				}
				catch (Exception ex5)
				{
					Logger.LogWarning((object)("QoL world step failed: " + ex5));
				}
			}
			PaintSelectable((Selectable)(object)_overwriteButton, _staged.Overwrite);
		}
		finally
		{
			_paintingWorld = false;
			_syncingSliders = false;
			if ((Object)(object)_applyWorld != (Object)null)
			{
				((Selectable)_applyWorld).interactable = !_staged.SameAs(_applied);
			}
		}
	}

	private void ShowStop(int index, string stop)
	{
		if (index < 0 || index >= _modifierReadouts.Count || string.IsNullOrEmpty(stop))
		{
			return;
		}
		Text label = _modifierReadouts[index];
		if ((Object)(object)label == (Object)null)
		{
			return;
		}
		while (_shownStops.Count <= index)
		{
			_shownStops.Add(string.Empty);
		}
		if (_shownStops[index] == stop && label.text == stop)
		{
			return;
		}
		Font font = label.font;
		if ((Object)(object)font != (Object)null)
		{
			font.RequestCharactersInTexture(stop, label.fontSize, label.fontStyle);
		}
		label.enabled = false;
		label.text = stop;
		label.enabled = true;
		label.SetAllDirty();
		label.cachedTextGenerator.Invalidate();
		_shownStops[index] = stop;
	}

	private string MarkText()
	{
		List<string> marks = new List<string>();
		for (int i = 0; i < _modifierSliders.Count; i++)
		{
			Slider slider = _modifierSliders[i];
			if ((Object)(object)slider == (Object)null)
			{
				marks.Add("dead");
				continue;
			}
			float anchor = (((Object)(object)slider.handleRect != (Object)null) ? slider.handleRect.anchorMin.x : -1f);
			marks.Add(Mathf.RoundToInt(slider.value) + "@" + anchor.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
		}
		return string.Join(", ", marks);
	}

	private string ReadoutText()
	{
		List<string> words = new List<string>();
		for (int i = 0; i < _modifierReadouts.Count; i++)
		{
			Text label = _modifierReadouts[i];
			words.Add(((Object)(object)label == (Object)null) ? string.Empty : label.text);
		}
		return string.Join(", ", words);
	}

	private static string ManagedKeyText()
	{
		if ((Object)(object)ZoneSystem.instance == (Object)null)
		{
			return string.Empty;
		}
		List<string> list = new List<string>();
		foreach (string globalKey in ZoneSystem.instance.GetGlobalKeys())
		{
			if (ModifierRules.IsManagedKey(globalKey))
			{
				list.Add(globalKey);
			}
		}
		return string.Join(", ", list);
	}

	private static void ApplySliderValue(Slider slider, int index, int count)
	{
		if ((Object)(object)slider == (Object)null)
		{
			return;
		}
		int max = ((count > 1) ? (count - 1) : 0);
		int clamped = ((index >= 0) ? ((index > max) ? max : index) : 0);
		slider.wholeNumbers = true;
		slider.minValue = 0f;
		slider.maxValue = max;
		slider.SetValueWithoutNotify(clamped);
		float fraction = ModifierRules.StopFraction(clamped, count);
		FitMark(slider);
		if ((Object)(object)slider.handleRect == (Object)null)
		{
			return;
		}
		RectTransform handle = slider.handleRect;
		handle.anchorMin = new Vector2(fraction, 0f);
		handle.anchorMax = new Vector2(fraction, 1f);
	}

	private static void FitMark(Slider slider)
	{
		if ((Object)(object)slider == (Object)null || (Object)(object)slider.handleRect == (Object)null)
		{
			return;
		}
		RectTransform handle = slider.handleRect;
		RectTransform area = ((Transform)handle).parent as RectTransform;
		if ((Object)(object)area != (Object)null)
		{
			area.anchorMin = new Vector2(0f, 0.5f);
			area.anchorMax = new Vector2(1f, 0.5f);
			area.pivot = new Vector2(0.5f, 0.5f);
			area.offsetMin = new Vector2(8f, -6f);
			area.offsetMax = new Vector2(-8f, 6f);
		}
		handle.anchorMin = new Vector2(handle.anchorMin.x, 0f);
		handle.anchorMax = new Vector2(handle.anchorMax.x, 1f);
		handle.sizeDelta = new Vector2(14f, 0f);
		handle.anchoredPosition = new Vector2(0f, 0f);
		Image image = ((Component)handle).GetComponent<Image>();
		if ((Object)(object)image == (Object)null)
		{
			return;
		}
		Sprite sprite = GUIManager.Instance.GetSprite("UISprite");
		if ((Object)(object)sprite != (Object)null)
		{
			image.sprite = sprite;
		}
		image.type = Image.Type.Sliced;
		((Graphic)image).color = SelectedRow;
		((Graphic)image).raycastTarget = true;
	}

	private void PlaceStep(int row, int index, int count)
	{
		if (row < 0 || row >= _modifierSliders.Count)
		{
			return;
		}
		ApplySliderValue(_modifierSliders[row], index, count);
	}

	public void SyncWorldMarks()
	{
		if (!WorldPageOpen() || _paintingWorld || _syncingSliders)
		{
			return;
		}
		_syncingSliders = true;
		try
		{
			for (int i = 0; i < ModifierNames.Length && i < _modifierSliders.Count; i++)
			{
				Slider slider = _modifierSliders[i];
				if ((Object)(object)slider == (Object)null)
				{
					continue;
				}
				IReadOnlyList<string> stops = ModifierRules.Stops(ModifierNames[i]);
				string stop = StopFor(ModifierNames[i]);
				int index = ModifierRules.StopIndex(ModifierNames[i], stop);
				ShowStop(i, stop);
				float anchor = (((Object)(object)slider.handleRect != (Object)null) ? slider.handleRect.anchorMin.x : 0f);
				if (ModifierRules.HandleNeedsPlace(Mathf.RoundToInt(slider.value), anchor, index, stops.Count))
				{
					ApplySliderValue(slider, index, stops.Count);
				}
				else
				{
					FitMark(slider);
				}
			}
		}
		finally
		{
			_syncingSliders = false;
		}
	}

	private bool WorldPageOpen()
	{
		return (Object)(object)_root != (Object)null && _root.activeSelf && _tab >= 0 && _tab < PageNames.Length && PageNames[_tab] == "World";
	}

	private string StopFor(string modifier)
	{
		return modifier switch
		{
			"Combat" => _staged.Combat, 
			"Death" => _staged.Death, 
			"Resources" => _staged.Resources, 
			"Raids" => _staged.Raids, 
			_ => _staged.Portals, 
		};
	}

	private static string ModifierCaption(string modifier)
	{
		return (modifier == "Death") ? "Death penalty" : modifier;
	}

	private static void PaintSelectable(Selectable selectable, bool selected)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)selectable == (Object)null))
		{
			ColorBlock colors = selectable.colors;
			Color selectedColor = (colors.pressedColor = (colors.highlightedColor = (colors.normalColor = (selected ? SelectedRow : Color.white))));
			colors.selectedColor = selectedColor;
			colors.disabledColor = DisabledTint;
			selectable.colors = colors;
		}
	}

	private void BuildItems(RectTransform body)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Expected O, but got Unknown
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("ItemBlock", new Type[3]
		{
			typeof(RectTransform),
			typeof(VerticalLayoutGroup),
			typeof(LayoutElement)
		});
		val.transform.SetParent((Transform)(object)body, false);
		VerticalLayoutGroup component = val.GetComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)component).spacing = 0f;
		((LayoutGroup)component).childAlignment = (TextAnchor)1;
		((HorizontalOrVerticalLayoutGroup)component).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)component).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)component).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)component).childForceExpandHeight = false;
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.flexibleWidth = 1f;
		component2.flexibleHeight = 1f;
		component2.minHeight = 120f;
		_filter = AddField(val.transform, "Item filter", 0f);
		Image component3 = ((Component)_filter).GetComponent<Image>();
		if ((Object)(object)component3 != (Object)null)
		{
			((Graphic)component3).color = Color.black;
		}
		((UnityEvent<string>)(object)_filter.onValueChanged).AddListener((UnityAction<string>)delegate
		{
			RefreshItems();
		});
		_items = AddScrollList(val.transform, alwaysShowBar: true, fasterItemScroll: true);
		RectTransform parent = AddRow((Transform)(object)body, (TextAnchor)3);
		AddFixedLabel((Transform)(object)parent, "Qty", 36f);
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent, "-", (UnityAction)delegate
		{
			StepQuantity(-1);
		}, 32f));
		_quantityField = AddNumberBox((Transform)(object)parent);
		_spawnControls.Add((Selectable)(object)_quantityField);
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent, "+", (UnityAction)delegate
		{
			StepQuantity(1);
		}, 32f));
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent, "x10", (UnityAction)delegate
		{
			MultiplyQuantity(10);
		}, 52f));
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent, "Max Stack", new UnityAction(SetMaxStack), 108f));
		RectTransform parent2 = AddRow((Transform)(object)body, (TextAnchor)3);
		AddFixedLabel((Transform)(object)parent2, "Quality", 72f);
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent2, "-", (UnityAction)delegate
		{
			StepQuality(-1);
		}, 32f));
		_qualityField = AddNumberBox((Transform)(object)parent2);
		_spawnControls.Add((Selectable)(object)_qualityField);
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent2, "+", (UnityAction)delegate
		{
			StepQuality(1);
		}, 32f));
		PaintNumbers();
		RectTransform parent3 = AddRow((Transform)(object)body, (TextAnchor)4);
		_spawnControls.Add((Selectable)(object)AddFlexButton((Transform)(object)parent3, "Spawn", new UnityAction(Spawn), 180f));
	}

	private void Select(ConnectedPlayer player)
	{
		_selected = player.Name;
		_selectedIsSelf = player.IsSelf;
		_confirmGrant = false;
		bool flag = !_selectedIsSelf && string.IsNullOrEmpty(player.SteamId);
		if ((Object)(object)_steamRow != (Object)null)
		{
			_steamRow.SetActive(flag);
		}
		Highlight(_playerButtons, _selected);
		ApplyPlayerActions();
		SetStatus(flag ? "No Steam ID on this connection. Type one to grant admin." : ("Selected " + player.Name + "."));
	}

	private void RefreshItems()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		if ((Object)(object)_items == (Object)null)
		{
			return;
		}
		Clear(_items);
		_itemButtons.Clear();
		string filter = (((Object)(object)_filter != (Object)null) ? _filter.text : string.Empty);
		foreach (SpawnableItem item in ItemSpawner.FindItems(filter))
		{
			SpawnableItem captured = item;
			Button val = AddRowButton(_items, captured.Label);
			((UnityEvent)val.onClick).AddListener((UnityAction)delegate
			{
				SelectItem(captured);
			});
			_itemButtons[captured.Prefab] = val;
		}
		Highlight(_itemButtons, _item);
		ApplySpawnEnabled();
	}

	private void SelectItem(SpawnableItem item)
	{
		_item = item.Prefab;
		_quality = 1;
		_quantity = 1;
		PaintNumbers();
		Highlight(_itemButtons, _item);
		ApplySpawnEnabled();
		SetStatus("Item " + item.Label + ".");
	}

	private void ApplyPlayerActions()
	{
		SetInteractable(_playerActions, !string.IsNullOrEmpty(_selected) && !_selectedIsSelf);
	}

	private void ApplySpawnEnabled()
	{
		SetInteractable(_spawnControls, !string.IsNullOrEmpty(_item));
	}

	private static void SetInteractable(List<Selectable> controls, bool enabled)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Selectable control in controls)
		{
			if (!((Object)(object)control == (Object)null))
			{
				control.interactable = enabled;
				ColorBlock colors = control.colors;
				colors.disabledColor = DisabledTint;
				control.colors = colors;
			}
		}
	}

	private void StepQuantity(int delta)
	{
		int num = _quantity + delta;
		if (num < 1 || num > 100)
		{
			SetStatus("Invalid quantity.");
			return;
		}
		_quantity = num;
		PaintNumbers();
	}

	private void MultiplyQuantity(int factor)
	{
		int num = _quantity * factor;
		if (num < 1)
		{
			num = 1;
		}
		if (num > 100)
		{
			num = 100;
		}
		if (num == _quantity)
		{
			SetStatus("Invalid quantity.");
			return;
		}
		_quantity = num;
		PaintNumbers();
	}

	private void SetMaxStack()
	{
		int num = (string.IsNullOrEmpty(_item) ? 1 : ItemSpawner.MaxStack(_item));
		if (num < 1)
		{
			num = 1;
		}
		if (num > 100)
		{
			num = 100;
		}
		_quantity = num;
		PaintNumbers();
	}

	private void StepQuality(int delta)
	{
		int num = (string.IsNullOrEmpty(_item) ? 1 : ItemSpawner.MaxQuality(_item));
		if (num < 1)
		{
			num = 1;
		}
		int num2 = _quality + delta;
		if (num2 < 1 || num2 > num)
		{
			SetStatus("Invalid quality.");
			return;
		}
		_quality = num2;
		PaintNumbers();
	}

	private void PaintNumbers()
	{
		if ((Object)(object)_quantityField != (Object)null)
		{
			_quantityField.text = _quantity.ToString();
		}
		if ((Object)(object)_qualityField != (Object)null)
		{
			_qualityField.text = _quality.ToString();
		}
	}

	private void Spawn()
	{
		ShowFailure(ItemSpawner.RequestSpawn(_selected, _item, _quantity, _quality));
	}

	private void Grant()
	{
		if (!_confirmGrant)
		{
			_confirmGrant = true;
			SetStatus("Click Grant admin again to confirm.");
		}
		else
		{
			_confirmGrant = false;
			string typedId = (((Object)(object)_steamId != (Object)null) ? _steamId.text : null);
			ShowFailure(AdminCommands.RequestGrant(_selected ?? string.Empty, typedId));
		}
	}

	private void AddModeButton(PlayerMode mode, string caption, Transform parent)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		GameObject val = GUIManager.Instance.CreateButton(caption, parent, Vector2.zero, Vector2.one, Vector2.zero, 200f, 34f);
		StretchText(val);
		Text componentInChildren = val.GetComponentInChildren<Text>();
		_modeLabels[mode] = componentInChildren;
		componentInChildren.text = caption + ": Off";
		((UnityEvent)val.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
		{
			ActionResult<bool> actionResult = GameplayModifiers.Toggle(mode);
			if (mode == PlayerMode.Fly && actionResult.Ok)
			{
				ConsoleManager.RememberFly(actionResult.Data);
			}
			SetStatus(actionResult.Ok ? (caption + " is " + (actionResult.Data ? "on" : "off") + ".") : (actionResult.Error ?? string.Empty));
			RefreshModes();
		});
	}

	private RectTransform AddTabRow()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		GameObject val = new GameObject("Tabs", new Type[2]
		{
			typeof(RectTransform),
			typeof(HorizontalLayoutGroup)
		});
		val.transform.SetParent(_root.transform, false);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 1f);
		component.anchorMax = new Vector2(1f, 1f);
		component.pivot = new Vector2(0.5f, 1f);
		component.anchoredPosition = new Vector2(0f, -42f);
		component.sizeDelta = new Vector2(-32f, 36f);
		HorizontalLayoutGroup component2 = val.GetComponent<HorizontalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)component2).spacing = 6f;
		((LayoutGroup)component2).padding = new RectOffset(4, 4, 0, 0);
		((LayoutGroup)component2).childAlignment = (TextAnchor)4;
		((HorizontalOrVerticalLayoutGroup)component2).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)component2).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)component2).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)component2).childForceExpandHeight = true;
		return component;
	}

	private void AddTab(RectTransform row, string caption, int index)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		GameObject val = GUIManager.Instance.CreateButton(caption, (Transform)(object)row, Vector2.zero, Vector2.one, Vector2.zero, 120f, 34f);
		StretchText(val);
		LayoutElement val2 = val.AddComponent<LayoutElement>();
		val2.preferredHeight = 34f;
		val2.minHeight = 34f;
		val2.flexibleWidth = 1f;
		Image component = val.GetComponent<Image>();
		if ((Object)(object)component != (Object)null)
		{
			((Graphic)component).raycastTarget = true;
		}
		Button component2 = val.GetComponent<Button>();
		Navigation navigation = ((Selectable)component2).navigation;
		navigation.mode = (Mode)0;
		((Selectable)component2).navigation = navigation;
		int captured = index;
		((UnityEvent)component2.onClick).AddListener((UnityAction)delegate
		{
			ShowTab(captured);
		});
		_tabs.Add(component2);
	}

	private RectTransform AddPage(RectTransform parent, string title)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		GameObject val = new GameObject(title, new Type[2]
		{
			typeof(RectTransform),
			typeof(VerticalLayoutGroup)
		});
		val.transform.SetParent((Transform)(object)parent, false);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		VerticalLayoutGroup component2 = val.GetComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)component2).spacing = 6f;
		((LayoutGroup)component2).padding = new RectOffset(8, 8, 4, 4);
		((LayoutGroup)component2).childAlignment = (TextAnchor)1;
		((HorizontalOrVerticalLayoutGroup)component2).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)component2).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)component2).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)component2).childForceExpandHeight = false;
		return component;
	}

	private void ShowTab(int index)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= PageNames.Length)
		{
			index = 0;
		}
		bool flag = PageNames[_tab] == "World" && PageNames[index] != "World";
		_tab = index;
		string text = PageNames[_tab];
		if (flag)
		{
			_staged = _applied;
		}
		if ((Object)(object)_pagesRoot != (Object)null)
		{
			for (int i = 0; i < ((Transform)_pagesRoot).childCount; i++)
			{
				Transform child = ((Transform)_pagesRoot).GetChild(i);
				if (!((Object)(object)child == (Object)null) && Array.IndexOf<string>(PageNames, ((Object)child).name) >= 0)
				{
					SetPageShown(((Component)child).gameObject, ((Object)child).name == text);
				}
			}
		}
		for (int j = 0; j < _tabs.Count; j++)
		{
			if (!((Object)(object)_tabs[j] == (Object)null))
			{
				ColorBlock colors = ((Selectable)_tabs[j]).colors;
				Color disabledColor = (colors.selectedColor = (colors.pressedColor = (colors.highlightedColor = (colors.normalColor = ((j == index) ? SelectedRow : Color.white)))));
				colors.disabledColor = disabledColor;
				((Selectable)_tabs[j]).colors = colors;
			}
		}
		if ((Object)(object)_pagesRoot != (Object)null)
		{
			try
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_pagesRoot);
			}
			catch (Exception ex)
			{
				Logger.LogWarning((object)("QoL page layout failed: " + ex.Message));
			}
		}
		if (text == "World")
		{
			try
			{
				LoadWorld();
			}
			catch (Exception ex2)
			{
				Logger.LogWarning((object)("QoL world tab failed: " + ex2));
			}
		}
	}

	private static void SetPageShown(GameObject page, bool show)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)page == (Object)null)
		{
			return;
		}
		try
		{
			if (show)
			{
				if (!page.activeSelf)
				{
					page.SetActive(true);
				}
				page.transform.localScale = Vector3.one;
			}
			else
			{
				page.transform.localScale = Vector3.zero;
				if (page.activeSelf)
				{
					page.SetActive(false);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning((object)("QoL page switch failed: " + ex.Message));
		}
	}

	private void BuildLog(RectTransform body)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Log", new Type[4]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(ScrollRect),
			typeof(LayoutElement)
		});
		val.transform.SetParent((Transform)(object)body, false);
		Image component = val.GetComponent<Image>();
		component.sprite = GUIManager.Instance.GetSprite("text_field");
		component.type = Image.Type.Sliced;
		((Graphic)component).color = Color.black;
		((Graphic)component).raycastTarget = false;
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.flexibleWidth = 1f;
		component2.flexibleHeight = 1f;
		component2.minHeight = 240f;
		component2.preferredHeight = 520f;
		GameObject val2 = new GameObject("Viewport", new Type[3]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(RectMask2D)
		});
		val2.transform.SetParent(val.transform, false);
		RectTransform component3 = val2.GetComponent<RectTransform>();
		component3.anchorMin = Vector2.zero;
		component3.anchorMax = Vector2.one;
		component3.offsetMin = new Vector2(8f, 8f);
		component3.offsetMax = new Vector2(-18f, -8f);
		((Graphic)val2.GetComponent<Image>()).color = new Color(0f, 0f, 0f, 0.01f);
		GameObject val3 = new GameObject("Content", new Type[3]
		{
			typeof(RectTransform),
			typeof(VerticalLayoutGroup),
			typeof(ContentSizeFitter)
		});
		val3.transform.SetParent(val2.transform, false);
		RectTransform component4 = val3.GetComponent<RectTransform>();
		component4.anchorMin = new Vector2(0f, 1f);
		component4.anchorMax = new Vector2(1f, 1f);
		component4.pivot = new Vector2(0.5f, 1f);
		VerticalLayoutGroup component5 = val3.GetComponent<VerticalLayoutGroup>();
		((LayoutGroup)component5).childAlignment = (TextAnchor)0;
		((HorizontalOrVerticalLayoutGroup)component5).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)component5).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)component5).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)component5).childForceExpandHeight = false;
		val3.GetComponent<ContentSizeFitter>().verticalFit = (FitMode)2;
		_console = AddLayoutText((Transform)(object)component4, string.Empty, 15, new Color(0.45f, 0.9f, 0.4f, 1f));
		_console.alignment = (TextAnchor)0;
		_console.horizontalOverflow = (HorizontalWrapMode)0;
		GameObject val4 = DefaultControls.CreateScrollbar(GUIManager.Instance.ValheimControlResources);
		val4.transform.SetParent(val.transform, false);
		Scrollbar component6 = val4.GetComponent<Scrollbar>();
		component6.direction = (Direction)2;
		RectTransform component7 = val4.GetComponent<RectTransform>();
		component7.anchorMin = new Vector2(1f, 0f);
		component7.anchorMax = new Vector2(1f, 1f);
		component7.pivot = new Vector2(1f, 0.5f);
		component7.offsetMin = new Vector2(-14f, 2f);
		component7.offsetMax = new Vector2(-2f, -2f);
		ScrollRect component8 = val.GetComponent<ScrollRect>();
		component8.viewport = component3;
		component8.content = component4;
		component8.horizontal = false;
		component8.vertical = true;
		component8.movementType = (MovementType)2;
		component8.verticalScrollbar = component6;
		component8.verticalScrollbarVisibility = (ScrollbarVisibility)0;
		GUIManager.Instance.ApplyScrollRectStyle(component8);
	}

	private RectTransform AddScrollList(Transform parent, bool alwaysShowBar, bool fasterItemScroll)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected O, but got Unknown
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("List", new Type[4]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(ScrollRect),
			typeof(LayoutElement)
		});
		val.transform.SetParent(parent, false);
		LayoutElement component = val.GetComponent<LayoutElement>();
		component.flexibleWidth = 1f;
		component.flexibleHeight = 1f;
		component.minHeight = 72f;
		component.preferredHeight = 320f;
		GameObject val2 = new GameObject("Viewport", new Type[3]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(RectMask2D)
		});
		val2.transform.SetParent(val.transform, false);
		RectTransform component2 = val2.GetComponent<RectTransform>();
		component2.anchorMin = Vector2.zero;
		component2.anchorMax = Vector2.one;
		component2.offsetMin = new Vector2(4f, 4f);
		component2.offsetMax = new Vector2(-16f, -4f);
		Image component3 = val2.GetComponent<Image>();
		((Graphic)component3).color = new Color(0f, 0f, 0f, 0.01f);
		((Graphic)component3).raycastTarget = true;
		GameObject val3 = new GameObject("Content", new Type[3]
		{
			typeof(RectTransform),
			typeof(VerticalLayoutGroup),
			typeof(ContentSizeFitter)
		});
		val3.transform.SetParent(val2.transform, false);
		RectTransform component4 = val3.GetComponent<RectTransform>();
		component4.anchorMin = new Vector2(0f, 1f);
		component4.anchorMax = new Vector2(1f, 1f);
		component4.pivot = new Vector2(0.5f, 1f);
		component4.anchoredPosition = Vector2.zero;
		component4.sizeDelta = new Vector2(0f, 0f);
		VerticalLayoutGroup component5 = val3.GetComponent<VerticalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)component5).spacing = 2f;
		((LayoutGroup)component5).padding = new RectOffset(2, 2, 2, 2);
		((LayoutGroup)component5).childAlignment = (TextAnchor)1;
		((HorizontalOrVerticalLayoutGroup)component5).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)component5).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)component5).childForceExpandWidth = true;
		((HorizontalOrVerticalLayoutGroup)component5).childForceExpandHeight = false;
		ContentSizeFitter component6 = val3.GetComponent<ContentSizeFitter>();
		component6.horizontalFit = (FitMode)0;
		component6.verticalFit = (FitMode)2;
		GameObject val4 = DefaultControls.CreateScrollbar(GUIManager.Instance.ValheimControlResources);
		val4.transform.SetParent(val.transform, false);
		Scrollbar component7 = val4.GetComponent<Scrollbar>();
		component7.direction = (Direction)2;
		RectTransform component8 = val4.GetComponent<RectTransform>();
		component8.anchorMin = new Vector2(1f, 0f);
		component8.anchorMax = new Vector2(1f, 1f);
		component8.pivot = new Vector2(1f, 0.5f);
		component8.offsetMin = new Vector2(-14f, 2f);
		component8.offsetMax = new Vector2(-2f, -2f);
		ScrollRect component9 = val.GetComponent<ScrollRect>();
		component9.viewport = component2;
		component9.content = component4;
		component9.horizontal = false;
		component9.vertical = true;
		component9.movementType = (MovementType)2;
		component9.verticalScrollbar = component7;
		component9.verticalScrollbarVisibility = (ScrollbarVisibility)((!alwaysShowBar) ? 2 : 0);
		GUIManager.Instance.ApplyScrollRectStyle(component9);
		if (fasterItemScroll)
		{
			component9.scrollSensitivity *= 1.1f;
		}
		return component4;
	}

	private Button AddRowButton(RectTransform parent, string caption)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GUIManager.Instance.CreateButton(caption, (Transform)(object)parent, Vector2.zero, Vector2.one, Vector2.zero, 0f, 30f);
		StretchText(val);
		LayoutElement val2 = val.AddComponent<LayoutElement>();
		val2.preferredHeight = 30f;
		val2.minHeight = 30f;
		val2.flexibleWidth = 1f;
		Button component = val.GetComponent<Button>();
		Navigation navigation = ((Selectable)component).navigation;
		navigation.mode = (Mode)0;
		((Selectable)component).navigation = navigation;
		return component;
	}

	private Button AddFlexButton(Transform parent, string caption, UnityAction action, float preferredWidth)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = ((preferredWidth > 0f) ? preferredWidth : 120f);
		GameObject val = GUIManager.Instance.CreateButton(caption, parent, Vector2.zero, Vector2.one, Vector2.zero, num, 32f);
		StretchText(val);
		LayoutElement val2 = val.AddComponent<LayoutElement>();
		val2.preferredHeight = 32f;
		val2.minHeight = 32f;
		if (preferredWidth > 0f)
		{
			val2.preferredWidth = preferredWidth;
			val2.minWidth = preferredWidth;
			val2.flexibleWidth = 0f;
		}
		else
		{
			val2.flexibleWidth = 1f;
		}
		Button component = val.GetComponent<Button>();
		((UnityEvent)component.onClick).AddListener(action);
		return component;
	}

	private InputField AddNumberBox(Transform parent)
	{
		InputField val = AddField(parent, string.Empty, 48f);
		val.readOnly = true;
		val.text = "1";
		if ((Object)(object)val.textComponent != (Object)null)
		{
			val.textComponent.alignment = (TextAnchor)4;
		}
		return val;
	}

	private Text AddReadout(Transform parent, string caption, float width)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Readout", new Type[3]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(LayoutElement)
		});
		val.transform.SetParent(parent, false);
		Image component = val.GetComponent<Image>();
		component.sprite = GUIManager.Instance.GetSprite("text_field");
		component.type = Image.Type.Sliced;
		((Graphic)component).color = new Color(0.12f, 0.1f, 0.08f, 0.95f);
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.preferredWidth = width;
		component2.minWidth = width;
		component2.preferredHeight = 32f;
		component2.flexibleWidth = 0f;
		Text val2 = AddText(caption, val.transform, 16, width, 32f);
		try
		{
			val2.alignment = (TextAnchor)4;
			val2.horizontalOverflow = (HorizontalWrapMode)1;
		}
		catch (Exception)
		{
			return val2;
		}
		return val2;
	}

	private Text AddFixedLabel(Transform parent, string caption, float width)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Text val = AddLayoutText(parent, caption, 16, Color.white);
		LayoutElement component = ((Component)val).gameObject.GetComponent<LayoutElement>();
		component.preferredWidth = width;
		component.minWidth = width;
		component.flexibleWidth = 0f;
		val.alignment = (TextAnchor)4;
		return val;
	}

	private Text AddLayoutText(Transform parent, string caption, int size, Color color)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Text val = AddText(caption, parent, size, 200f, 24f);
		val.alignment = (TextAnchor)4;
		val.horizontalOverflow = (HorizontalWrapMode)1;
		LayoutElement val2 = ((Component)val).gameObject.AddComponent<LayoutElement>();
		val2.preferredHeight = (float)size + 8f;
		val2.minHeight = (float)size + 8f;
		val2.flexibleWidth = 1f;
		((Graphic)val).color = color;
		return val;
	}

	private RectTransform AddRow(Transform parent, TextAnchor alignment)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Row", new Type[3]
		{
			typeof(RectTransform),
			typeof(HorizontalLayoutGroup),
			typeof(LayoutElement)
		});
		val.transform.SetParent(parent, false);
		HorizontalLayoutGroup component = val.GetComponent<HorizontalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)component).spacing = 6f;
		((LayoutGroup)component).childAlignment = alignment;
		((HorizontalOrVerticalLayoutGroup)component).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)component).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)component).childForceExpandWidth = false;
		((HorizontalOrVerticalLayoutGroup)component).childForceExpandHeight = false;
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.preferredHeight = 34f;
		component2.minHeight = 34f;
		component2.flexibleWidth = 1f;
		return val.GetComponent<RectTransform>();
	}

	private Slider AddStepSlider(Transform row)
	{
		Slider slider = AddSlider(row);
		LayoutElement element = ((Component)slider).GetComponent<LayoutElement>();
		element.ignoreLayout = false;
		element.flexibleWidth = 1f;
		element.minWidth = 160f;
		element.preferredWidth = 220f;
		element.preferredHeight = 18f;
		element.minHeight = 18f;
		Sprite sprite = GUIManager.Instance.GetSprite("text_field");
		Image background = ((Component)slider).GetComponent<Image>();
		if ((Object)(object)background == (Object)null)
		{
			background = ((Component)slider).gameObject.AddComponent<Image>();
		}
		background.sprite = sprite;
		background.type = Image.Type.Sliced;
		((Graphic)background).color = new Color(0.2f, 0.18f, 0.16f, 1f);
		((Graphic)background).raycastTarget = true;
		if ((Object)(object)slider.fillRect != (Object)null)
		{
			Image fill = ((Component)slider.fillRect).GetComponent<Image>();
			if ((Object)(object)fill != (Object)null)
			{
				((Graphic)fill).color = new Color(1f, 1f, 1f, 0f);
				((Graphic)fill).raycastTarget = false;
			}
		}
		if ((Object)(object)slider.handleRect != (Object)null)
		{
			Image handle = ((Component)slider.handleRect).GetComponent<Image>();
			if ((Object)(object)handle != (Object)null)
			{
				Sprite mark = GUIManager.Instance.GetSprite("UISprite");
				handle.sprite = ((Object)(object)mark != (Object)null) ? mark : sprite;
				handle.type = Image.Type.Sliced;
				((Graphic)handle).color = SelectedRow;
				((Graphic)handle).raycastTarget = true;
			}
			FitMark(slider);
		}
		return slider;
	}

	private Slider AddSlider(Transform parent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = DefaultControls.CreateSlider(GUIManager.Instance.ValheimControlResources);
		val.transform.SetParent(parent, false);
		LayoutElement val2 = val.AddComponent<LayoutElement>();
		val2.flexibleWidth = 1f;
		val2.preferredWidth = 180f;
		val2.minWidth = 120f;
		val2.preferredHeight = 24f;
		val2.minHeight = 24f;
		Slider component = val.GetComponent<Slider>();
		component.wholeNumbers = true;
		GUIManager.Instance.ApplySliderStyle(component);
		return component;
	}

	private InputField AddField(Transform parent, string placeholder, float width)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GUIManager.Instance.CreateInputField(parent, Vector2.zero, Vector2.one, Vector2.zero, (ContentType)0, placeholder, 16, width, 32f);
		LayoutElement val2 = val.GetComponent<LayoutElement>();
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = val.AddComponent<LayoutElement>();
		}
		val2.preferredHeight = 32f;
		val2.minHeight = 32f;
		val2.flexibleWidth = ((width > 0f) ? 0f : 1f);
		if (width > 0f)
		{
			val2.preferredWidth = width;
			val2.minWidth = width;
		}
		return val.GetComponent<InputField>();
	}

	private Text AddText(string caption, Transform parent, int size, float width, float height)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GUIManager.Instance.CreateText(caption, parent, Center, Center, Vector2.zero, GUIManager.Instance.AveriaSerifBold, size, Color.white, true, Color.black, width, height, false);
		return val.GetComponent<Text>();
	}

	private static RectTransform CreateStretch(string name, Transform parent, float left, float bottom, float right, float top)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name, new Type[1] { typeof(RectTransform) });
		val.transform.SetParent(parent, false);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = new Vector2(left, bottom);
		component.offsetMax = new Vector2(0f - right, 0f - top);
		return component;
	}

	private static void AnchorTopCenter(RectTransform rect, float x, float yFromTop, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		rect.anchorMin = new Vector2(0.5f, 1f);
		rect.anchorMax = new Vector2(0.5f, 1f);
		rect.pivot = new Vector2(0.5f, 1f);
		rect.anchoredPosition = new Vector2(x, 0f - yFromTop);
		rect.sizeDelta = new Vector2(width, height);
	}

	private static void AnchorTopRight(RectTransform rect, float xFromRight, float yFromTop, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		rect.anchorMin = new Vector2(1f, 1f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(1f, 1f);
		rect.anchoredPosition = new Vector2(0f - xFromRight, 0f - yFromTop);
		rect.sizeDelta = new Vector2(width, height);
	}

	private static void AnchorBottom(RectTransform rect, float yFromBottom, float height, float rightInset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 0f);
		rect.pivot = new Vector2(0.5f, 0f);
		rect.anchoredPosition = new Vector2((32f - rightInset) / 2f, yFromBottom);
		rect.sizeDelta = new Vector2(0f - (32f + rightInset), height);
	}

	private static void AnchorBottomRight(RectTransform rect, float xFromRight, float yFromBottom, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		rect.anchorMin = new Vector2(1f, 0f);
		rect.anchorMax = new Vector2(1f, 0f);
		rect.pivot = new Vector2(1f, 0f);
		rect.anchoredPosition = new Vector2(0f - xFromRight, yFromBottom);
		rect.sizeDelta = new Vector2(width, height);
	}

	private static void StretchText(GameObject buttonObject)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Text componentInChildren = buttonObject.GetComponentInChildren<Text>();
		if (!((Object)(object)componentInChildren == (Object)null))
		{
			RectTransform rectTransform = ((Graphic)componentInChildren).rectTransform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = new Vector2(6f, 2f);
			rectTransform.offsetMax = new Vector2(-6f, -2f);
		}
	}

	private void PaintConsole()
	{
		if ((Object)(object)_console == (Object)null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string logLine in _logLines)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append("> ").Append(logLine);
		}
		_console.text = stringBuilder.ToString();
	}

	private static void Highlight(Dictionary<string, Button> rows, string? selected)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<string, Button> row in rows)
		{
			ColorBlock colors = ((Selectable)row.Value).colors;
			Color selectedColor = (colors.pressedColor = (colors.highlightedColor = (colors.normalColor = ((row.Key == selected) ? SelectedRow : Color.white))));
			colors.selectedColor = selectedColor;
			((Selectable)row.Value).colors = colors;
		}
	}

	private static void Clear(RectTransform parent)
	{
		for (int num = ((Transform)parent).childCount - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)((Transform)parent).GetChild(num)).gameObject);
		}
	}

	private void ShowFailure<T>(ActionResult<T> result)
	{
		if (!result.Ok && result.Error != null)
		{
			SetStatus(result.Error);
		}
	}
}

using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ValheimQoLCM.Core;

namespace ValheimQoLCM;

/// <summary>Valheim wood panel. Clicks call the gameplay services.</summary>
public sealed class ConsoleView
{
    private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    private readonly ConfigEntry<float> _skillLoss;
    private readonly Dictionary<PlayerMode, Text> _modeLabels = new Dictionary<PlayerMode, Text>();
    private GameObject _root;
    private Text _status;
    private RectTransform _players;
    private RectTransform _items;
    private InputField _filter;
    private InputField _steamId;
    private GameObject _steamRow;
    private Text _quantityLabel;
    private Text _qualityLabel;
    private Text _percentLabel;
    private Slider _slider;
    private string _selected;
    private string _item;
    private string _listedPlayers = string.Empty;
    private int _quantity = SpawnValidation.MinQuantity;
    private int _quality = 1;
    private bool _confirmGrant;

    /// <summary>Creates the view. The widgets are built when the game GUI exists.</summary>
    public ConsoleView(ConfigEntry<float> skillLoss)
    {
        _skillLoss = skillLoss;
    }

    /// <summary>Creates the wood panel under the Jötunn front GUI.</summary>
    public void Build(Transform parent)
    {
        if (_root != null)
        {
            Object.Destroy(_root);
            _modeLabels.Clear();
        }

        _root = GUIManager.Instance.CreateWoodpanel(parent, Center, Center, Vector2.zero, 980f, 660f, false);
        AddText("Valheim QoL", new Vector2(0f, 292f), 28, 420f, 36f);
        _status = AddText(string.Empty, new Vector2(0f, 256f), 16, 860f, 28f);
        AddText("Connected players", new Vector2(-250f, 214f), 18, 260f, 24f);
        _players = AddColumn(new Vector2(-250f, -20f), new Vector2(280f, 420f));
        AddModeButton(PlayerMode.God, "God", new Vector2(40f, 180f));
        AddModeButton(PlayerMode.Fly, "Fly", new Vector2(190f, 180f));
        AddModeButton(PlayerMode.Creative, "Creative", new Vector2(340f, 180f));
        AddModeButton(PlayerMode.FreeCam, "Free cam", new Vector2(40f, 130f));
        AddAction("Bring me to player", new Vector2(80f, 70f), 220f, () => ShowFailure(AdminCommands.RequestBringMe(_selected)));
        AddAction("Bring player to me", new Vector2(310f, 70f), 220f, () => ShowFailure(AdminCommands.RequestBringTarget(_selected)));
        _filter = AddField("Item filter", new Vector2(190f, 10f), 300f);
        _filter.onValueChanged.AddListener(_ => RefreshItems());
        _items = AddColumn(new Vector2(190f, -70f), new Vector2(300f, 110f));
        _quantityLabel = AddText(QuantityText(), new Vector2(40f, -150f), 16, 120f, 24f);
        AddAction("-", new Vector2(130f, -150f), 36f, () => StepQuantity(-1));
        AddAction("+", new Vector2(172f, -150f), 36f, () => StepQuantity(1));
        _qualityLabel = AddText(QualityText(), new Vector2(250f, -150f), 16, 120f, 24f);
        AddAction("-", new Vector2(340f, -150f), 36f, () => StepQuality(-1));
        AddAction("+", new Vector2(382f, -150f), 36f, () => StepQuality(1));
        AddAction("Spawn", new Vector2(190f, -196f), 160f, Spawn);
        _slider = AddSlider(new Vector2(160f, -250f));
        _percentLabel = AddText(PercentText(_slider.value), new Vector2(360f, -250f), 16, 140f, 24f);
        _slider.onValueChanged.AddListener(value => _percentLabel.text = PercentText(value));
        AddAction("Apply skill loss", new Vector2(120f, -296f), 200f, () => ShowFailure(DeathPenaltyManager.RequestPercent(_slider.value)));
        var steamField = AddField("Steam ID", new Vector2(120f, -346f), 220f);
        _steamId = steamField;
        _steamRow = steamField.gameObject;
        _steamRow.SetActive(false);
        AddAction("Grant admin", new Vector2(340f, -346f), 160f, Grant);
        AddText(Plugin.Version, new Vector2(0f, -308f), 14, 80f, 22f);
        _root.SetActive(false);
        RefreshItems();
    }

    /// <summary>True after the wood panel exists.</summary>
    public bool IsBuilt => _root != null;

    /// <summary>Draws the panel above other custom GUI.</summary>
    public void BringToFront()
    {
        if (_root != null)
        {
            _root.transform.SetAsLastSibling();
        }
    }

    /// <summary>Shows or hides the panel. Hiding does not undo gameplay changes.</summary>
    public void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }

        if (visible)
        {
            _listedPlayers = string.Empty;
            _confirmGrant = false;
        }
    }

    /// <summary>Replaces the status line.</summary>
    public void SetStatus(string message)
    {
        if (_status != null)
        {
            _status.text = message ?? string.Empty;
        }
    }

    /// <summary>Reloads the connected-player buttons when the names change.</summary>
    public void RefreshPlayers()
    {
        if (_players == null)
        {
            return;
        }

        var rows = AdminCommands.ListConnected();
        var key = new StringBuilder();
        foreach (var row in rows)
        {
            key.Append(row.Name).Append('|').Append(row.SteamId).Append(';');
        }

        var next = key.ToString();
        if (next == _listedPlayers)
        {
            return;
        }

        _listedPlayers = next;
        Clear(_players);
        var found = false;
        var index = 0;
        foreach (var row in rows)
        {
            var captured = row;
            if (captured.Name == _selected)
            {
                found = true;
            }

            var label = captured.IsSelf ? captured.Name + " (you)" : captured.Name;
            var button = AddChildButton(_players, label, new Vector2(0f, 180f - (index * 36f)), 250f);
            button.onClick.AddListener(() => Select(captured));
            index++;
        }

        if (!found)
        {
            _selected = null;
            _confirmGrant = false;
            if (_steamRow != null)
            {
                _steamRow.SetActive(false);
            }
        }
    }

    /// <summary>Updates mode buttons from the local character.</summary>
    public void RefreshModes()
    {
        foreach (var pair in _modeLabels)
        {
            pair.Value.text = pair.Key + ": " + (GameplayModifiers.IsEnabled(pair.Key) ? "On" : "Off");
        }
    }

    /// <summary>Moves the slider to the synced percent.</summary>
    public void RefreshPercent()
    {
        if (_slider == null || _percentLabel == null)
        {
            return;
        }

        var percent = _skillLoss != null ? SkillLoss.ClampPercent(_skillLoss.Value) : Plugin.SkillLossPercent;
        _slider.SetValueWithoutNotify(percent);
        _percentLabel.text = PercentText(percent);
    }

    private void Select(ConnectedPlayer player)
    {
        _selected = player.Name;
        _confirmGrant = false;
        var needsTypedId = string.IsNullOrEmpty(player.SteamId);
        if (_steamRow != null)
        {
            _steamRow.SetActive(needsTypedId);
        }

        SetStatus(needsTypedId
            ? "No Steam ID on this connection. Type one to grant admin."
            : "Selected " + player.Name + ".");
    }

    private void RefreshItems()
    {
        if (_items == null)
        {
            return;
        }

        Clear(_items);
        var filter = _filter != null ? _filter.text : string.Empty;
        var index = 0;
        foreach (var name in ItemSpawner.FindItems(filter))
        {
            var captured = name;
            var button = AddChildButton(_items, captured, new Vector2(0f, 36f - (index * 28f)), 270f);
            button.onClick.AddListener(() =>
            {
                _item = captured;
                _quality = 1;
                if (_qualityLabel != null)
                {
                    _qualityLabel.text = QualityText();
                }

                SetStatus("Item " + captured + ".");
            });
            index++;
        }
    }

    private void StepQuantity(int delta)
    {
        var next = _quantity + delta;
        if (next < SpawnValidation.MinQuantity || next > SpawnValidation.MaxQuantity)
        {
            SetStatus("Invalid quantity.");
            return;
        }

        _quantity = next;
        _quantityLabel.text = QuantityText();
    }

    private void StepQuality(int delta)
    {
        var max = string.IsNullOrEmpty(_item) ? 1 : ItemSpawner.MaxQuality(_item);
        if (max < 1)
        {
            max = 1;
        }

        var next = _quality + delta;
        if (next < 1 || next > max)
        {
            SetStatus("Invalid quality.");
            return;
        }

        _quality = next;
        _qualityLabel.text = QualityText();
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
            return;
        }

        _confirmGrant = false;
        var typed = _steamId != null ? _steamId.text : null;
        ShowFailure(AdminCommands.RequestGrant(_selected, typed));
    }

    private void AddModeButton(PlayerMode mode, string caption, Vector2 position)
    {
        var buttonObject = GUIManager.Instance.CreateButton(caption, _root.transform, Center, Center, position, 140f, 36f);
        var label = buttonObject.GetComponentInChildren<Text>();
        _modeLabels[mode] = label;
        label.text = caption + ": Off";
        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            var result = GameplayModifiers.Toggle(mode);
            SetStatus(result.Ok ? caption + " is " + (result.Data ? "on" : "off") + "." : result.Error);
            RefreshModes();
        });
    }

    private void AddAction(string caption, Vector2 position, float width, UnityAction action)
    {
        var buttonObject = GUIManager.Instance.CreateButton(caption, _root.transform, Center, Center, position, width, 36f);
        buttonObject.GetComponent<Button>().onClick.AddListener(action);
    }

    private Button AddChildButton(RectTransform parent, string caption, Vector2 position, float width)
    {
        var buttonObject = GUIManager.Instance.CreateButton(caption, parent, Center, Center, position, width, 32f);
        return buttonObject.GetComponent<Button>();
    }

    private Text AddText(string caption, Vector2 position, int size, float width, float height)
    {
        var textObject = GUIManager.Instance.CreateText(
            caption,
            _root.transform,
            Center,
            Center,
            position,
            GUIManager.Instance.AveriaSerifBold,
            size,
            Color.white,
            true,
            Color.black,
            width,
            height,
            false);
        return textObject.GetComponent<Text>();
    }

    private InputField AddField(string placeholder, Vector2 position, float width)
    {
        var fieldObject = GUIManager.Instance.CreateInputField(
            _root.transform,
            Center,
            Center,
            position,
            InputField.ContentType.Standard,
            placeholder,
            16,
            width,
            32f);
        return fieldObject.GetComponent<InputField>();
    }

    private Slider AddSlider(Vector2 position)
    {
        var sliderObject = DefaultControls.CreateSlider(GUIManager.Instance.ValheimControlResources);
        sliderObject.transform.SetParent(_root.transform, false);
        var rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = Center;
        rect.anchorMax = Center;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240f, 24f);
        var slider = sliderObject.GetComponent<Slider>();
        slider.minValue = SkillLoss.MinPercent;
        slider.maxValue = SkillLoss.MaxPercent;
        slider.wholeNumbers = true;
        GUIManager.Instance.ApplySliderStyle(slider);
        return slider;
    }

    private RectTransform AddColumn(Vector2 position, Vector2 size)
    {
        var column = new GameObject("Column", typeof(RectTransform));
        column.transform.SetParent(_root.transform, false);
        var rect = column.GetComponent<RectTransform>();
        rect.anchorMin = Center;
        rect.anchorMax = Center;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static void Clear(RectTransform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void ShowFailure<T>(ActionResult<T> result)
    {
        if (!result.Ok)
        {
            SetStatus(result.Error);
        }
    }

    private string QuantityText()
    {
        return "Qty " + _quantity;
    }

    private string QualityText()
    {
        return "Quality " + _quality;
    }

    private static string PercentText(float value)
    {
        return value.ToString("0") + "%";
    }
}

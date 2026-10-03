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
    private static readonly Color SelectedRow = new Color(0.95f, 0.55f, 0.15f, 1f);
    private static readonly Color DisabledTint = new Color(0.566f, 0.566f, 0.566f, 0.5f);

    private const float PanelWidth = 1080f;
    private const float PanelHeight = 880f;
    private const float ModuleInsetLeft = 22f;
    private const float ModuleInsetRight = 22f;
    private const float ModuleInsetTop = 46f;
    private const float ModuleInsetBottom = 108f;
    private const float ModuleGap = 10f;
    private const int MaxLogLines = 8;
    private const string CreditsUrl = "https://github.com/LuisbatistaOT/Valheim-QoL-CM";

    private readonly ConfigEntry<float> _skillLoss;
    private readonly Dictionary<PlayerMode, Text> _modeLabels = new Dictionary<PlayerMode, Text>();
    private readonly Dictionary<string, Button> _playerButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> _itemButtons = new Dictionary<string, Button>();
    private readonly List<string> _logLines = new List<string>();
    private readonly List<Selectable> _playerActions = new List<Selectable>();
    private readonly List<Selectable> _spawnControls = new List<Selectable>();

    private GameObject _root = null!;
    private Text _console = null!;
    private RectTransform _players = null!;
    private RectTransform _items = null!;
    private InputField _filter = null!;
    private InputField _steamId = null!;
    private InputField _quantityField = null!;
    private InputField _qualityField = null!;
    private GameObject _steamRow = null!;
    private Text _percentLabel = null!;
    private Slider _slider = null!;
    private string? _selected;
    private bool _selectedIsSelf;
    private string? _item;
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
            _playerButtons.Clear();
            _itemButtons.Clear();
            _playerActions.Clear();
            _spawnControls.Clear();
        }

        _root = GUIManager.Instance.CreateWoodpanel(parent, Center, Center, Vector2.zero, PanelWidth, PanelHeight, false);
        var title = AddText("Valheim QoL - CM", _root.transform, 22, 460f, 32f);
        title.alignment = TextAnchor.MiddleCenter;
        AnchorTopCenter(title.rectTransform, 0f, 12f, 460f, 32f);
        var version = AddText("Version " + Plugin.Version, _root.transform, 16, 150f, 24f);
        version.alignment = TextAnchor.MiddleRight;
        AnchorTopRight(version.rectTransform, 36f, 16f, 150f, 24f);

        var modules = CreateStretch("Modules", _root.transform, ModuleInsetLeft, ModuleInsetBottom, ModuleInsetRight, ModuleInsetTop);
        var grid = modules.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.spacing = new Vector2(ModuleGap, ModuleGap);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        var gridWidth = PanelWidth - ModuleInsetLeft - ModuleInsetRight;
        var gridHeight = PanelHeight - ModuleInsetTop - ModuleInsetBottom;
        grid.cellSize = new Vector2((gridWidth - ModuleGap) / 2f, (gridHeight - ModuleGap) / 2f);

        BuildPlayers(AddModule(modules, "Player Management"));
        BuildCheats(AddModule(modules, "Global Cheats"));
        BuildItems(AddModule(modules, "Item Spawner"));
        BuildSkills(AddModule(modules, "Server skill loss"));

        _console = AddText(string.Empty, _root.transform, 15, 900f, 84f);
        _console.alignment = TextAnchor.LowerLeft;
        AnchorBottom(_console.rectTransform, 22f, 84f, 180f);
        var credit = GUIManager.Instance.CreateButton("By Alfamud", _root.transform, Center, Center, Vector2.zero, 140f, 28f);
        StretchText(credit);
        AnchorBottomRight(credit.GetComponent<RectTransform>(), 28f, 8f, 140f, 28f);
        credit.GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(CreditsUrl));
        PaintConsole();
        ApplyPlayerActions();
        ApplySpawnEnabled();
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

    /// <summary>Appends one line to the bottom console. Closing the panel does not clear it.</summary>
    public void SetStatus(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        _logLines.Add(message);
        while (_logLines.Count > MaxLogLines)
        {
            _logLines.RemoveAt(0);
        }

        PaintConsole();
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
        _playerButtons.Clear();
        var found = false;
        foreach (var row in rows)
        {
            var captured = row;
            if (captured.Name == _selected)
            {
                found = true;
            }

            var label = captured.IsSelf ? captured.Name + " (you)" : captured.Name;
            var button = AddRowButton(_players, label);
            button.onClick.AddListener(() => Select(captured));
            _playerButtons[captured.Name] = button;
        }

        if (!found)
        {
            _confirmGrant = false;
            if (_steamRow != null)
            {
                _steamRow.SetActive(false);
            }

            ConnectedPlayer? self = null;
            foreach (var row in rows)
            {
                if (row.IsSelf)
                {
                    self = row;
                    break;
                }
            }

            if (self != null)
            {
                _selected = self.Name;
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

    private void BuildPlayers(RectTransform body)
    {
        _players = AddScrollList(body, true, false);
        _playerActions.Add(AddFlexButton(body, "Bring me to player", () => ShowFailure(AdminCommands.RequestBringMe(_selected)), 0f));
        _playerActions.Add(AddFlexButton(body, "Bring player to me", () => ShowFailure(AdminCommands.RequestBringTarget(_selected)), 0f));
        _playerActions.Add(AddFlexButton(body, "Grant admin", Grant, 0f));
        _steamId = AddField(body, "Steam ID", 0f);
        _steamRow = _steamId.gameObject;
        _steamRow.SetActive(false);
    }

    private void BuildCheats(RectTransform body)
    {
        var gridObject = new GameObject("CheatGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridObject.transform.SetParent(body, false);
        var grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.cellSize = new Vector2(200f, 34f);
        grid.spacing = new Vector2(8f, 8f);
        grid.childAlignment = TextAnchor.UpperCenter;
        var element = gridObject.GetComponent<LayoutElement>();
        element.preferredHeight = 118f;
        element.minHeight = 118f;
        element.flexibleWidth = 1f;
        AddModeButton(PlayerMode.God, "God", gridObject.transform);
        AddModeButton(PlayerMode.Fly, "Fly", gridObject.transform);
        AddModeButton(PlayerMode.Creative, "Creative", gridObject.transform);
        AddModeButton(PlayerMode.FreeCam, "Free cam", gridObject.transform);
        AddModeButton(PlayerMode.Ghost, "Ghost", gridObject.transform);
        var actions = AddRow(body, TextAnchor.MiddleCenter);
        AddFlexButton(actions, "Tame", () => ShowFailure(NearbyCommands.RequestTame()), 140f);
        AddFlexButton(actions, "Kill enemies", () => ShowFailure(NearbyCommands.RequestKillEnemies()), 180f);
    }

    private void BuildItems(RectTransform body)
    {
        var block = new GameObject("ItemBlock", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        block.transform.SetParent(body, false);
        var blockLayout = block.GetComponent<VerticalLayoutGroup>();
        blockLayout.spacing = 0f;
        blockLayout.childAlignment = TextAnchor.UpperCenter;
        blockLayout.childControlWidth = true;
        blockLayout.childControlHeight = true;
        blockLayout.childForceExpandWidth = true;
        blockLayout.childForceExpandHeight = false;
        var blockElement = block.GetComponent<LayoutElement>();
        blockElement.flexibleWidth = 1f;
        blockElement.flexibleHeight = 1f;
        blockElement.minHeight = 120f;

        _filter = AddField(block.transform, "Item filter", 0f);
        var filterImage = _filter.GetComponent<Image>();
        if (filterImage != null)
        {
            filterImage.color = Color.black;
        }

        _filter.onValueChanged.AddListener(_ => RefreshItems());
        _items = AddScrollList(block.transform, true, true);

        var quantityRow = AddRow(body, TextAnchor.MiddleLeft);
        AddFixedLabel(quantityRow, "Qty", 36f);
        _spawnControls.Add(AddFlexButton(quantityRow, "-", () => StepQuantity(-1), 32f));
        _quantityField = AddNumberBox(quantityRow);
        _spawnControls.Add(_quantityField);
        _spawnControls.Add(AddFlexButton(quantityRow, "+", () => StepQuantity(1), 32f));
        _spawnControls.Add(AddFlexButton(quantityRow, "x10", () => MultiplyQuantity(10), 52f));
        _spawnControls.Add(AddFlexButton(quantityRow, "Max Stack", SetMaxStack, 108f));

        var qualityRow = AddRow(body, TextAnchor.MiddleLeft);
        AddFixedLabel(qualityRow, "Quality", 72f);
        _spawnControls.Add(AddFlexButton(qualityRow, "-", () => StepQuality(-1), 32f));
        _qualityField = AddNumberBox(qualityRow);
        _spawnControls.Add(_qualityField);
        _spawnControls.Add(AddFlexButton(qualityRow, "+", () => StepQuality(1), 32f));
        PaintNumbers();

        var spawnRow = AddRow(body, TextAnchor.MiddleCenter);
        _spawnControls.Add(AddFlexButton(spawnRow, "Spawn", Spawn, 180f));
    }

    private void BuildSkills(RectTransform body)
    {
        // Spec 001 REQ-6 keeps this percent for every death on the server, not the selected player.
        AddLayoutText(body, "Every player, on the next death", 14, new Color(0.9f, 0.86f, 0.75f, 1f));
        var row = AddRow(body, TextAnchor.MiddleCenter);
        _slider = AddSlider(row);
        _percentLabel = AddReadout(row, PercentText(_slider.value), 64f);
        _slider.onValueChanged.AddListener(value => _percentLabel.text = PercentText(value));
        var applyRow = AddRow(body, TextAnchor.MiddleCenter);
        AddFlexButton(applyRow, "Apply skill loss", () => ShowFailure(DeathPenaltyManager.RequestPercent(_slider.value)), 200f);
    }

    private void Select(ConnectedPlayer player)
    {
        _selected = player.Name;
        _selectedIsSelf = player.IsSelf;
        _confirmGrant = false;
        var needsTypedId = !_selectedIsSelf && string.IsNullOrEmpty(player.SteamId);
        if (_steamRow != null)
        {
            _steamRow.SetActive(needsTypedId);
        }

        Highlight(_playerButtons, _selected);
        ApplyPlayerActions();
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
        _itemButtons.Clear();
        var filter = _filter != null ? _filter.text : string.Empty;
        foreach (var item in ItemSpawner.FindItems(filter))
        {
            var captured = item;
            var button = AddRowButton(_items, captured.Label);
            button.onClick.AddListener(() => SelectItem(captured));
            _itemButtons[captured.Prefab] = button;
        }

        Highlight(_itemButtons, _item);
        ApplySpawnEnabled();
    }

    private void SelectItem(SpawnableItem item)
    {
        _item = item.Prefab;
        _quality = 1;
        _quantity = SpawnValidation.MinQuantity;
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
        foreach (var control in controls)
        {
            if (control == null)
            {
                continue;
            }

            control.interactable = enabled;
            var colors = control.colors;
            colors.disabledColor = DisabledTint;
            control.colors = colors;
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
        PaintNumbers();
    }

    private void MultiplyQuantity(int factor)
    {
        var next = _quantity * factor;
        if (next < SpawnValidation.MinQuantity)
        {
            next = SpawnValidation.MinQuantity;
        }

        if (next > SpawnValidation.MaxQuantity)
        {
            next = SpawnValidation.MaxQuantity;
        }

        if (next == _quantity)
        {
            SetStatus("Invalid quantity.");
            return;
        }

        _quantity = next;
        PaintNumbers();
    }

    private void SetMaxStack()
    {
        var stack = string.IsNullOrEmpty(_item) ? SpawnValidation.MinQuantity : ItemSpawner.MaxStack(_item);
        if (stack < SpawnValidation.MinQuantity)
        {
            stack = SpawnValidation.MinQuantity;
        }

        if (stack > SpawnValidation.MaxQuantity)
        {
            stack = SpawnValidation.MaxQuantity;
        }

        _quantity = stack;
        PaintNumbers();
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
        PaintNumbers();
    }

    private void PaintNumbers()
    {
        if (_quantityField != null)
        {
            _quantityField.text = _quantity.ToString();
        }

        if (_qualityField != null)
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
            return;
        }

        _confirmGrant = false;
        var typed = _steamId != null ? _steamId.text : null;
        ShowFailure(AdminCommands.RequestGrant(_selected ?? string.Empty, typed));
    }

    private void AddModeButton(PlayerMode mode, string caption, Transform parent)
    {
        var buttonObject = GUIManager.Instance.CreateButton(caption, parent, Vector2.zero, Vector2.one, Vector2.zero, 200f, 34f);
        StretchText(buttonObject);
        var label = buttonObject.GetComponentInChildren<Text>();
        _modeLabels[mode] = label;
        label.text = caption + ": Off";
        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            var result = GameplayModifiers.Toggle(mode);
            SetStatus(result.Ok ? caption + " is " + (result.Data ? "on" : "off") + "." : result.Error ?? string.Empty);
            RefreshModes();
        });
    }

    private RectTransform AddModule(RectTransform parent, string title)
    {
        var panel = GUIManager.Instance.CreateWoodpanel(parent, Vector2.zero, Vector2.one, Vector2.zero, 100f, 100f, false);
        panel.name = title;
        var body = CreateStretch("Body", panel.transform, 18f, 16f, 18f, 16f);
        var layout = body.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.padding = new RectOffset(2, 2, 2, 2);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        AddLayoutText(body, title, 18, Color.white);
        return body;
    }

    private RectTransform AddScrollList(Transform parent, bool alwaysShowBar, bool fasterItemScroll)
    {
        var list = new GameObject("List", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement));
        list.transform.SetParent(parent, false);
        var listElement = list.GetComponent<LayoutElement>();
        listElement.flexibleWidth = 1f;
        listElement.flexibleHeight = 1f;
        listElement.minHeight = 72f;
        listElement.preferredHeight = 112f;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewport.transform.SetParent(list.transform, false);
        var viewRect = viewport.GetComponent<RectTransform>();
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = new Vector2(4f, 4f);
        viewRect.offsetMax = new Vector2(-16f, -4f);
        var viewImage = viewport.GetComponent<Image>();
        viewImage.color = new Color(0f, 0f, 0f, 0.01f);
        viewImage.raycastTarget = true;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);
        var rows = content.GetComponent<VerticalLayoutGroup>();
        rows.spacing = 2f;
        rows.padding = new RectOffset(2, 2, 2, 2);
        rows.childAlignment = TextAnchor.UpperCenter;
        rows.childControlWidth = true;
        rows.childControlHeight = true;
        rows.childForceExpandWidth = true;
        rows.childForceExpandHeight = false;
        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scrollbarObject = DefaultControls.CreateScrollbar(GUIManager.Instance.ValheimControlResources);
        scrollbarObject.transform.SetParent(list.transform, false);
        var scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        var barRect = scrollbarObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1f, 0f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(1f, 0.5f);
        barRect.offsetMin = new Vector2(-14f, 2f);
        barRect.offsetMax = new Vector2(-2f, -2f);

        var scroll = list.GetComponent<ScrollRect>();
        scroll.viewport = viewRect;
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = alwaysShowBar
            ? ScrollRect.ScrollbarVisibility.Permanent
            : ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        GUIManager.Instance.ApplyScrollRectStyle(scroll);
        if (fasterItemScroll)
        {
            scroll.scrollSensitivity *= 1.1f;
        }

        return contentRect;
    }

    private Button AddRowButton(RectTransform parent, string caption)
    {
        var buttonObject = GUIManager.Instance.CreateButton(caption, parent, Vector2.zero, Vector2.one, Vector2.zero, 0f, 30f);
        StretchText(buttonObject);
        var element = buttonObject.AddComponent<LayoutElement>();
        element.preferredHeight = 30f;
        element.minHeight = 30f;
        element.flexibleWidth = 1f;
        var button = buttonObject.GetComponent<Button>();
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        return button;
    }

    private Button AddFlexButton(Transform parent, string caption, UnityAction action, float preferredWidth)
    {
        var width = preferredWidth > 0f ? preferredWidth : 120f;
        var buttonObject = GUIManager.Instance.CreateButton(caption, parent, Vector2.zero, Vector2.one, Vector2.zero, width, 32f);
        StretchText(buttonObject);
        var element = buttonObject.AddComponent<LayoutElement>();
        element.preferredHeight = 32f;
        element.minHeight = 32f;
        if (preferredWidth > 0f)
        {
            element.preferredWidth = preferredWidth;
            element.minWidth = preferredWidth;
            element.flexibleWidth = 0f;
        }
        else
        {
            element.flexibleWidth = 1f;
        }

        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);
        return button;
    }

    private InputField AddNumberBox(Transform parent)
    {
        var field = AddField(parent, string.Empty, 48f);
        field.readOnly = true;
        field.text = "1";
        if (field.textComponent != null)
        {
            field.textComponent.alignment = TextAnchor.MiddleCenter;
        }

        return field;
    }

    private Text AddReadout(Transform parent, string caption, float width)
    {
        var box = new GameObject("Readout", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        box.transform.SetParent(parent, false);
        var image = box.GetComponent<Image>();
        image.sprite = GUIManager.Instance.GetSprite("text_field");
        image.type = Image.Type.Sliced;
        image.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);
        var element = box.GetComponent<LayoutElement>();
        element.preferredWidth = width;
        element.minWidth = width;
        element.preferredHeight = 32f;
        element.flexibleWidth = 0f;
        var text = AddText(caption, box.transform, 16, width, 32f);
        text.alignment = TextAnchor.MiddleCenter;
        var rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }

    private Text AddFixedLabel(Transform parent, string caption, float width)
    {
        var text = AddLayoutText(parent, caption, 16, Color.white);
        var element = text.gameObject.GetComponent<LayoutElement>();
        element.preferredWidth = width;
        element.minWidth = width;
        element.flexibleWidth = 0f;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private Text AddLayoutText(Transform parent, string caption, int size, Color color)
    {
        var text = AddText(caption, parent, size, 200f, 24f);
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        var element = text.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = size + 8f;
        element.minHeight = size + 8f;
        element.flexibleWidth = 1f;
        text.color = color;
        return text;
    }

    private RectTransform AddRow(Transform parent, TextAnchor alignment)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.childAlignment = alignment;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        var element = row.GetComponent<LayoutElement>();
        element.preferredHeight = 34f;
        element.minHeight = 34f;
        element.flexibleWidth = 1f;
        return row.GetComponent<RectTransform>();
    }

    private Slider AddSlider(Transform parent)
    {
        var sliderObject = DefaultControls.CreateSlider(GUIManager.Instance.ValheimControlResources);
        sliderObject.transform.SetParent(parent, false);
        var element = sliderObject.AddComponent<LayoutElement>();
        element.flexibleWidth = 1f;
        element.preferredWidth = 180f;
        element.minWidth = 120f;
        element.preferredHeight = 24f;
        element.minHeight = 24f;
        var slider = sliderObject.GetComponent<Slider>();
        slider.minValue = SkillLoss.MinPercent;
        slider.maxValue = SkillLoss.MaxPercent;
        slider.wholeNumbers = true;
        GUIManager.Instance.ApplySliderStyle(slider);
        return slider;
    }

    private InputField AddField(Transform parent, string placeholder, float width)
    {
        var fieldObject = GUIManager.Instance.CreateInputField(
            parent,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            InputField.ContentType.Standard,
            placeholder,
            16,
            width,
            32f);
        var element = fieldObject.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = fieldObject.AddComponent<LayoutElement>();
        }

        element.preferredHeight = 32f;
        element.minHeight = 32f;
        element.flexibleWidth = width > 0f ? 0f : 1f;
        if (width > 0f)
        {
            element.preferredWidth = width;
            element.minWidth = width;
        }

        return fieldObject.GetComponent<InputField>();
    }

    private Text AddText(string caption, Transform parent, int size, float width, float height)
    {
        var textObject = GUIManager.Instance.CreateText(
            caption,
            parent,
            Center,
            Center,
            Vector2.zero,
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

    private static RectTransform CreateStretch(string name, Transform parent, float left, float bottom, float right, float top)
    {
        var node = new GameObject(name, typeof(RectTransform));
        node.transform.SetParent(parent, false);
        var rect = node.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        return rect;
    }

    private static void AnchorTopCenter(RectTransform rect, float x, float yFromTop, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, -yFromTop);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void AnchorTopRight(RectTransform rect, float xFromRight, float yFromTop, float width, float height)
    {
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-xFromRight, -yFromTop);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void AnchorBottom(RectTransform rect, float yFromBottom, float height, float rightInset)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2((32f - rightInset) / 2f, yFromBottom);
        rect.sizeDelta = new Vector2(-(32f + rightInset), height);
    }

    private static void AnchorBottomRight(RectTransform rect, float xFromRight, float yFromBottom, float width, float height)
    {
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-xFromRight, yFromBottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void StretchText(GameObject buttonObject)
    {
        var label = buttonObject.GetComponentInChildren<Text>();
        if (label == null)
        {
            return;
        }

        var rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(6f, 2f);
        rect.offsetMax = new Vector2(-6f, -2f);
    }

    private void PaintConsole()
    {
        if (_console == null)
        {
            return;
        }

        var text = new StringBuilder();
        foreach (var line in _logLines)
        {
            if (text.Length > 0)
            {
                text.Append('\n');
            }

            text.Append(line);
        }

        _console.text = text.ToString();
    }

    private static void Highlight(Dictionary<string, Button> rows, string? selected)
    {
        foreach (var pair in rows)
        {
            var colors = pair.Value.colors;
            var color = pair.Key == selected ? SelectedRow : Color.white;
            colors.normalColor = color;
            colors.highlightedColor = color;
            colors.pressedColor = color;
            colors.selectedColor = color;
            pair.Value.colors = colors;
        }
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
        if (!result.Ok && result.Error != null)
        {
            SetStatus(result.Error);
        }
    }

    private static string PercentText(float value)
    {
        return value.ToString("0") + "%";
    }
}

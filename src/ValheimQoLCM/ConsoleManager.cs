using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Managers;
using UnityEngine;

namespace ValheimQoLCM;

/// <summary>Opens and closes the admin panel from the backtick key.</summary>
public static class ConsoleManager
{
    private static readonly KeyboardShortcut OpenKey = new KeyboardShortcut(KeyCode.BackQuote);
    private static readonly KeyboardShortcut PreviousDefault = new KeyboardShortcut(KeyCode.Tab, KeyCode.LeftControl);

    private static Plugin _plugin = null!;
    private static ConfigEntry<KeyboardShortcut> _toggle = null!;
    private static ConsoleView _view = null!;
    private static bool _open;
    private static float _nextRefresh;
    private static bool _loggedWaiting;

    /// <summary>Binds the hotkey and builds the panel when the game GUI is ready.</summary>
    public static void Create(Plugin plugin, ConfigEntry<float> skillLoss)
    {
        _plugin = plugin;
        _toggle = plugin.Config.Bind(
            "Input",
            "TogglePanel",
            OpenKey,
            "Open or close the admin panel. Default is the backtick key.");
        if (IsSameShortcut(_toggle.Value, PreviousDefault))
        {
            _toggle.Value = OpenKey;
        }

        if (GUIManager.IsHeadless())
        {
            return;
        }

        InputManager.Instance.AddButton(Plugin.Guid, new ButtonConfig
        {
            Name = "ToggleQoLPanel",
            ShortcutConfig = _toggle,
            ActiveInCustomGUI = true,
            Hint = "QoL panel",
            BlockOtherInputs = false
        });

        _view = new ConsoleView(skillLoss);
        GUIManager.OnCustomGUIAvailable += Build;
        Build();
    }

    /// <summary>Writes a one-line result on the open panel.</summary>
    public static void Show(string message)
    {
        if (_view != null)
        {
            _view.SetStatus(message);
        }
        else if (_plugin != null)
        {
            Jotunn.Logger.LogInfo(message);
        }
    }

    private static void Build()
    {
        if (GUIManager.CustomGUIFront == null || _view == null)
        {
            return;
        }

        _view.Build(GUIManager.CustomGUIFront.transform);
        _view.SetVisible(_open);
        if (_open)
        {
            GUIManager.BlockInput(true);
        }
    }

    internal static void Tick()
    {
        if (_view == null)
        {
            return;
        }

        EnsureBuilt();
        if (!WasPressed())
        {
            RefreshWhileOpen();
            return;
        }

        if (Player.m_localPlayer == null)
        {
            if (!_loggedWaiting)
            {
                _loggedWaiting = true;
                Jotunn.Logger.LogInfo("QoL panel waits until a character is in the world.");
            }

            return;
        }

        if (!Plugin.LocalIsAdmin())
        {
            return;
        }

        if (!_view.IsBuilt)
        {
            Jotunn.Logger.LogWarning("QoL panel could not be created.");
            return;
        }

        SetOpen(!_open);
    }

    /// <summary>Hides or shows the panel. Gameplay changes already applied stay in place.</summary>
    private static void SetOpen(bool open)
    {
        _open = open;
        _view.SetVisible(open);
        GUIManager.BlockInput(open);
        Jotunn.Logger.LogInfo(open ? "QoL panel opened." : "QoL panel closed.");
        if (!open)
        {
            return;
        }

        _view.BringToFront();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _view.RefreshPlayers();
        _view.RefreshModes();
        _view.RefreshPercent();
    }

    private static void RefreshWhileOpen()
    {
        if (!_open || _view == null || !_view.IsBuilt)
        {
            return;
        }

        if (!Plugin.LocalIsAdmin() || Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
            return;
        }

        if (Time.unscaledTime < _nextRefresh)
        {
            return;
        }

        _nextRefresh = Time.unscaledTime + 1f;
        _view.RefreshPlayers();
        _view.RefreshModes();
    }

    private static void EnsureBuilt()
    {
        if (_view == null || _view.IsBuilt || GUIManager.CustomGUIFront == null)
        {
            return;
        }

        _view.Build(GUIManager.CustomGUIFront.transform);
        _view.SetVisible(_open);
    }

    private static bool WasPressed()
    {
        return Input.GetKeyDown(KeyCode.BackQuote)
            || (_toggle != null && _toggle.Value.IsDown())
            || ZInput.GetButtonDown("ToggleQoLPanel");
    }

    private static bool IsSameShortcut(KeyboardShortcut left, KeyboardShortcut right)
    {
        if (left.MainKey != right.MainKey)
        {
            return false;
        }

        var leftMods = left.Modifiers;
        var rightMods = right.Modifiers;
        var leftCount = 0;
        var rightCount = 0;
        foreach (var unused in leftMods)
        {
            leftCount++;
        }

        foreach (var unused in rightMods)
        {
            rightCount++;
        }

        if (leftCount != rightCount)
        {
            return false;
        }

        foreach (var modifier in leftMods)
        {
            var found = false;
            foreach (var other in rightMods)
            {
                if (other == modifier)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
        }

        return true;
    }
}

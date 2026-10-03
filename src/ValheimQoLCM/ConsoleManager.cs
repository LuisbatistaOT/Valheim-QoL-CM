using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Managers;
using UnityEngine;

namespace ValheimQoLCM;

/// <summary>Opens and closes the admin panel from the rebindable hotkey.</summary>
public static class ConsoleManager
{
    private static Plugin _plugin;
    private static ConfigEntry<KeyboardShortcut> _toggle;
    private static ConsoleView _view;
    private static bool _open;
    private static float _nextRefresh;

    /// <summary>Binds the hotkey and builds the panel when the game GUI is ready.</summary>
    public static void Create(Plugin plugin, ConfigEntry<float> skillLoss)
    {
        _plugin = plugin;
        _toggle = plugin.Config.Bind(
            "Input",
            "TogglePanel",
            new KeyboardShortcut(KeyCode.Tab, KeyCode.LeftControl),
            "Open or close the admin panel. Default is Left Ctrl+Tab.");

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
            BlockOtherInputs = true
        });

        _view = new ConsoleView(skillLoss);
        GUIManager.OnCustomGUIAvailable += Build;
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
        if (_view == null || Player.m_localPlayer == null)
        {
            return;
        }

        var pressed = ZInput.GetButtonDown("ToggleQoLPanel") || _toggle.Value.IsDown();
        if (pressed)
        {
            if (Plugin.LocalIsAdmin())
            {
                SetOpen(!_open);
            }
        }

        if (!_open)
        {
            return;
        }

        if (!Plugin.LocalIsAdmin() || Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
            return;
        }

        if (Time.unscaledTime >= _nextRefresh)
        {
            _nextRefresh = Time.unscaledTime + 1f;
            _view.RefreshPlayers();
            _view.RefreshModes();
        }
    }

    private static void SetOpen(bool open)
    {
        _open = open;
        if (_view != null)
        {
            _view.SetVisible(open);
        }

        GUIManager.BlockInput(open);
        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _view.RefreshPlayers();
            _view.RefreshModes();
            _view.RefreshPercent();
        }
    }
}

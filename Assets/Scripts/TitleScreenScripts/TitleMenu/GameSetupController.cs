using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Single Player / Host Game panel. It remembers which of the two the player chose and hands
/// CREATE GAME to whichever <see cref="GameSetupPanel"/> the FOOTBALL / GOLF tabs have made visible,
/// so the tabs stay the single source of truth for which game is being set up.
/// </summary>
public class GameSetupController : MonoBehaviour
{
    private readonly List<GameSetupPanel> _panels = new List<GameSetupPanel>();
    private bool _isHosting;

    public bool IsHosting { get { return _isHosting; } }

    private void Awake()
    {
        GetComponentsInChildren(true, _panels);
        ApplyMode();
    }

    /// <summary>Wired to the SINGLE PLAYER entry.</summary>
    public void ChooseSinglePlayer()
    {
        _isHosting = false;
        ApplyMode();
    }

    /// <summary>Wired to the HOST GAME entry.</summary>
    public void ChooseHosting()
    {
        _isHosting = true;
        ApplyMode();
    }

    /// <summary>Wired to the CREATE GAME button.</summary>
    public void CreateGame()
    {
        GameSetupPanel panel = VisiblePanel();
        if (panel == null)
        {
            Debug.LogWarning("GameSetupController: CREATE GAME with no game tab showing.");
            return;
        }
        panel.CreateGame(_isHosting);
    }

    private GameSetupPanel VisiblePanel()
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            if (_panels[i].gameObject.activeInHierarchy)
                return _panels[i];
        }
        return null;
    }

    private void ApplyMode()
    {
        for (int i = 0; i < _panels.Count; i++)
            _panels[i].SetHosting(_isHosting);
    }
}

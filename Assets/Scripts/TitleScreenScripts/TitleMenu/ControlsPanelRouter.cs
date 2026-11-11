using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends the CONTROLS panel's SAVE and RESET TO DEFAULT to whichever tab is showing, so the tabs
/// stay the only thing that decides which game and scheme is being edited.
/// </summary>
public class ControlsPanelRouter : MonoBehaviour
{
    private readonly List<RebindingPanel> _panels = new List<RebindingPanel>();
    private TitleMenuController _menu;

    private void Awake()
    {
        GetComponentsInChildren(true, _panels);
        _menu = GetComponentInParent<TitleMenuController>();
    }

    /// <summary>Wired to SAVE. Closes the panel once the bindings are written.</summary>
    public void Save()
    {
        RebindingPanel panel = Visible();
        if (panel != null)
            panel.Save();
        if (_menu != null)
            _menu.CloseAll();
    }

    /// <summary>
    /// Wired to RESET TO DEFAULT. The panel stays open on purpose, so the player can see what every
    /// binding went back to.
    /// </summary>
    public void ResetAll()
    {
        RebindingPanel panel = Visible();
        if (panel != null)
            panel.ResetAll();
    }

    private RebindingPanel Visible()
    {
        for (int i = 0; i < _panels.Count; i++)
        {
            if (_panels[i].gameObject.activeInHierarchy)
                return _panels[i];
        }
        return null;
    }
}

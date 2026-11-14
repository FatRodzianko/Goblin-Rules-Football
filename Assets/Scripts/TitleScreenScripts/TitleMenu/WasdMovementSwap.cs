using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Moves Football's keyboard movement from the arrow keys to W/A/S/D.
///
/// Each movement direction is rebound the same way a player rebinding it by hand would, so whatever
/// was on its new letter takes the arrow it gave up: with the shipped layout that puts Dive on up,
/// Attack on left, Slide on down and Block on right. Going through the same swap keeps the page's
/// one rule - no two live bindings share a key - even for a player who has already rebound some of
/// these.
///
/// The kickoff aim and the kick-after positioning steer with the movement keys in the shipped layout,
/// so they follow movement onto the letters, and back again when the Move row is reset. Neither is
/// listed on the controls page, and neither map is live alongside the others, so they cannot clash.
///
/// Every change is an ordinary binding override, so RESET TO DEFAULT undoes all of it.
/// </summary>
public static class WasdMovementSwap
{
    private const string MoveAction = "Player/Move";

    private static readonly string[] DirectionParts = { "up", "down", "left", "right" };
    private static readonly string[] DirectionKeys = { "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d" };

    // Each steering action and the movement direction whose key it takes.
    private static readonly string[] SteeringActions =
    {
        "KickOff/KickoffAngleUp",
        "KickOff/KickoffAngleDown",
        "KickAfterPositioning/KickAfterPositioning-Left",
        "KickAfterPositioning/KickAfterPositioning-Right",
    };
    private static readonly string[] SteeringParts = { "up", "down", "left", "right" };

    /// <summary>True if <paramref name="action"/> is the movement action this swap moves.</summary>
    public static bool IsMovement(InputAction action)
    {
        if (action == null || action.actionMap == null)
            return false;
        return string.Equals(action.actionMap.name + "/" + action.name, MoveAction, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies the swap to <paramref name="asset"/>. <paramref name="scope"/> is the maps that are live
    /// together, as the controls page uses them for clashes. Returns the actions that were moved to make
    /// room, or null if Football's movement could not be found. An empty list with
    /// <paramref name="changed"/> false means movement was already on W/A/S/D.
    /// </summary>
    public static List<string> Apply(InputActionAsset asset, IList<InputActionMap> scope, string controlScheme, out bool changed)
    {
        changed = false;
        if (asset == null)
            return null;
        InputAction move = asset.FindAction(MoveAction, false);
        if (move == null)
            return null;

        var displaced = new List<string>();
        for (int d = 0; d < DirectionParts.Length; d++)
        {
            int index = FindCompositePart(move, DirectionParts[d], controlScheme);
            if (index < 0)
                continue;
            string previousPath = move.bindings[index].effectivePath;
            if (string.Equals(previousPath, DirectionKeys[d], StringComparison.OrdinalIgnoreCase))
                continue;

            SetPath(move, index, DirectionKeys[d]);
            changed = true;
            string moved = BindingConflicts.SwapWithConflict(scope, move, index, previousPath);
            if (moved != null)
                displaced.Add(moved);
        }

        if (MatchSteeringToMovement(asset, controlScheme))
            changed = true;
        return displaced;
    }

    /// <summary>
    /// Puts the kickoff aim and the kick-after positioning on the keys movement uses now. Returns true
    /// if any of them changed.
    /// </summary>
    public static bool MatchSteeringToMovement(InputActionAsset asset, string controlScheme)
    {
        InputAction move = asset != null ? asset.FindAction(MoveAction, false) : null;
        if (move == null)
            return false;

        bool changed = false;
        for (int s = 0; s < SteeringActions.Length; s++)
        {
            int part = FindCompositePart(move, SteeringParts[s], controlScheme);
            InputAction steering = asset.FindAction(SteeringActions[s], false);
            if (part < 0 || steering == null)
                continue;
            string key = move.bindings[part].effectivePath;
            if (string.IsNullOrEmpty(key))
                continue;

            for (int b = 0; b < steering.bindings.Count; b++)
            {
                InputBinding binding = steering.bindings[b];
                if (binding.isComposite || binding.isPartOfComposite || !InScheme(binding, controlScheme))
                    continue;
                if (string.Equals(binding.effectivePath, key, StringComparison.OrdinalIgnoreCase))
                    continue;
                SetPath(steering, b, key);
                changed = true;
            }
        }
        return changed;
    }

    private static int FindCompositePart(InputAction action, string partName, string controlScheme)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isPartOfComposite)
                continue;
            if (!string.Equals(binding.name, partName, StringComparison.OrdinalIgnoreCase))
                continue;
            if (InScheme(binding, controlScheme))
                return i;
        }
        return -1;
    }

    // An override that only restores the shipped key is removed instead, so the saved overrides list
    // only what really differs from the default.
    private static void SetPath(InputAction action, int bindingIndex, string path)
    {
        InputActionMap map = action.actionMap;
        bool wasEnabled = map != null && map.enabled;
        if (wasEnabled)
            map.Disable();

        if (string.Equals(action.bindings[bindingIndex].path, path, StringComparison.OrdinalIgnoreCase))
            action.RemoveBindingOverride(bindingIndex);
        else
            action.ApplyBindingOverride(bindingIndex, path);

        if (wasEnabled)
            map.Enable();
    }

    private static bool InScheme(InputBinding binding, string controlScheme)
    {
        if (string.IsNullOrEmpty(controlScheme))
            return true;
        if (string.IsNullOrEmpty(binding.groups))
            return false;
        string[] groups = binding.groups.Split(InputBinding.Separator);
        for (int i = 0; i < groups.Length; i++)
        {
            if (string.Equals(groups[i], controlScheme, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

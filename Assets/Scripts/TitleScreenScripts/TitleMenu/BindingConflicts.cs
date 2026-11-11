using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Settles a rebind that lands on a control something else is already using.
///
/// The answer is always a straight swap: the displaced action takes the control the rebound action
/// just gave up. Football's gamepad scheme has no spare buttons at all - the three maps that are
/// live together already use the sticks, both triggers, both shoulders, all four face buttons and
/// the whole d-pad - so "find something free" has no answer to give. A swap always has one, always
/// terminates, and never leaves an action unreachable.
///
/// Only maps that are live at the same time count as a conflict. Football deliberately reuses
/// buttons across maps that are never enabled together - buttonSouth is Slide, SubmitPosition,
/// SubmitCoin and KickAfterSubmit - and those must be left alone.
/// </summary>
public static class BindingConflicts
{
    /// <summary>
    /// Hands <paramref name="previousPath"/> to whichever binding in <paramref name="scope"/> already
    /// held the control that was just bound. Returns the displaced action's name, or null if the
    /// control was free.
    /// </summary>
    public static string SwapWithConflict(IList<InputActionMap> scope, InputAction action, int bindingIndex, string previousPath)
    {
        if (scope == null || action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            return null;

        InputBinding changed = action.bindings[bindingIndex];
        string newPath = changed.effectivePath;
        if (string.IsNullOrEmpty(newPath))
            return null;

        for (int m = 0; m < scope.Count; m++)
        {
            InputActionMap map = scope[m];
            if (map == null)
                continue;

            for (int i = 0; i < map.bindings.Count; i++)
            {
                InputBinding candidate = map.bindings[i];
                if (candidate.isComposite)
                    continue;
                if (candidate.id == changed.id)
                    continue;
                if (!SharesControlScheme(candidate, changed))
                    continue;
                if (!IsSameControl(candidate.effectivePath, newPath))
                    continue;

                // Overrides cannot be applied to a map that is running.
                bool wasEnabled = map.enabled;
                if (wasEnabled)
                    map.Disable();
                map.ApplyBindingOverride(i, new InputBinding { overridePath = previousPath ?? string.Empty });
                if (wasEnabled)
                    map.Enable();

                return candidate.action;
            }
        }
        return null;
    }

    private static bool SharesControlScheme(InputBinding a, InputBinding b)
    {
        // A binding with no groups belongs to every scheme, so it can always clash.
        if (string.IsNullOrEmpty(a.groups) || string.IsNullOrEmpty(b.groups))
            return true;

        string[] first = a.groups.Split(InputBinding.Separator);
        string[] second = b.groups.Split(InputBinding.Separator);
        for (int i = 0; i < first.Length; i++)
        {
            for (int j = 0; j < second.Length; j++)
            {
                if (first[i].Length > 0 && string.Equals(first[i], second[j], StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    // <Gamepad>/buttonSouth and <XInputController>/buttonSouth are the same physical button written
    // two ways, so the device layouts are compared by inheritance rather than by spelling.
    private static bool IsSameControl(string first, string second)
    {
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
            return false;
        if (string.Equals(first, second, StringComparison.OrdinalIgnoreCase))
            return true;

        string layoutA = InputControlPath.TryGetDeviceLayout(first);
        string layoutB = InputControlPath.TryGetDeviceLayout(second);
        if (string.IsNullOrEmpty(layoutA) || string.IsNullOrEmpty(layoutB))
            return false;
        if (!string.Equals(ControlPart(first), ControlPart(second), StringComparison.OrdinalIgnoreCase))
            return false;
        if (string.Equals(layoutA, layoutB, StringComparison.OrdinalIgnoreCase))
            return true;
        return InputSystem.IsFirstLayoutBasedOnSecond(layoutA, layoutB)
            || InputSystem.IsFirstLayoutBasedOnSecond(layoutB, layoutA);
    }

    private static string ControlPart(string path)
    {
        int slash = path.IndexOf('/');
        if (slash >= 0 && slash + 1 < path.Length)
            return path.Substring(slash + 1);
        return path;
    }
}

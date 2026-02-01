using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableBaseAction", menuName = "BombRun/Actions/New Scriptable Base Action")]
public class ScriptableBaseAction : ScriptableObject
{
    [SerializeField] private BaseAction _baseAction;
}

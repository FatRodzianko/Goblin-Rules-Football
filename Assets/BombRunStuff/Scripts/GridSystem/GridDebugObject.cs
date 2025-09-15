using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridDebugObject : MonoBehaviour
{
    private object _gridObject;
    [SerializeField] private TextMeshPro _text;

    public virtual void SetGridObject(object gridObject)
    {
        this._gridObject = gridObject;
    }
    public void SetDebugText()
    {
        _text.text = _gridObject.ToString();
    }
    protected virtual void Update()
    {
        //SetDebugText();
    }
}

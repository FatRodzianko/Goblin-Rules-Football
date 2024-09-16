using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{

    private float _totalSpinAmount;
    private float _maxSpinAmount = 360f;


    private void Update()
    {
        if (!_isActive)
        {
            return;
        }


        float spinAddAmount = _maxSpinAmount * Time.deltaTime;

        if (_totalSpinAmount + spinAddAmount >= _maxSpinAmount)
        {
            spinAddAmount = _maxSpinAmount - _totalSpinAmount;
            _totalSpinAmount += spinAddAmount;
            _isActive = false;
        }
        else
        {
            _totalSpinAmount += spinAddAmount;
        }

        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
    }
    public void Spin()
    {
        _totalSpinAmount = 0;
        _isActive = true;
    }
}

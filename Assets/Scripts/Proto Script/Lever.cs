using System;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

public class Lever : Interactable
{
    [SerializeField] private int _channel;
    
    private bool OnOff;

    private GameObject _pivot;
    [SerializeField] private AnimationCurve _animationCurve;
    private float time;

    private void Awake()
    {
        _pivot = this.gameObject.transform.GetChild(0).gameObject;
    }

    protected override void OnClick()
    {
        if (OnOff)
        {
            OnOff = false;
            SwitchManager.SetSwitch(_channel,true);
        }
        else
        {
            OnOff = true;
            SwitchManager.SetSwitch(_channel,false);
        }
    }

    private void FixedUpdate()
    {
        var value = _animationCurve.Evaluate(time);
        _pivot.transform.rotation = UnityEngine.Quaternion.Euler(new Vector3(0,0,value));
        if (OnOff)
        {
            time +=Time.deltaTime;
            if (time >= _animationCurve.length)
            {
                time = _animationCurve.length;
            }
        }
        else
        {
            time -=Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
            }
        }
    }
}
using System;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

// TEST CLASS
public class Lever : Interactable
{
    [SerializeField] private int _channel;
    
    private bool OnOff;

    private Transform _pivot;
    private Vector3 _startRotation;
    [SerializeField] private AnimationCurve _animationCurve;
    private float time;

    private void Awake()
    {
        _startRotation = transform.GetChild(0).transform.localEulerAngles;
        _pivot = transform.GetChild(0);
    }

    public override void BeginInteraction()
    {
        OnOff ^= true;
        SwitchManager.SetSwitch(_channel, OnOff);
    }

    private void FixedUpdate()
    {
        var value = _animationCurve.Evaluate(time);
        _pivot.localEulerAngles = new Vector3(0, 0, value) + _startRotation;
        if (OnOff)
        {
            time += Time.deltaTime;
            if (time >= 1)
            {
                time = 1;
            }
        }
        else
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
            }
        }
    }
}
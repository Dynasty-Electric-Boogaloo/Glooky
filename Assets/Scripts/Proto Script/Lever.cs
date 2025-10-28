using System;
using UnityEngine;

public class Lever : Interactable
{
    private bool OnOff;

    private GameObject _pivot;
    [SerializeField] private AnimationCurve _animationCurve;

    private void Awake()
    {
        _pivot = this.gameObject.transform.GetChild(0).gameObject;
        Debug.Log(_pivot);
    }

    protected override void OnClick()
    {
        if (OnOff)
        {
            OnOff = false;
        }
        else
        {
            OnOff = true;
        }
    }

    private void FixedUpdate()
    {
        //_pivot.transform.rotation = 
    }
}
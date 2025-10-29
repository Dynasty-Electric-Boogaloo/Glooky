using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//TEST Class do not use.
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private int channelListenedTo;
    
    [SerializeField] private float movementSpeed;
    private PhysicsController _physicsController;

    private Vector3 _selectPosition;
    [SerializeField] private GameObject _platform;
    [SerializeField] private GameObject _PosA;
    [SerializeField] private GameObject _PosB;

    private void Awake()
    {
        _selectPosition = _PosA.transform.position;
        _physicsController = GetComponent<PhysicsController>();
    }

    private void FixedUpdate()
    {
        Debug.Log(SwitchManager.GetSwitch(6));
        if (SwitchManager.GetSwitch(6) == false) {return;}

        
        _platform.transform.position = Vector3.MoveTowards(_platform.transform.position, _selectPosition, movementSpeed * Time.deltaTime);
        if ((_selectPosition-_platform.transform.position).sqrMagnitude <= 0.1f)
        {
            StartCoroutine(ChangePosition());
        }

    }
    IEnumerator ChangePosition()
    {
        yield return new WaitForSeconds(3f);
        
        if ((_PosA.transform.position-_platform.transform.position).sqrMagnitude <= 0.1f)
        {
            _selectPosition = _PosB.transform.position;
        }  
        if ((_PosB.transform.position-_platform.transform.position).sqrMagnitude <= 0.1f)
        {
            _selectPosition = _PosA.transform.position;
        } 
    }
}
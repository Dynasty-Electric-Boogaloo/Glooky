using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraPriorityChange : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _followCamera;
    [SerializeField] private CinemachineCamera _fixCamera;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Cursor"))
        {
            _followCamera.Priority = 0;
            _fixCamera.Priority = 1;;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Cursor"))
        {
            _followCamera.Priority = 1;
            _fixCamera.Priority = 0;
        }
    }
}

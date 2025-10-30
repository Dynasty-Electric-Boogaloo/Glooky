using System;
using Unity.Cinemachine;
using UnityEngine;

public class CamCenterCurs : MonoBehaviour
{
    [Header("CentreCursor")]
    [SerializeField] private GameObject CursorA;
    [SerializeField] private GameObject CursorB;
    private Vector3 distance;

    [Header("Zoom")] 
    [SerializeField] private AnimationCurve _curve;

    [SerializeField] private CinemachineCamera _cinemachineCamera;
    


    private void FixedUpdate()
    {
        distance = (CursorA.transform.position - CursorB.transform.position)/2;
        transform.position = CursorA.transform.position - distance+new Vector3(1f,11.6f,-13f);


        var zoom = _curve.Evaluate(distance.sqrMagnitude);
        _cinemachineCamera.Lens.OrthographicSize = zoom;
    }
}

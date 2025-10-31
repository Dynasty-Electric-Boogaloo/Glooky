using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class CamCenterCurs : MonoBehaviour
{
    [Header("CentreCursor")]
    [SerializeField] private GameObject CursorA;
    [SerializeField] private GameObject CursorB;
    private Vector3 distance;
    [SerializeField] private Vector3 _offSet = new Vector3(1f,11.6f,-13f);

    [Header("Zoom")] 
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _lerpSpeed;

    [SerializeField] private CinemachineCamera _cinemachineCamera;
    


    private void FixedUpdate()
    {
        distance = (CursorA.transform.position - CursorB.transform.position)/2;
        transform.position = CursorA.transform.position - distance+ _offSet;


        
        var zoom = Mathf.Lerp(_cinemachineCamera.Lens.OrthographicSize,_curve.Evaluate(distance.sqrMagnitude), _lerpSpeed*Time.deltaTime);
        _cinemachineCamera.Lens.OrthographicSize = zoom;
    }
}

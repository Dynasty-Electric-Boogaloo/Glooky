using System;
using UnityEngine;

public class MovPlat : MonoBehaviour
{
    [SerializeField] private float _movingSpeed;
    [SerializeField] private GameObject Platform;

    private Vector3 _selectPosition;
    [SerializeField] private GameObject PosA;
    [SerializeField] private GameObject PosB;

    private void Awake()
    {
        _selectPosition = PosA.transform.position;
    }

    private void FixedUpdate()
    {
        Platform.transform.position = Vector3.MoveTowards(Platform.transform.position,_selectPosition,_movingSpeed*Time.deltaTime);
        
        if ((Platform.transform.position-PosA.transform.position).sqrMagnitude <= 0.1)
        {
            _selectPosition = PosB.transform.position;
        }
        if ((Platform.transform.position-PosB.transform.position).sqrMagnitude <= 0.1)
        {
            _selectPosition = PosA.transform.position;
        }
    }
}

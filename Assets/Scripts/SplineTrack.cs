using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class SplineTrack : MonoBehaviour
{
    [Serializable]
    private struct SplineStopPoint
    {
        public int splinePoint; //Key space
        public float distance;
    }

    [Serializable]
    private struct SplineSection
    {
        public float distance; //Final curve space
        public float value; //Raw curve space
        public bool flatten;
    }
    
    [SerializeField] private SplineContainer spline;
    [SerializeField] private Rigidbody block;
    [SerializeField] private SplineStopPoint[] stopPoints;
    //TODO Unserialize this field, it's only for viewing for tests
    [SerializeField] private AnimationCurve positionEvaluationCurve;
    
    private float _curveLength;
    private float _trackLength;

    private void Start()
    {
        ComputePositionEvaluationCurve();
    }

    public Vector3 GetPosition(float time)
    {
        if (!spline.Spline.Closed)
        {
            time *= 2;
            if (time > 1)
                time = 2 - time;
        }
        
        Vector3 splinePos = spline.Spline.EvaluatePosition(positionEvaluationCurve.Evaluate(time));
        return splinePos + spline.transform.position;
    }

    public float GetLength()
    {
        return _trackLength;
    }

    private void ComputePositionEvaluationCurve()
    {
        positionEvaluationCurve.ClearKeys();

        _curveLength = 0;
        _trackLength = 0;
        
        var sections = new List<SplineSection>();
        var currentStop = 0;
        
        Array.Sort(stopPoints, (x, y) => x.splinePoint.CompareTo(y.splinePoint));
        
        for (var i = 0; i < spline.Spline.Count; i++)
        {
            var section = spline.Spline.GetCurveLength(i);
            sections.Add(new SplineSection{distance = _trackLength, value = _curveLength});

            var stopDistance = 0f;
            
            if (i < stopPoints.Length && i == stopPoints[currentStop].splinePoint)
            {
                stopDistance = stopPoints[currentStop].distance;
                var s = sections[^1];
                s.flatten = true;
                sections[^1] = s;
                
                sections.Add(new SplineSection{distance = _trackLength + stopDistance, value = _curveLength, flatten = true});
                currentStop++;
            }
            
            _trackLength += section + stopDistance;
            _curveLength += section;
        }

        var keyCount = sections.Count - (spline.Spline.Closed ? 0 : 1);
        
        for (var i = 0; i < keyCount; i++)
        {
            positionEvaluationCurve.AddKey(new Keyframe(sections[i].distance / _trackLength, sections[i].value / _curveLength, 0, 0));
        }
        
        for (var i = 0; i < keyCount; i++)
        {
            if (!sections[i].flatten)
                positionEvaluationCurve.SmoothTangents(i, 0);
        }

        positionEvaluationCurve.AddKey(new Keyframe(1, 1, 0, 0));
        if (!sections[^1].flatten)
            positionEvaluationCurve.SmoothTangents(keyCount, 0);
    }
}
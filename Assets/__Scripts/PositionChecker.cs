using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionChecker : MonoBehaviour
{
    [HideInInspector]
    public bool isSafe = true;

    static private Transform _PositionChecker_ANCHOR;
    static Transform PositionChecker_ANCHOR
    {
        get
        {
            if (_PositionChecker_ANCHOR == null)
            {
                GameObject go = new GameObject("PositionCheckerAnchor");
                _PositionChecker_ANCHOR = go.transform;
            }
            return _PositionChecker_ANCHOR;
        }
    }

    void Start()
    {
        transform.SetParent(PositionChecker_ANCHOR, true);
    }
}
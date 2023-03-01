using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    [SerializeField] public static float leftSide = -4.5f;
    [SerializeField] public static float rightSide = 4.5f;
    [SerializeField] public float internalLeft;
    [SerializeField] public float internalRight;


    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        internalLeft = leftSide;
        internalRight = rightSide;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GenerateLevel : MonoBehaviour
{
    public GameObject[] section;
    public int zPos = 50;
    public bool creatingSection = false;
    public int secNum;

    // Update is called once per frame
    void Update()
    {
        if(creatingSection == false)
        {
            creatingSection = true;
            StartCoroutine(GenerateSection());
        }
    }

    IEnumerator GenerateSection()
    {
        Random random= new Random();
        secNum = random.Next(0,3);
        Instantiate(section[secNum], new Vector3(0,0, zPos), Quaternion.identity);
        zPos += 50;
        yield return new WaitForSeconds(6);
        creatingSection= false;
    }
}

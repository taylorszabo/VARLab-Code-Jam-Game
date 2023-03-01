using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public GameObject[] sandFloor;
    public int zPos = 50;
    public bool creatingSection = false;
    public int secNum;

    // Update is called once per frame
    void Update()
    {
        if (!creatingSection)
        {
            creatingSection = true;
            StartCoroutine(GenerateSection());
        }
    }

    IEnumerator GenerateSection()
    {
        secNum = Random.Range(0, 3);
        Instantiate(sandFloor[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        zPos += 50;
        yield return new WaitForSeconds(2);
        creatingSection = false;
    }
}


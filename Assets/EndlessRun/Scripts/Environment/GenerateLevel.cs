using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    [SerializeField] private const int AVAILABLE_SECTIONS = 3;
    [SerializeField] private const int SECTION_LENGTH = 120;

    [SerializeField] public GameObject[] section;
    [SerializeField] public int zPos = 120;
    [SerializeField] public bool creatingSection = false;
    [SerializeField] public int secNum;


    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

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
        secNum = Random.Range(0, AVAILABLE_SECTIONS);
        Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        zPos += SECTION_LENGTH;
        yield return new WaitForSeconds(secNum);        // Can be any tiny amount in seconds
        creatingSection = false;
    }
}

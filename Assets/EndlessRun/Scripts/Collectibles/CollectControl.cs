using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectControl : MonoBehaviour
{
    [SerializeField] public static int coinCount;
    public GameObject coinCountDisplay;

    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        coinCountDisplay.GetComponent<TextMeshProUGUI>().text = coinCount.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectableControl : MonoBehaviour
{
    public static int coinCount;
    public GameObject coinCountDisplay;

    // Update is called once per frame
    void Update()
    {
        coinCountDisplay.GetComponent<TextMeshProUGUI>().text = coinCount.ToString();
    }
}

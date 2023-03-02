using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCoin : MonoBehaviour
{
	// Start is called before the first frame update
	//void Start()
	//{

	//}

	// Update is called once per frame
	//void Update()
	//{

	//}

	[SerializeField] public AudioSource coinFX;

	private void OnTriggerEnter(Collider other)
	{
		coinFX.Play();
		this.gameObject.SetActive(false);
	}
}

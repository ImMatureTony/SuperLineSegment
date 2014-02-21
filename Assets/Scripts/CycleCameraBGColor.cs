using UnityEngine;
using System.Collections;

public class CycleCameraBGColor : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		iTween.ColorTo(GetComponent<tk2dCamera>().gameObject, iTween.Hash("r", UnityEngine.Random.Range(0f, 1f), "g", UnityEngine.Random.Range (0f, 1f), "b", UnityEngine.Random.Range(0f, 1f)));
	}
}

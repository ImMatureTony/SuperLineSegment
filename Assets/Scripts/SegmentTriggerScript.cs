using UnityEngine;
using System.Collections;

public class SegmentTriggerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter(Collider collider) {
		MainGameScript.Instance.TransitionToState(MainGameScript.GameState.GAME_OVER);
    }
}

using UnityEngine;
using System.Collections;

public class ScoreScript : MonoBehaviour
{
	static private tk2dTextMesh textMesh;
	static private float score;

	static public float Score
	{
		get
		{
			return score;	
		}
		
		set
		{
			score = value;	
		}
	}
	
	// Use this for initialization
	void Start ()
	{
		score = 0;
		textMesh = GetComponent<tk2dTextMesh>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (score >= 0) {
			textMesh.text = score.ToString("F2");
		}
		textMesh.Commit();
	}
	
	public float GetScore() {
		return score;	
	}
}
using UnityEngine;
using System.Collections;


public class ColorCycler : MonoBehaviour {
	
	public ScoreScript _scoreScript;
	private float _lastScore;
	public SegmentScript _segment;
	public MainGameScript _mainGameScript;
	public tk2dSprite _scrollBG;
	public tk2dTextMesh _scoreText;
	public tk2dTextMesh _titleTextOne;
	public tk2dTextMesh _titleTextTwo;
	public tk2dTextMesh _titleTextThree;
	public tk2dTextMesh _plotText;
	
	// Use this for initialization
	void Start () {
		_lastScore = 0f;
		SwitchToPalette(0);
	}
	
	// Update is called once per frame
	void Update () {
/*		if (_lastScore > 20f) {
			SwitchToPalette(UnityEngine.Random.Range(0, 4));
			_lastScore = 0f;
		}
		
		_lastScore += Time.deltaTime;*/
		
		float currentScore = _scoreScript.GetScore();
		if (Mathf.Floor(currentScore) > Mathf.Floor(_lastScore)) {
			SwitchToPalette(Mathf.FloorToInt(currentScore) % 8);
		}
			
		_lastScore = currentScore;
	}
	
	public void SwitchToPalette(int colorPalette) {
		Color bg = Color.white;
		Color bgAlternate = Color.white;
		Color obstacle = Color.white;
		
		switch(colorPalette) {
		// dark green
		case 0:
			bg = new Color(8f / 255f, 26f / 255f, 5f/ 255f, 1);
			bgAlternate = new Color(0, 0, 0, 1);
			obstacle = new Color(97f / 255f, 207f / 255f, 81f / 255f, 1);
			break;
		// dark orange
		case 2:
			bg = new Color(36f / 255f, 20f / 255f, 0f/ 255f, 1);
			bgAlternate = new Color(0, 0, 0, 1);
			obstacle = new Color(240f / 255f, 136f / 255f, 17f / 255f, 1);
			break;
		// dark blue
		case 1:
			obstacle = new Color(44f / 255f, 115f / 255f, 191f/ 255f, 1);
			bgAlternate = new Color(0f / 255f, 0f / 255f, 0f / 255f, 1);
			bg = new Color(3f / 255f, 16f / 255f, 33f / 255f, 1);
			break;
		// the black & white
		case 3:
			case 8:
			bg = new Color(0, 0, 0, 1);
			bgAlternate = new Color(0, 0, 0, 1);
			obstacle = new Color(1, 1, 1, 1);
			break;
		// light color
		case 4:
			bg = new Color(217f / 255f, 233f / 255f, 252f / 255f, 1);
			bgAlternate = new Color(1, 1, 1, 1);
			obstacle = new Color(24f / 255f,  117f / 255f, 240f / 255f, 1);
			break;
		// dark red
		case 6:
			bg = new Color(36f / 255f, 0f, 7f / 255f, 1);
			bgAlternate = new Color(0, 0, 0, 1);
			obstacle = new Color(227f / 255f, 23f / 255f, 50f / 255f, 1);
			break;
		// yellow
		case 5:
			bg = new Color(79f / 255f, 75f / 255f, 0f / 255f, 1);
			bgAlternate = new Color(0, 0, 0, 1);
			obstacle = new Color(244f / 255f, 247f / 255f, 22f / 255f, 1);
			break;
		// the grey
		case 7:	
			bg = new Color(220f / 255f, 220f / 255f, 220f / 255f, 1);
			bgAlternate = new Color(1, 1, 1, 1);
			obstacle = new Color(0, 0, 0, 1);
			break;
		}
		
		// Set BG Color (switch to alternate every other swish of the line)
		GetComponent<tk2dSprite>().color = bgAlternate;
		_scrollBG.color = bg;
		
		// Set Segment Color
		_segment.ResetToggleCounter();
		_segment.ShiftToColor(obstacle);
		_mainGameScript.ShiftObstaclesToColor(obstacle);
		_scoreText.color = obstacle;
		_titleTextOne.color = obstacle;
		_titleTextTwo.color = obstacle;
		_titleTextThree.color = obstacle;
		_plotText.color = obstacle;
		
		// WEKLJF
		_lastScore -= 1f;
	}
	
	// TODO IDEA: flicker new colors in?
}

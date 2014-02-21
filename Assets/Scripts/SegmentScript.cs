using UnityEngine;
using System.Collections;

public class SegmentScript : MonoBehaviour 
{
	public tk2dClippedSprite sprite;
	
	public ColorCycler _cycler;
	public bg_scroller _bgScroller;
	
	public tk2dSprite _scrollBG;
	public tk2dSprite _nonScrollBG;
	
	private float width;
	private Rect spriteRec;
	
	private bool engulfed;
	
	private float timeSinceStart = 0f;
	private float xMin = 0.48f;
	private float widthMin;
	
	private bool shouldOscillate = false;
	private bool shouldFlash = false;
	
	private Transform colliderTransform;
	
	private float lineStartTime = 0;
	
	private int _toggleCounter = 0;
	private bool _toggleEven = false;
	
	private int _jumpsTriggered = 0;
	private int _superJumpsTriggered = 0;
	
	private Vector3 _downVelocity;
	private Vector3 _defaultPositon;
	private Vector3 _defaultPositionTop;
	
	public Transform ColliderTransform
	{
		get
		{
			return colliderTransform;
		}
	}
	
	public void ShiftToColor(Color color) {
		transform.GetChild(0).gameObject.GetComponent<tk2dClippedSprite>().color = color;
	}
	
	void Start()
	{	
		_downVelocity = new Vector3(0, -175, 0);
		_defaultPositon = new Vector3(320, 0, 0);
		_defaultPositionTop = new Vector3(320, 1135, 0);
		
		// Get the 'size' of the line segment.
		Bounds bounds = sprite.GetUntrimmedBounds();
		width = bounds.max.x - bounds.min.x;
		
		// We want the segment to be mostly clipped on the left and right sides initially.
		spriteRec = sprite.ClipRect;
		spriteRec.x = xMin;
		widthMin = (0.5f - xMin) * 2;
		spriteRec.width = widthMin;
		sprite.ClipRect = spriteRec;

		colliderTransform = sprite.transform;
		
		// Register Segment with the main game script.
		MainGameScript.Instance.RegisterSegment(this);
		
		sprite.renderer.enabled = false;
	}
	
	public void ResetToggleCounter() {
		_toggleCounter = 0;	
	}
	
	public void StartDeath() {
		iTween.Stop(gameObject);
		_cycler.enabled = false;
		_cycler.SwitchToPalette(3);
		shouldOscillate = false;
		shouldFlash = true;
		rigidbody.velocity = Vector3.zero;
		rigidbody.useGravity = false;
	}
	
	public void FinishDeath() {
		shouldFlash = false;
		sprite.renderer.enabled = false;
		spriteRec.x = xMin;
		spriteRec.width = xMin + widthMin - spriteRec.x;
		sprite.ClipRect = spriteRec;
	}
	
	public void StartLiving() {
		shouldFlash = true;
		sprite.renderer.enabled = true;
		transform.position = new Vector3(transform.position.x, -4, transform.position.z);
		iTween.MoveTo(gameObject, iTween.Hash("y", 240, "easeType", iTween.EaseType.easeOutCirc, "time", 2f));
		_cycler.enabled = true;
		_bgScroller.ScrollBG();
		_cycler.SwitchToPalette(0);
	}
	
	public void FinishLiving() {
		shouldFlash = false;
		sprite.renderer.enabled = true;
		//_bgScroller.enabled = true;
		shouldOscillate = true;
		lineStartTime = Time.time;
		rigidbody.velocity = Vector3.zero;
		rigidbody.useGravity = true;
		
		//HACK TO GET LINE TO COLOR RIGHT
		_cycler.SwitchToPalette(0);
	}
	
	void FixedUpdate() {
		if (!(Time.time < lineStartTime || !shouldOscillate))
		{	
			if (rigidbody.velocity.y < _downVelocity.y) {
				rigidbody.velocity = _downVelocity;
			}
		
			if (transform.position.y >= 1135 || transform.position.y <= 0) {
				MainGameScript.Instance.TransitionToState(MainGameScript.GameState.GAME_OVER);
			}
			
/*			if (transform.position.y <= 2) {
				transform.position = _defaultPositon;	
				rigidbody.velocity = Vector3.zero;
			}*/
			
			if (_jumpsTriggered > 0) {
				rigidbody.velocity = Vector3.zero;
				rigidbody.AddForce (0, 300 * _jumpsTriggered, 0, ForceMode.Impulse);
				_jumpsTriggered = 0;
				iTween.ScaleTo(gameObject, iTween.Hash("y", 2.25f, "easeType", iTween.EaseType.easeOutExpo, "onComplete", "BackToNormalSize", "time", 0.15f));
				audio.pitch = 2.75f + UnityEngine.Random.Range(-0.15f, 0.15f);
				audio.Play();
			}
			
			if (_superJumpsTriggered > 0) {
				rigidbody.velocity = Vector3.zero;
				rigidbody.AddForce (0, 550 * _superJumpsTriggered, 0, ForceMode.Impulse);
				_superJumpsTriggered = 0;
				iTween.ScaleTo(gameObject, iTween.Hash("y", 3.5f, "easeType", iTween.EaseType.easeOutExpo,"onComplete", "BackToNormalSize", "time", 0.15f));
				audio.pitch = 5.75f + UnityEngine.Random.Range(-0.25f, 0.25f);
				audio.Play ();
			}
		}	
	}
	
	void BackToNormalSize() {
		iTween.ScaleTo(gameObject, iTween.Hash("y", 1f));	
	}
	
	// Main loop for the sprite.  Unclip the right side, snap back, unclip the left side, snap back, repeat. Like a strange horizontally bunjee jumping pendulum. Sinusoidal.
	void Update()
	{
		if (shouldFlash) {
			Color color = new Color(Random.Range(0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
			sprite.color = color;
		}
		
		// TODO: CHANGE ALL TIMING TO BE RELATIVE TO SONG TIME ELAPSED, SEE IF CAN PULL THAT INFORMATION.		
		if (Time.time < lineStartTime || !shouldOscillate) {
			return;
		}
		
		timeSinceStart = Time.time - lineStartTime;
		
		if (Input.GetButtonDown("Fire2")) {
			_jumpsTriggered++;
		} else if (Input.GetButtonDown("Fire1")) {
			_superJumpsTriggered++;	
		}
		
/*		// Get the x-coordinate for the endpoint stretching out from the center if the rightmost point is seen as 1 and the leftmost point seen as 0.
		float endpointRoughXPos = Mathf.Abs(Mathf.Cos(0.25f * MainGameScript.Instance.gameSpeed + timeSinceStart * MainGameScript.Instance.gameSpeed)) / 2;
				
		// See if we're at an even half-period, if so, we're going to the right, otherwise left.
		float halfPeriod = Mathf.Floor((0.25f * MainGameScript.Instance.gameSpeed + timeSinceStart * MainGameScript.Instance.gameSpeed) / (Mathf.PI));
		if (halfPeriod % 2 == 0) {
			endpointRoughXPos = 1 - endpointRoughXPos;
		}
		
		if (endpointRoughXPos > 0.5) {
			spriteRec.x = xMin;
			spriteRec.width = Mathf.Max(endpointRoughXPos - spriteRec.x, widthMin);
		} else {
			spriteRec.x = Mathf.Min(xMin, endpointRoughXPos);
			spriteRec.width = xMin + widthMin - spriteRec.x;
		}
/**/
/**/
		
		float endpointRoughXPos = (Mathf.Sin(timeSinceStart * MainGameScript.Instance.gameSpeed) + 1) / 2;
		
		if ((_toggleCounter == 0 || _toggleCounter == 2)) {
			if (_toggleEven && endpointRoughXPos > 0.5) {
				IncrementToggleCounter();
			} else if (!_toggleEven && endpointRoughXPos < 0.5) {
				IncrementToggleCounter();
			}
		} else {
			if (!_toggleEven && endpointRoughXPos > 0.5) {
				IncrementToggleCounter();
			} else if (_toggleEven && endpointRoughXPos < 0.5) {
				IncrementToggleCounter();
			}
		}
/* square wave		
 * float endpointRoughXPos = ((0.25f * MainGameScript.Instance.gameSpeed + timeSinceStart * 2f * Mathf.PI) % (MainGameScript.Instance.gameSpeed)) / MainGameScript.Instance.gameSpeed;
		if (endpointRoughXPos > 0.5f) {
			endpointRoughXPos = 0.5f - (endpointRoughXPos - 0.5f);	
		}
		
		endpointRoughXPos *= 2f;
		
*/
		if (endpointRoughXPos > 0.5) {
			spriteRec.x = xMin;
			spriteRec.width = Mathf.Max(endpointRoughXPos - spriteRec.x, widthMin);
		} else {
			spriteRec.x = Mathf.Min(xMin, endpointRoughXPos);
			spriteRec.width = xMin + widthMin - spriteRec.x;
		}
		
		if (endpointRoughXPos > 0.35 && endpointRoughXPos < 0.65) {
			//iTween.ScaleTo(gameObject, iTween.Hash("y", 3.5f, "easeType", iTween.EaseType.easeOutElastic));
			//sprite.color = new Color(1f, 0, 0, 1f);
//			iTween.ColorTo(sprite.gameObject, iTween.Hash("r", 255, "g", 0, "b", 0));
		} else {
			//iTween.ScaleTo(gameObject, iTween.Hash("y", 1.2f));
			//iTween.ColorTo(sprite.gameObject, iTween.Hash("r", 0, "g", 0, "b", 0));
			//sprite.color = new Color(0, 0, 0, 1f);
		}
/**/				
		sprite.ClipRect = spriteRec;
	}
	
	private void IncrementToggleCounter() {
		_toggleCounter++;
		if (_toggleCounter >= 4) {
			Color newBgColor = _scrollBG.color;
			Color newScrollBgColor = _nonScrollBG.color;
			newScrollBgColor.a = newBgColor.a;
			newBgColor.a = 1f;
			
			_scrollBG.color = newScrollBgColor;
			_nonScrollBG.color = newBgColor;
			
			_toggleCounter = 0;
			//_toggleEven = !_toggleEven;
		}
	}
}
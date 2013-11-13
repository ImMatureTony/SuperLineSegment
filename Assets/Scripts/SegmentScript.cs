using UnityEngine;
using System.Collections;

public class SegmentScript : MonoBehaviour 
{
	public tk2dClippedSprite sprite;
	
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
	
	public Transform ColliderTransform
	{
		get
		{
			return colliderTransform;
		}
	}

	void Start()
	{	
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
	
	public void StartDeath() {
		shouldOscillate = false;
		shouldFlash = true;
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
		iTween.MoveFrom(gameObject, iTween.Hash("y", -4, "easeType", iTween.EaseType.easeOutCirc, "time", 2f));
	}
	
	public void FinishLiving() {
		shouldFlash = false;
		sprite.renderer.enabled = true;
		sprite.color = new Color(0f, 0f, 0f, 1f);
		shouldOscillate = true;
		lineStartTime = Time.time;
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
		if (endpointRoughXPos > 0.5) {
			spriteRec.x = xMin;
			spriteRec.width = Mathf.Max(endpointRoughXPos - spriteRec.x, widthMin);
		} else {
			spriteRec.x = Mathf.Min(xMin, endpointRoughXPos);
			spriteRec.width = xMin + widthMin - spriteRec.x;
		}
/**/				
		sprite.ClipRect = spriteRec;
	}

}
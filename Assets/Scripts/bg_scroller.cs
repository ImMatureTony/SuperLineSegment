using UnityEngine;
using System.Collections;

public class bg_scroller : MonoBehaviour {
	
	private Vector3 _initialPos;
	private Vector3 _destPos;
	private tk2dSprite _sprite;
	
	// Use this for initialization
	void Start () {
		_initialPos = transform.position;
		
		ScrollBG();
	}
	
	public void ScrollBG() {
		iTween.Stop(gameObject);
		transform.position = _initialPos;
		transform.localScale = Vector3.one;
		_sprite = GetComponent<tk2dSprite>();
		_destPos = new Vector3(_initialPos.x, _initialPos.y - 1331, _initialPos.z);
		iTween.ScaleTo(gameObject, iTween.Hash("x", transform.localScale.x * 4f, "loopType", iTween.LoopType.pingPong, "time", 5f, "easeType", iTween.EaseType.linear));
		iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "loopType", iTween.LoopType.pingPong, "easeType", iTween.EaseType.spring, "onupdatetarget", gameObject, "onupdate", "tweenOnUpdateCallback"));
		iTween.MoveTo(gameObject, iTween.Hash("x", _destPos.x, "y", _destPos.y, "z", _destPos.z, "easeType", iTween.EaseType.linear, "time", 4f, "loopType", iTween.LoopType.loop));	
	}
	
	void tweenOnUpdateCallback( float newValue )
	{
	    _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, newValue);
	}
	
	// Update is called once per frame
	void Update () {
	}
}

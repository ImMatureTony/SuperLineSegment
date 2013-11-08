using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


public class MainGameScript : MonoBehaviour
{	
	private SegmentScript segment;
	
	public GameObject titleText;
	public GameObject startText;
	
	//All the obstacle prefabs should be assigned here.
	public Transform shapeOval;
	public Transform shapeRoundedRect;
	public Transform shapeRect;
	public Transform shapeEasyTriangle;
	public Transform shapeHardTriangle;
	
	public Dictionary<String, Transform> shapeStringToShape;
	
	public float gameSpeed;
	public float gameStartTime;
	private float accelerateTime = 0f;
	private float obstacleDropSpeedMultiplier = 0;
	
	private int startTextFlashTimer = 0;
	private int startTextFlashFrameCount = 15;
	
	private bool stationaryObstaclesSpawned = false;
	
	public Camera gameCam;
	
	public AudioClip sndGameOver;
	public AudioClip sndPoundingMotive;
	public AudioClip sndIntro;
	public AudioClip sndStart;
	
	private XmlDocument levelDoc;
	
	private struct Obstacle
	{
    	public string shape;
   	 	public float timing;
    	public string side;
	}
	private LinkedList<Obstacle> gameObstacleList = new LinkedList<Obstacle>();
	private LinkedList<Transform> activeObstacleList = new LinkedList<Transform>();
	
	public enum GameState
	{
		BLANK,
		PRE_INTRO,
    	INTRO,
		WAITING_FOR_GAME_START,
    	GAME_START,
    	IN_TRANSIT,
		GAME_OVER
	}
	private GameState gameState;
	
	// Treat this class as a singleton.  This will hold the instance of the class.
	private static MainGameScript instance;
	
	public static MainGameScript Instance
	{
		get
		{
			// This should NEVER happen, so we want to know about it if it does 
			if(instance == null)
			{
				Debug.LogError("MainGameScript instance does not exist");	
			}
			return instance;	
		}
	}

	void Awake()
	{
		instance = this;
	}
	
	public void TransitionToState(GameState state) {
		print("transitioning to state: " + state);
		
		// don't process it doubly if we're already in that state.
		if (state == gameState) return;
		
		gameState = state;
		
		startText.renderer.enabled = false;
		titleText.renderer.enabled = false;
		
		switch(gameState) {
			case GameState.PRE_INTRO:
				// Pre-intro just exists to give things a chance to initialize and time themselves correctly.
				Invoke("TransitionToNextState", 1f);
				break;
			case GameState.INTRO:
				titleText.renderer.enabled = true;
				audio.clip = sndIntro;
				audio.Play();
				// intro naturally times out and becomes waiting for game start
				Invoke("TransitionToNextState", audio.clip.length);
				break;
			case GameState.WAITING_FOR_GAME_START:
				ResetLevel();
				startTextFlashTimer = 0;
				break;
			case GameState.IN_TRANSIT:
				audio.clip = sndPoundingMotive;
				audio.loop = true;
				audio.Play();
				segment.FinishLiving();
				break;
			case GameState.GAME_OVER:
				audio.clip = sndGameOver;
				audio.loop = false;
				audio.Play();
				StopAllObstacles();
				Invoke("RecedeObstacles", audio.clip.length / 3f);
				Invoke("FinishSegmentDeath", audio.clip.length / 2f);
				Invoke("ResetLevel", audio.clip.length);
				Invoke("TransitionToNextState", audio.clip.length);
				segment.StartDeath();
				break;
			case GameState.GAME_START:
				gameStartTime = Time.time;
				audio.clip = sndStart;
				audio.loop = false;
				audio.Play();
				// game start naturally times out and becomes in transit
				Invoke("TransitionToNextState", audio.clip.length);		
				ResetLevel();
				segment.StartLiving();
				break;
			default:
            	break;
		}
	}
	
	void FinishSegmentDeath() {
		segment.FinishDeath();	
	}
	
	void TransitionToNextState() {
		switch(gameState) {
			case GameState.PRE_INTRO:
				TransitionToState(GameState.INTRO);
				break;
			case GameState.INTRO:
				TransitionToState(GameState.GAME_START);
				break;
			case GameState.IN_TRANSIT:
				break;
			case GameState.WAITING_FOR_GAME_START:
				TransitionToState(GameState.GAME_START);
				break;
			case GameState.GAME_OVER:
				TransitionToState(GameState.WAITING_FOR_GAME_START);
				break;
			case GameState.GAME_START:
				TransitionToState(GameState.IN_TRANSIT);
				break;
		}
	}
	
	void Start () 
	{	
		shapeStringToShape = new Dictionary<string, Transform> {
	    	{ "EasyOval", shapeOval},
			{ "EasyRect", shapeRect},
			{ "RoundedRect", shapeRoundedRect},
			{ "EasyTriangle", shapeEasyTriangle},
			{ "HardTriangle", shapeHardTriangle}
		};
		
		levelDoc = new XmlDocument();
		levelDoc.Load("./Assets/LevelData/Startpoint.xml");
		
		TransitionToState(GameState.PRE_INTRO);
	}
	
	void FixedUpdate()
	{
		switch(gameState) {
			case GameState.INTRO:
				// Check to see if accelerate button is being held down, and if so start the game early.
				if (Input.GetAxis("Fire1") > 0)
				{
					CancelInvoke("TransitionToNextState");
					TransitionToState(GameState.GAME_START);
				}
				break;
			case GameState.IN_TRANSIT:
				// Check to see if accelerate button is being held down, and if so change vertical speed of things in some way that feels "right".
				if (Input.GetAxis("Fire1") > 0)
				{
					accelerateTime += Time.fixedDeltaTime;
				}
				HandleObstacleSpawning();
				HandleObstacleDropping();
				break;
			case GameState.GAME_OVER:
				break;
			case GameState.WAITING_FOR_GAME_START:
				if (startTextFlashTimer >= startTextFlashFrameCount) {
					startText.renderer.enabled = !startText.renderer.enabled;
					startTextFlashTimer -= startTextFlashFrameCount;
				}
				startTextFlashTimer++;
				// Check to see if accelerate button is being held down, and if so start the game.
				if (Input.GetAxis("Fire1") > 0)
				{
					TransitionToNextState();
				}
				break;
			case GameState.GAME_START:
				if (startTextFlashTimer >= startTextFlashFrameCount) {
					startText.renderer.enabled = !startText.renderer.enabled;
					startTextFlashTimer -= startTextFlashFrameCount;
				}
				startTextFlashTimer++;
				HandleObstacleSpawning();
				HandleObstacleDropping();
				break;
			default:
            	break;
		}
	}
	
	void Update()
	{	
		switch(gameState) {
			case GameState.INTRO:
				break;
			case GameState.IN_TRANSIT:
				break;
			case GameState.GAME_OVER:
				break;
			case GameState.GAME_START:
				// check if start music is done playing, if so, switch to in transit state.
				if (!audio.isPlaying) {
					TransitionToState(GameState.IN_TRANSIT);
				}
				break;
			default:
            	break;
		}
	}
	
	private void StopAllObstacles() {
		Vector3 obstacleStopVelocity = new Vector3(0, 0, 0);
		foreach(Transform obstacle in activeObstacleList) {
			obstacle.rigidbody.velocity = obstacleStopVelocity;
		}
		stationaryObstaclesSpawned = false;
	}
	
	private void HandleObstacleDropping() {
		float newObstacleDropSpeedMultiplier = 1f;
		if (Input.GetAxis("Fire1") > 0 && gameState.Equals(GameState.IN_TRANSIT))
		{
			newObstacleDropSpeedMultiplier = 2f;
		}
		
		// only if the speed multiplier has changed do we reset the velocities on the obstacles.
		if ((newObstacleDropSpeedMultiplier != obstacleDropSpeedMultiplier) || stationaryObstaclesSpawned) {
			obstacleDropSpeedMultiplier = newObstacleDropSpeedMultiplier;
			Vector3 obstacleVelocity = new Vector3(0, -40 * gameSpeed * obstacleDropSpeedMultiplier, 0);
			foreach(Transform obstacle in activeObstacleList) {
				obstacle.rigidbody.velocity = obstacleVelocity;
			}
			stationaryObstaclesSpawned = false;
		}
	}
	
	private void HandleObstacleSpawning()
	{
		float timeSinceStart = Time.time - gameStartTime + accelerateTime;
		
		LinkedList<Obstacle> toSpawn = new LinkedList<Obstacle>();
		
		// For now, loop through every obstacle on every frame, probably change this later (unless no performance problems are found...)
		foreach(Obstacle obs in gameObstacleList) {
			if (obs.timing <= (timeSinceStart / gameSpeed)) {
				toSpawn.AddLast(obs);
			}
		}

		foreach(Obstacle obs in toSpawn) {
			Vector3 spawnPos = new Vector3(0, 1136, 0);
			Transform obsTransform = (Transform) Instantiate(shapeStringToShape[obs.shape], spawnPos, Quaternion.identity);
			if (obs.side.Equals("right")) {
				obsTransform.Translate(new Vector3(640, 0, 0));
				obsTransform.Rotate(new Vector3(0, 180, 0));
			}
			activeObstacleList.AddLast(obsTransform);
			stationaryObstaclesSpawned = true;
			gameObstacleList.Remove(obs);
		}
	}
	
	private void RecedeObstacles() {
		foreach(Transform obstacle in activeObstacleList) {
			if (obstacle.position.x <= 320) {
				// move off to the left.
				iTween.MoveTo(obstacle.gameObject, iTween.Hash("x", -320, "easeType", iTween.EaseType.easeInOutExpo, "loopType", "none", "delay", UnityEngine.Random.Range(0F, 1F)));
			} else {
				// move off to the right.
				iTween.MoveTo(obstacle.gameObject, iTween.Hash("x", 960, "easeType", iTween.EaseType.easeInOutExpo, "loopType", "none", "delay", UnityEngine.Random.Range(0f, 1f)));
			}
		}
	}
	
	private void ResetLevel() {
		// 1. Clear out the level, destroying any obstacle objects/data leftover from last run.
		foreach(Transform activeObs in activeObstacleList) {
			Destroy(activeObs.gameObject);		
		}
		activeObstacleList.Clear();
		gameObstacleList.Clear();
		
		// 2. Reset accelerate time to 0.
		accelerateTime = 0f;
		
		// 3. Fill in the level
		foreach(XmlNode node in levelDoc.DocumentElement.ChildNodes){
			Obstacle obstacle = new Obstacle();
			
			obstacle.shape = node.SelectSingleNode("SHAPE").InnerText;
			obstacle.timing = Convert.ToSingle(node.SelectSingleNode("TIMING").InnerText);
			obstacle.side = node.SelectSingleNode("SIDE").InnerText;
			
			gameObstacleList.AddLast(obstacle);
		}	
	}
	
	public void RegisterSegment(SegmentScript seg)
	{
		segment = seg;
	}
	
	void OnTriggerEnter(Collider collider) {
        print("line collided");
    }
}

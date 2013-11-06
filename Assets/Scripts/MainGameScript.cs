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
	public float gameStartTime = 99999999999999;
	private float accelerateTime = 0f;
	public float obstacleSpeedMultiplier;
	
	private int startTextFlashTimer = 0;
	private int startTextFlashFrameCount = 15;
	
	public Camera gameCam;
	
	public AudioClip sndGameOver;
	public AudioClip sndPoundingMotive;
	public AudioClip sndIntro;
	public AudioClip sndStart;
	
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
		PRE_INTRO,
    	INTRO,
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
	
	void TransitionToState(GameState state) {
		print("transitioning to state: " + state);
		
		gameState = state;
		
		startText.renderer.enabled = false;
		titleText.renderer.enabled = false;
		
		switch(gameState) {
			case GameState.PRE_INTRO:
				// Pre-intro just exists to give things a chance to initialize and time themselves correctly.
				Invoke("TransitionToNextState", 0.5f);
				break;
			case GameState.INTRO:
				titleText.renderer.enabled = true;
				audio.clip = sndIntro;
				audio.Play();
				// intro naturally times out and becomes waiting for game start
				Invoke("TransitionToNextState", audio.clip.length);
				break;
			case GameState.IN_TRANSIT:
				gameStartTime = Time.realtimeSinceStartup;
				audio.clip = sndPoundingMotive;
				audio.loop = true;
				audio.Play();
				break;
			case GameState.GAME_OVER:
				break;
			case GameState.GAME_START:
				audio.clip = sndStart;
				audio.loop = false;
				audio.Play();
				startText.renderer.enabled = false;
				startTextFlashTimer = 0;
				// game start naturally times out and becomes in transit
				Invoke("TransitionToNextState", audio.clip.length);
				break;
			default:
            	break;
		}
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
			case GameState.GAME_OVER:
				TransitionToState(GameState.GAME_START);
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
		
		ReadInLevelData();
		TransitionToState(GameState.PRE_INTRO);
	}
	
	void FixedUpdate()
	{
		switch(gameState) {
			case GameState.INTRO:
				break;
			case GameState.IN_TRANSIT:
				// Check to see if accelerate button is being held down, and if so change vertical speed of things in some way that feels "right".
				if (Input.GetAxis("Fire1") > 0)
				{
					accelerateTime += Time.fixedDeltaTime;
				}
				break;
			case GameState.GAME_OVER:
				break;
			case GameState.GAME_START:
				if (startTextFlashTimer >= startTextFlashFrameCount) {
					startText.renderer.enabled = !startText.renderer.enabled;
					startTextFlashTimer -= startTextFlashFrameCount;
				}
				startTextFlashTimer++;
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
		if (gameState.Equals(GameState.INTRO)) {

		}
		
		HandleObstacleSpawning();
		HandleObstacleDropping();
	}
	
	private void HandleObstacleDropping() {
		float userInputSpeedMultiplier = 1f;
		if (Input.GetAxis("Fire1") > 0)
		{
			userInputSpeedMultiplier = 2f;
			print ("YUPPP");
		}
		
		foreach(Transform obstacle in activeObstacleList) {
			obstacle.Translate(0, userInputSpeedMultiplier * obstacleSpeedMultiplier * Time.deltaTime * gameSpeed * -1, 0);
		}
	}
	
	private void HandleObstacleSpawning()
	{
		float timeSinceStart = Time.realtimeSinceStartup - gameStartTime + accelerateTime;
		
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
			gameObstacleList.Remove(obs);
		}
		
		
	}
	
	private void ReadInLevelData()
	{	
		XmlDocument doc = new XmlDocument();
		doc.Load("./Assets/LevelData/Startpoint.xml");
		
		// For each Obstacle listed in the level data...
		foreach(XmlNode node in doc.DocumentElement.ChildNodes){
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
}

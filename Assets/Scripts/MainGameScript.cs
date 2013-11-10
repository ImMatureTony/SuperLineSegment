using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class MainGameScript : MonoBehaviour
{	
	private SegmentScript segment;
	
	public GameObject titleText;
	public GameObject titleTextTwo;
	public GameObject titleTextThree;
	public GameObject plotText;
	public GameObject startText;
	public GameObject scoreText;
	
	private bool haveHadAGameOver = false;
	
	private int currentStartTextMessageIndex = 0;
	private string[] startTextMessages;
	
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
	private float currentVelocity = 0f;
	
	private int startTextFlashTimer = 0;
	private int startTextFlashFrameCount = 15;
	
	private bool stationaryObstaclesSpawned = false;
	
	public Camera gameCam;
	
	public AudioClip sndGameOver;
	public AudioClip sndPoundingMotive;
	public AudioClip sndIntro;
	public AudioClip sndStart;
	
	private XmlDocument[] obstacleDocuments;
	
	private bool oughtToMirrorChunk;
	
	private struct Obstacle
	{
    	public string shape;
   	 	public float timing;
		public float rotation;
		public float xTweak;
		public float yTweak;
    	public string side;
	}
	private LinkedList<Obstacle> gameObstacleList = new LinkedList<Obstacle>();
	private LinkedList<Transform> activeObstacleList = new LinkedList<Transform>();
	
	private List<List<Obstacle>> easyChunks = new List<List<Obstacle>>();
	private List<List<Obstacle>> lesserMediumChunks = new List<List<Obstacle>>();
	private List<List<Obstacle>> mediumChunks = new List<List<Obstacle>>();	
	private List<List<Obstacle>> hardChunks = new List<List<Obstacle>>();
	
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
	
	private void HideScoreText() {
		iTween.MoveTo(scoreText, iTween.Hash("y", 1300, "easeType", iTween.EaseType.easeInOutExpo));	
	}
	
	private void AnimateHideTitle() {
		iTween.MoveBy(titleText.gameObject, iTween.Hash("y", 1000, "easeType", iTween.EaseType.easeInOutExpo, "time", 2));
		iTween.MoveBy(titleTextTwo.gameObject, iTween.Hash("y", 1000, "easeType", iTween.EaseType.easeInOutExpo, "time", 2));
		iTween.MoveBy(titleTextThree.gameObject, iTween.Hash("y", 1000, "easeType", iTween.EaseType.easeInOutExpo, "time", 2));
		AnimatePlot();
	}
	
	private void AnimatePlot() {
		plotText.renderer.enabled = true;
		iTween.MoveFrom(plotText.gameObject, iTween.Hash("y", -300, "easeType", iTween.EaseType.easeInOutExpo, "time", 3));
	}
	
	private void AnimatePlotText() {
		String text = "Engaging Startpoint";
		int numDots = (int) (Time.fixedTime * 4 % 3) + 1;
		while (numDots > 0) {
			text += ".";
			numDots--;	
		}
		plotText.GetComponent<tk2dTextMesh>().text = text;
		plotText.GetComponent<tk2dTextMesh>().Commit();
	}
	
	public void TransitionToState(GameState state) {
		Debug.Log("transitioning to state: " + state);
		
		// don't process it doubly if we're already in that state.
		if (state == gameState) return;
		
		gameState = state;
		
		startText.renderer.enabled = false;
		titleText.renderer.enabled = false;
		titleTextTwo.renderer.enabled = false;
		titleTextThree.renderer.enabled = false;
		plotText.renderer.enabled = false;
		
		switch(gameState) {
			case GameState.PRE_INTRO:
				// Pre-intro just exists to give things a chance to initialize and time themselves correctly.
				titleText.renderer.enabled = true;
				titleTextTwo.renderer.enabled = true;
				titleTextThree.renderer.enabled = true;
				iTween.MoveFrom(titleText.gameObject, iTween.Hash("x", -200, "easeType", iTween.EaseType.easeInOutExpo, "time", 3.5f));
				iTween.MoveFrom(titleTextTwo.gameObject, iTween.Hash("x", 840, "easeType", iTween.EaseType.easeInOutExpo, "time", 5.5f));
				iTween.MoveFrom(titleTextThree.gameObject, iTween.Hash("x", -200, "easeType", iTween.EaseType.easeInOutExpo, "time", 7.5f));
				Invoke("AnimateHideTitle", 7.5f);
				Invoke("TransitionToNextState", 1f);
				break;
			case GameState.INTRO:
				titleText.renderer.enabled = true;
				titleTextTwo.renderer.enabled = true;
				titleTextThree.renderer.enabled = true;
				audio.clip = sndIntro;
				audio.Play();
				// intro naturally times out and becomes waiting for game start
				Invoke("TransitionToNextState", audio.clip.length);
				break;
			case GameState.WAITING_FOR_GAME_START:
				iTween.MoveTo(gameCam.gameObject, iTween.Hash("y", 0, "easeType", iTween.EaseType.linear, "time", 0.01));
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
				Application.ExternalCall("kongregate.stats.submit", "high_score", (int) (ScoreScript.Score * 100));
				haveHadAGameOver = true;
				currentStartTextMessageIndex = 0;
				iTween.Stop();
				iTween.Stop(gameCam.gameObject);
				iTween.Stop(scoreText);
				audio.clip = sndGameOver;
				audio.loop = false;
				audio.Play();
				StopAllObstacles();
				Invoke("RecedeObstacles", audio.clip.length / 3f);
				Invoke("FinishSegmentDeath", audio.clip.length / 2f);
				Invoke("HideScoreText", audio.clip.length / 2f);
				Invoke("ResetLevel", audio.clip.length);
				Invoke("TransitionToNextState", audio.clip.length);
				segment.StartDeath();
				break;
			case GameState.GAME_START:
				ScoreScript.Score = 0;
				gameStartTime = Time.fixedTime;
				audio.clip = sndStart;
				audio.loop = false;
				audio.Play();
				
				startText.GetComponent<tk2dTextMesh>().text = "WARNING:\nNO ENDPOINT FOUND\n";
				startText.GetComponent<tk2dTextMesh>().Commit();
			
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
		startTextMessages = new string[6] {
			"ERROR:\nSEGMENT AT FAULT!\n",
			"ERROR:\nSEGMENT AT FAULT!\n",
			"SPACE FOR COURAGE",
			"SPACE FOR COURAGE",
			"CTRL FOR CAUTION",
			"CTRL FOR CAUTION"
		};
		
		shapeStringToShape = new Dictionary<string, Transform> {
	    	{ "EasyOval", shapeOval},
			{ "EasyRect", shapeRect},
			{ "RoundedRect", shapeRoundedRect},
			{ "EasyTriangle", shapeEasyTriangle},
			{ "HardTriangle", shapeHardTriangle}
		};
		oughtToMirrorChunk = UnityEngine.Random.Range(0,1) == 0;
		
		// read in obstacle data.
		XmlDocument obstaclesEasyDoc = new XmlDocument();
		obstaclesEasyDoc.LoadXml(ObstacleData.easyObstacles);
		XmlDocument obstaclesMediumDoc = new XmlDocument();
		obstaclesMediumDoc.LoadXml(ObstacleData.mediumObstacles);
		XmlDocument obstaclesHardDoc = new XmlDocument();
		obstaclesHardDoc.LoadXml(ObstacleData.hardObstacles);
		
		FillChunkListFromXml(easyChunks, ObstacleData.easyObstacles);
		FillChunkListFromXml(lesserMediumChunks, ObstacleData.lesserMediumObstacles);
		FillChunkListFromXml(mediumChunks, ObstacleData.mediumObstacles);
		FillChunkListFromXml(hardChunks, ObstacleData.hardObstacles);
		
		TransitionToState(GameState.PRE_INTRO);
	}
	
	private void FillChunkListFromXml(List<List<Obstacle>> chunkList, String chunkData) {
		XmlDocument obstacleDoc = new XmlDocument();
		obstacleDoc.LoadXml(chunkData);
		
		XmlNodeList chunks = obstacleDoc.FirstChild.SelectNodes("CHUNK");
		foreach(XmlNode chunkNode in chunks){
			List<Obstacle> chunk = new List<Obstacle>();
			foreach(XmlNode node in chunkNode.SelectNodes("OBSTACLE")) {
				Obstacle obstacle = new Obstacle();
				
				obstacle.shape = node.SelectSingleNode("SHAPE").InnerText;
				obstacle.timing = Convert.ToSingle(node.SelectSingleNode("TIMING").InnerText);
				
				XmlNode rotNode = node.SelectSingleNode("ROTATION");
				if (rotNode != null) {
					obstacle.rotation = Convert.ToSingle(rotNode.InnerText);	
				} else {
					obstacle.rotation = 0;	
				}
				
				XmlNode xTweakNode = node.SelectSingleNode("X_TWEAK");
				if (xTweakNode != null) {
					obstacle.xTweak = Convert.ToSingle(xTweakNode.InnerText);	
				} else {
					obstacle.xTweak = 0;	
				}
				
				XmlNode yTweakNode = node.SelectSingleNode("Y_TWEAK");
				if (yTweakNode != null) {
					obstacle.yTweak = Convert.ToSingle(yTweakNode.InnerText);	
				} else {
					obstacle.yTweak = 0;	
				}
				
				if (node.SelectSingleNode("SIDE").InnerText.Equals("left")) {
					obstacle.side = "right";	
				} else {
					obstacle.side = "left";
				}
	
				chunk.Add(obstacle);
			}
			chunkList.Add(chunk);
		}
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
				AnimatePlotText();
				break;
			case GameState.IN_TRANSIT:
				// Check to see if accelerate button is being held down, and if so change vertical speed of things in some way that feels "right".
				if (Input.GetAxis("Fire1") > 0) {
					iTween.MoveTo(gameCam.gameObject, iTween.Hash("y", -100, "easeType", iTween.EaseType.linear));
					iTween.MoveTo(scoreText, iTween.Hash("y", 1100 - 100, "easeType", iTween.EaseType.linear));
				} else if (Input.GetAxis ("Fire2") > 0) {
					iTween.MoveTo(gameCam.gameObject, iTween.Hash("y", 100, "easeType", iTween.EaseType.linear));
					iTween.MoveTo(scoreText, iTween.Hash("y", 1100 + 100, "easeType", iTween.EaseType.linear));
				} else {
					iTween.MoveTo(gameCam.gameObject, iTween.Hash("y", 0, "easeType", iTween.EaseType.linear));
					iTween.MoveTo(scoreText, iTween.Hash("y", 1100, "easeType", iTween.EaseType.linear));
				}
				
				ScoreScript.Score = TimeSinceStart() / 30;
			
				HandleObstacles();	
			
				break;
			case GameState.GAME_OVER:
				break;
			case GameState.WAITING_FOR_GAME_START:
				if (startTextFlashTimer >= startTextFlashFrameCount) {
					startText.renderer.enabled = !startText.renderer.enabled;
					startTextFlashTimer -= startTextFlashFrameCount;
					if (haveHadAGameOver && startText.renderer.enabled) {
						startText.GetComponent<tk2dTextMesh>().text = startTextMessages[currentStartTextMessageIndex];
						startText.GetComponent<tk2dTextMesh>().Commit();
						currentStartTextMessageIndex = (currentStartTextMessageIndex + 1) % startTextMessages.Length;
					}
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
			
				ScoreScript.Score = TimeSinceStart() / 30;
			
				iTween.MoveTo(scoreText, iTween.Hash("y", 1100, "easeType", iTween.EaseType.linear));
			
				HandleObstacles();
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
			// HACKY: undo whatever translation happened in this frame? hmm....
			obstacle.Translate(new Vector3(0, -1 / 30 * obstacle.rigidbody.velocity.y, 0));
			obstacle.rigidbody.velocity = obstacleStopVelocity;
		}
		stationaryObstaclesSpawned = false;
	}
	
	private void HandleObstacles() {
		HandleObstacleDropping();
		HandleObstaclePruning();
		HandleObstacleSpawning();
	}
	
	private void HandleObstacleDropping() {
		//Benchy.Begin();
		float newObstacleDropSpeedMultiplier = 1f;
		if (Input.GetAxis("Fire1") > 0 && gameState.Equals(GameState.IN_TRANSIT))
		{
			newObstacleDropSpeedMultiplier = 2f;
			accelerateTime += Time.fixedDeltaTime;
		} else if (Input.GetAxis("Fire2") > 0 && gameState.Equals(GameState.IN_TRANSIT))
		{
			newObstacleDropSpeedMultiplier = 0.5f;
			accelerateTime -= Time.fixedDeltaTime / 2f;
		}
		
		// only if the speed multiplier has changed do we reset the velocities on the obstacles.
		if ((newObstacleDropSpeedMultiplier != obstacleDropSpeedMultiplier) || stationaryObstaclesSpawned) {
			obstacleDropSpeedMultiplier = newObstacleDropSpeedMultiplier;
			currentVelocity = -50 * gameSpeed * obstacleDropSpeedMultiplier;
			Vector3 obstacleVelocity = new Vector3(0, currentVelocity, 0);
			foreach(Transform obstacle in activeObstacleList) {
				obstacle.rigidbody.velocity = obstacleVelocity;
			}
			stationaryObstaclesSpawned = false;
		}
		//Benchy.End();
	}
	
	private void HandleObstaclePruning() {
		LinkedList<Transform> toPrune = new LinkedList<Transform>();
		
		foreach(Transform obstacle in activeObstacleList) {
			if (obstacle.position.y < -300) {
				toPrune.AddLast(obstacle);
			}
		}
	
		foreach(Transform obsToPrune in toPrune) {
			activeObstacleList.Remove(obsToPrune);
			Destroy(obsToPrune.gameObject);
		}
	}
	
	private void AddNewChunkToSpawnList() {
//		Benchy.Begin();
		
		// If we're out of obstacles to spawn, add a new chunk to the list, every 30 seconds, a new difficulty of chunk pieces is unlocked as a possibility.
		// up the difficulty every 30 seconds?
		int chunkDifficulty = UnityEngine.Random.Range(0, (int) Math.Min(1, Math.Ceiling(TimeSinceStart() / 5)));
		List<List<Obstacle>> chunksForSelectedDifficulty = null;
		if (chunkDifficulty == 0) {
			chunksForSelectedDifficulty = easyChunks;	
		} else if (chunkDifficulty == 1) {
			chunksForSelectedDifficulty = lesserMediumChunks;
		} else if (chunkDifficulty == 2) {
			chunksForSelectedDifficulty = mediumChunks;	
		} else if (chunkDifficulty == 3) {
			chunksForSelectedDifficulty = hardChunks;	
		}
		
		List<Obstacle> chunkToSpawn = chunksForSelectedDifficulty[UnityEngine.Random.Range(0, chunksForSelectedDifficulty.Count)];
		
		for(int i = 0; i < chunkToSpawn.Count; i++) {
			Obstacle node = chunkToSpawn[i];
			
			node.timing = TimeSinceStart() / gameSpeed + node.timing * 0.8f;
			
			if (oughtToMirrorChunk) {
				if (node.side == "left") {
					node.side = "right";
				} else {
					node.side = "left";	
				}
			}
		
			gameObstacleList.AddLast(node);
		}
		
		oughtToMirrorChunk = !oughtToMirrorChunk;
		
//		Benchy.End ();
	}
	
	private float TimeSinceStart() {
		return Time.fixedTime - gameStartTime + accelerateTime;	
	}
	
	private void HandleObstacleSpawning() {
		if (gameObstacleList.Count == 0) {
			AddNewChunkToSpawnList();
		}
		
		SpawnObstacles();
		
	}
	
	private void SpawnObstacles() {
		LinkedList<Obstacle> toSpawn = new LinkedList<Obstacle>();

		float timingCompareValue = TimeSinceStart() / gameSpeed;
		foreach(Obstacle obs in gameObstacleList) {
			if (obs.timing < timingCompareValue) {
				toSpawn.AddLast(obs);
			}
		}
		
		foreach(Obstacle obs in toSpawn) {
			if (!obs.shape.Equals("EndPiece")) {
				Vector3 spawnPos = new Vector3(-1, 1248, 0);
				Transform obsTransform = (Transform) Instantiate(shapeStringToShape[obs.shape], spawnPos, Quaternion.identity);
				if (obs.side.Equals("right")) {
					obsTransform.Translate(new Vector3(642, 0, 0));
					obsTransform.Rotate(new Vector3(0, 180, 0));
				}
				
				float worldScaleYTweak = currentVelocity * (timingCompareValue - obs.timing) * gameSpeed / obstacleDropSpeedMultiplier;
				obsTransform.Translate(0, worldScaleYTweak, 0);
				
				obsTransform.Rotate(0, 0, -1 * obs.rotation);
				obsTransform.Translate(obs.xTweak, obs.yTweak, 0);
				activeObstacleList.AddLast(obsTransform);

				int tweenX = 300;
				obsTransform.Translate(-1 * tweenX, 0, 0);
				iTween.MoveBy(obsTransform.gameObject, iTween.Hash("x", tweenX, "easeType", iTween.EaseType.easeInOutExpo, "loopType", "none"));
				
				stationaryObstaclesSpawned = true;
			}
			gameObstacleList.Remove(obs);
		}
	}
	
	private void RecedeObstacles() {
		foreach(Transform obstacle in activeObstacleList) {
			iTween.MoveBy(obstacle.gameObject, iTween.Hash("x", -320, "easeType", iTween.EaseType.easeInOutExpo, "loopType", "none", "delay", UnityEngine.Random.Range(0F, 1F)));
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
	}
	
	public void RegisterSegment(SegmentScript seg)
	{
		segment = seg;
	}
}

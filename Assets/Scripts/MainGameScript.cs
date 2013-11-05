using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class MainGameScript : MonoBehaviour
{	
	private SegmentScript segment;
	
	public GameObject titleText;
	
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
	public float obstacleSpeedMultiplier;
	
	public Camera gameCam;
	public AudioClip poundingMotive;
	
	private struct Obstacle
	{
    	public string shape;
   	 	public float timing;
    	public string side;
	}
	private LinkedList<Obstacle> gameObstacleList = new LinkedList<Obstacle>();
	private LinkedList<Transform> activeObstacleList = new LinkedList<Transform>();
	
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
		
		AudioSource.PlayClipAtPoint(poundingMotive, new Vector3());
	}
	
	void FixedUpdate()
	{
		// Check to see if accelerate button is being held down, and if so change vertical speed of things in some way that feels "right".
		if (Input.GetAxis("Fire1") > 0 && Time.realtimeSinceStartup > gameStartTime)
		{
			accelerateTime += Time.fixedDeltaTime;	
		}
	}
	
	void Update()
	{
		// Check to see if accelerate button is being held down, and if so change vertical speed of things in some way that feels "right".
		if(Input.GetButtonDown ("Fire1"))
		{
			
		}
		
		// Hide title text if the game has started
		if (Time.realtimeSinceStartup >= gameStartTime) {
			Destroy(titleText);
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

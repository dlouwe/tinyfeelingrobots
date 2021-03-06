﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random; 

public class BoardManager : MonoBehaviour {

  [Serializable]
  public class Count
  {
      public int minimum; //Minimum value for our Count class.
      public int maximum; //Maximum value for our Count class.
      
      //Assignment constructor.
      public Count (int min, int max)
      {
          minimum = min;
          maximum = max;
      }
  }
  
  private Text debugText;
  private float debugRefresh = 50f;
  private float cycleCount = 50f;
  private float totalCycles = 0f;

  public LayerMask blockingLayer;

  public GameObject water;
  public GameObject grass;
  public GameObject ground;
  public GameObject herbivore;

  public int gridWidth = 50;
  public int gridHeight = 50;
  public float grassChance = 0.15f;

  private int waterSeedsMin = 5;
  private int waterSeedsMax = 15;
  private int waterMax = 800;

  private Transform boardHolder;

  public List <Brain> brains = new List <Brain> ();

  void InitializeGrid() {

    debugText = GameObject.Find("DebugText").GetComponent<Text>();
    debugText.text = "";
    
    water.name = "Water";
    LayoutObjectAtRandom(water, waterSeedsMin, waterSeedsMax);

    GameObject[] waterTiles = GameObject.FindGameObjectsWithTag("Water");

    while( waterTiles.Length <= waterMax ) {

      // get random water tile
      int randIndex = Random.Range(0, waterTiles.Length);

      waterTiles[randIndex].GetComponent<Water>().AttemptGrow();
      
      waterTiles = GameObject.FindGameObjectsWithTag("Water");
    }

    foreach (GameObject waterTile in waterTiles) {
      waterTile.GetComponent<Water>().checkDepth();
    }

    // calculate how many grass squares to add
    int totalSquares = gridHeight * gridWidth;
    int grassSeedsMax = Mathf.RoundToInt(totalSquares * grassChance);
    int grassSeedsMin = Mathf.RoundToInt(grassSeedsMax * grassChance);

    LayoutEntityAtRandom(grass, grassSeedsMin, grassSeedsMax, 200, 20, 40);
    LayoutEntityAtRandom(herbivore, 10, 10, 3, 5, 10);

  }

  Vector2 RandomPosition() {
    // failsafe to prevent infinite loop
    int maxTries = 100;
    int tries = 0;

    Vector2 RandomPosition;
    int randomX;
    int randomY;
    bool spaceFree = false;

    do {
      randomX = Random.Range((gridWidth/2) * -1, (gridWidth/2) + 1);
      randomY = Random.Range((gridHeight/2) * -1, (gridHeight/2) + 1);

      spaceFree = isSpaceFree( randomX, randomY );
    } while(!spaceFree && tries++ < maxTries);

    RandomPosition = new Vector2( randomX, randomY );

    return RandomPosition;
  }

  void LayoutObjectAtRandom( GameObject entity, int minimum, int maximum ) {
    int objectCount = Random.Range(minimum, maximum+1);

    for (int i = 0; i < objectCount; i++) {
      Vector2 randomPosition = RandomPosition();
      GameObject newObj = Instantiate(entity, randomPosition, Quaternion.identity);
      newObj.name = entity.name;
    }
  }

  void LayoutEntityAtRandom( GameObject entity, int minimum, int maximum, int maxSize, int cyclesMin, int cyclesMax ) {
    int objectCount = Random.Range(minimum, maximum+1);

    for (int i = 0; i < objectCount; i++) {
      Vector2 randomPosition = RandomPosition();
      Brain brain = new Brain();

      // brain.cyclesPerUpdateMax = 1;
      // brain.cyclesPerUpdateMin = 1;

      brain.cyclesPerUpdateMax = cyclesMin;
      brain.cyclesPerUpdateMin = cyclesMax;
      brain.setMaxSize(maxSize);

      GameObject newEntity = Instantiate(entity, randomPosition, Quaternion.identity);
      newEntity.name = entity.name;

      brain.AddEntity(newEntity);

      brains.Add(brain);
    }
  }

  bool isSpaceFree( int x, int y ) {
    Vector2 position = new Vector2( x, y );

    Collider2D[] collisions = Physics2D.OverlapBoxAll( position, new Vector2( 1, 1 ), 0f, blockingLayer );
      
    if (collisions.Length == 0) {
      return true;
    }

    return false;
  }

  void LayoutObjectAtPosition( GameObject entity, int x, int y ) {
    Vector2 position = new Vector2(x, y);
    GameObject newObject = Instantiate(entity, position, Quaternion.identity);
    newObject.name = entity.name;
  }

  public void SetupGame() {

    InitializeGrid();

    // LayoutObjectAtRandom(grass, minGrass, maxGrass);

  }

  // Use this for initialization
  void Start () {
    
  }
  
  // Update is called once per frame
  void Update () {
    
    List<Brain> newBrains = new List<Brain>();
    List<Brain> removeBrains = new List<Brain>();
    
    // run each brain
    foreach (Brain brain in brains) {
      
      if (brain.getCurrentSize() >= brain.getMaxSize()) {
        
        Brain newBrain = new Brain();
        newBrain.setMaxSize(brain.getMaxSize());
        newBrain.setCyclesMax(brain.cyclesPerUpdateMax);
        newBrain.setCyclesMin(brain.cyclesPerUpdateMin);
        newBrain.setCyclesPer(brain.cyclesPerUpdateMin);
        newBrain.setEntities(brain.Split());
        newBrains.Add(newBrain);
        continue;
        
      }
      
      if (brain.getCurrentSize() >= 1) {
        brain.Update();
      }
      else {
        removeBrains.Add(brain);
      }
    }
    
    foreach (Brain newBrain in newBrains) { brains.Add(newBrain); }
    foreach (Brain removeBrain in removeBrains) { brains.Remove(removeBrain); }

    totalCycles++;
  
    List<string> entityNames = new List<string>();
    List<int> entityCounts = new List<int>();
  
    if (cycleCount++ > debugRefresh) {
      cycleCount = 0;
      
      GameObject[] entities = GameObject.FindGameObjectsWithTag("Entity");
      foreach (GameObject entity in entities) {
        
        if (!entityNames.Contains(entity.name)) {
          entityNames.Add(entity.name);
        }
        int nameIndex = entityNames.IndexOf(entity.name);
        
        if (entityCounts.Count <= nameIndex) {
          entityCounts.Add(1);
        }
        else {
          entityCounts[nameIndex]++;
        }
        
      }
      
      debugText.text = "Game Cycles: " + totalCycles;
      
      for (int key = 0; key < entityNames.Count; ++key) {
        
        debugText.text += "\n" + entityNames[key] + ": " + entityCounts[key];
        
      }
      
      debugText.text += "\nBrains: " + brains.Count;
    }
  }
}

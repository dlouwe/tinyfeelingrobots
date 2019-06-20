using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

  public LayerMask blockingLayer;

  public GameObject water;
  public GameObject grass;
  public GameObject ground;

  public int gridWidth = 50;
  public int gridHeight = 50;
  public float grassChance = 0.15f;

  private int waterSeedsMin = 5;
  private int waterSeedsMax = 15;
  private int waterMax = 800;

  private Transform boardHolder;

  public List <Brain<Grass>> grassBrains = new List <Brain<Grass>> ();

  void InitializeGrid() {

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

    LayoutGrassAtRandom(grass, grassSeedsMin, grassSeedsMax);

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
      Instantiate(entity, randomPosition, Quaternion.identity);
    }
  }

  void LayoutGrassAtRandom( GameObject entity, int minimum, int maximum ) {
    int objectCount = Random.Range(minimum, maximum+1);

    for (int i = 0; i < objectCount; i++) {
      Vector2 randomPosition = RandomPosition();
      Brain<Grass> brain = new Brain<Grass>();

      brain.setMaxSize(20);
      brain.AddEntity(Instantiate(entity, randomPosition, Quaternion.identity));

      grassBrains.Add(brain);
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
    Instantiate(entity, position, Quaternion.identity);
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
    
    // run each brain
    foreach (Brain<Grass> grassBrain in grassBrains) {
      grassBrain.Update();
    }

  }
}

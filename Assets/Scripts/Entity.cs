using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Entity : MonoBehaviour {
  
  public float currentAge;
  public float maxAge;

  public LayerMask blockingLayer;
  public GameObject addToBrain = null;

  public float energy;
  public float maxEnergy;
  public float energyLoss;
  
  protected float maxWaterDistance = 75f;
  private float maxSearchDistance = 40f;
  private float maxPathNodes = 25f;
  private float defaultSearchDistance = 4f;

  protected GameObject pathfindTarget = null;
  protected float speed = 0f;

  protected Collider2D[] surroundings;

  protected BoxCollider2D boxCollider;
  protected Rigidbody2D rb2D;
  protected SpriteRenderer spriteRenderer;

  public PropertyRange hungerThreshold;
  public float hungerThresholdBase;
  public float hungerThresholdVariance;

  protected PropertyRange startEnergy;
  public float startEnergyBase;
  public float startEnergyVariance;
  protected PropertyRange energyPerAdjacentSame;
  public float energyPerAdjacentSameBase;
  public float energyPerAdjacentSameVariance;
  protected PropertyRange energyFromWater;
  public float energyFromWaterBase;
  public float energyFromWaterVariance;
  protected PropertyRange growDistance;
  public float growDistanceBase;
  public float growDistanceVariance;
  
  public float size;

  public float healthyRed;
  public float healthyGreen;
  public float healthyBlue;
  public float dyingRed;
  public float dyingGreen;
  public float dyingBlue;
  
  public float growthThreshold;
  public float growthChance;
  public float growthCost;
  
  private bool isAwake = false;

  // Use this for initialization
  void Awake () {

    //Get a component reference to this object's BoxCollider2D
    boxCollider = GetComponent <BoxCollider2D> ();

    //Get a component reference to this object's Rigidbody2D
    rb2D = GetComponent <Rigidbody2D> ();

    //Get a component reference to this object's SpriteRenderer
    spriteRenderer = GetComponent <SpriteRenderer> ();

    startEnergy = new PropertyRange(startEnergyBase, startEnergyVariance);
    energyFromWater = new PropertyRange(energyFromWaterBase, energyFromWaterVariance);
    energyPerAdjacentSame = new PropertyRange(energyPerAdjacentSameBase, energyPerAdjacentSameVariance);
    growDistance = new PropertyRange(growDistanceBase,growDistanceVariance);
    hungerThreshold = new PropertyRange(hungerThresholdBase,hungerThresholdVariance);

    // set starting energy
    energy = startEnergy.randVal;

    currentAge = 1;
    
    isAwake = true;

  }

  void Start() {}

  protected void MoveToTarget() {

    if (!pathfindTarget) { return; }
    
    List<Path> path = getBestPath();
    
    if (path != null && path.Count > 2) {
      transform.position = new Vector2(path[1].x, path[1].y);
    }
    if (path != null && path.Count == 2) {
      Eat(pathfindTarget);
    }
    
  }

  private List<Path> getBestPath() {
    
    List<Path> evaluationList = new List<Path>();
    List<Path> closedPathList = new List<Path>();

    Path destinationSquare = new Path(0, 0, null, (int) pathfindTarget.transform.position.x, (int) pathfindTarget.transform.position.y);
    evaluationList.Add(new Path(0, BlocksToTarget(transform.position, pathfindTarget.transform.position), null, (int) transform.position.x, (int) transform.position.y));
    Path currentSquare = null;

    while (evaluationList.Count > 0) {

      currentSquare = getPathWithLowestFScore(evaluationList);
      
      // hard-limit path nodes when no path can be found
      if (evaluationList.Count > maxPathNodes || closedPathList.Count > 500) { 
        
        // this may be a bad path; pick a new target
        pathfindTarget = null;
        
        return buildPath(currentSquare); 
      }

      closedPathList.Add(currentSquare);
      evaluationList.Remove(currentSquare);

      // have we found the destination?
      if (doesPathListContain( closedPathList, destinationSquare )) {
        return buildPath(currentSquare);
      }

      List<Path> adjacentSquares = GetAdjacentSquares(currentSquare);
      
      foreach (Path p in adjacentSquares) {

        if(doesPathListContain(closedPathList,p)) {
          continue; // skip this one, we already know about it
        }

        if (!doesPathListContain(evaluationList,p)) {
          evaluationList.Add(p);
        }
      }
      
    }

    return null;

  }

  private bool doesPathListContain( List<Path> haystack, Path needle ) {
    
    foreach (Path p in haystack) {
      if (p.x == needle.x && p.y == needle.y) {
        return true;
      }
    }
    
    return false;
  }

  // reverse path
  private List<Path> buildPath(Path p) {
    List<Path> bestPath = new List<Path> ();
    Path currentLoc = p;
    bestPath.Insert (0,currentLoc);
    while (currentLoc.parent != null) {
        currentLoc = currentLoc.parent;
        bestPath.Insert (0, currentLoc);
    }
    return bestPath;
  }
  
  // Update is called once per frame
  // void Update () {
    
  // }

  public void _Update() {
    
    if (!isAwake) { return; }
    
    currentAge++;
    energy -= energyLoss;
    surroundings = CheckSurroundings();
    
    if (energy <= 1) {
      gameObject.SetActive(false);
      return;
    }
    
    float currentHungerThreshold = hungerThreshold.randVal;
    
    if (currentHungerThreshold >= 0 && energy < currentHungerThreshold) {
      
      if (!pathfindTarget || !pathfindTarget.activeInHierarchy) {
        pathfindTarget = GetMeal();
      }
    
      if (pathfindTarget != null) {
        MoveToTarget();
      }
    }
    
    Feed();
    addToBrain = AttemptGrow();

    if (energy > maxEnergy) {
      energy = maxEnergy;
    }

    // update colour
    float healthRatio = energy / maxEnergy;

    float redRange = healthyRed - dyingRed;
    float greenRange = healthyGreen - dyingGreen;
    float blueRange = healthyBlue - dyingBlue;
    
    float redDiff = redRange * healthRatio;
    float greenDiff = greenRange * healthRatio;
    float blueDiff = blueRange * healthRatio;

    float newRed = (dyingRed + redDiff) / 255;
    float newGreen = (dyingGreen + greenDiff) / 255;
    float newBlue = (dyingBlue + blueDiff) / 255;
    
    spriteRenderer.color = new Color(newRed, newGreen, newBlue, 1f);

  }

  void Feed() {
    float totalNewEnergy = 0;
    float energyFromWaterVal = energyFromWater.randVal;
    float energyFromSurroundingSameVal = energyPerAdjacentSame.randVal;
    
    if (energyFromWaterVal > 0) {
      float distanceToWater = Mathf.Min(maxWaterDistance, DistanceToWater());
      float energyFromWaterTotal = energyFromWaterVal * ((maxWaterDistance - distanceToWater) / maxWaterDistance);
      
      totalNewEnergy += energyFromWaterTotal;
    }

    if (energyFromSurroundingSameVal != 0) {
      // determine same surrounding object
      int surroundingSame = 0;
      foreach (Collider2D curSurrounding in surroundings) {
        if (curSurrounding.name == gameObject.name) {
          surroundingSame++;
        }
      }

      totalNewEnergy += energyFromSurroundingSameVal * surroundingSame;

    }

    // linearly reduce energy gain as age reaches max
    totalNewEnergy *= ((maxAge - currentAge) / maxAge);
    
    changeEnergy(totalNewEnergy);

  }
  
  void Eat(GameObject meal) {
    
    Entity targetEntity = pathfindTarget.GetComponent<Entity>();
    float targetEnergy = targetEntity.energy;
    
    float totalNewEnergy = targetEnergy * ((maxAge - currentAge) / maxAge);
    
    this.energy += totalNewEnergy;
    targetEntity.Die();
    
  }

  public GameObject AttemptGrow () {

    // do we meet conditions to grow?
    if (energy > growthThreshold && surroundings != null && surroundings.Length < 8 && Random.Range(0f,1f) <= growthChance) {

      GameObject newObj = Grow();
      if (newObj != null) {
        energy -= growthCost;
        return newObj;
      }

    }
    return null;
  }

  GameObject Grow() {

    int growDistanceVal = (int) growDistance.randVal;
  
    // pick a random direction
    int randX = Random.Range(growDistanceVal*-1, growDistanceVal+1);
    int randY = Random.Range(growDistanceVal*-1, growDistanceVal+1);

    // skip if we pick center
    if (randX != 0 || randY != 0) {
      Vector2 randPosition = (Vector2) transform.position + new Vector2( randX, randY );
      
      // check if there's anything there
      Collider2D[] collisions = Physics2D.OverlapBoxAll( randPosition, new Vector2( 1, 1 ), 0f, blockingLayer );
    
      if (collisions.Length == 0) {
        GameObject newObj = Instantiate(gameObject, randPosition, Quaternion.identity);
        newObj.name = gameObject.name;
        return newObj;
      }
    }

    return null;

  }

  protected Collider2D[] CheckSurroundings() {

    if (!boxCollider) {return null;}

    boxCollider.enabled = false;
    Collider2D[] collisions = Physics2D.OverlapBoxAll( transform.position, new Vector2(3, 3), 0f, blockingLayer );
    boxCollider.enabled = true;

    return collisions;

  }

  public void changeEnergy( float energyChange ) {
    energy += energyChange;
  }
  
  protected GameObject GetMeal() {
    return GetMeal(defaultSearchDistance);
  }
  
  protected GameObject GetMeal(float searchDistance) {
    
    float minDistance = maxSearchDistance * maxSearchDistance;
    // GameObject closestObject = null;
    Vector3 currentPosition = transform.position;
    List<Collider2D> filteredCollisions = new List<Collider2D>();

    boxCollider.enabled = false;
    Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, searchDistance, blockingLayer);
    boxCollider.enabled = true;

    foreach (Collider2D currentCollider in collisions) {
      if (currentCollider.gameObject.tag == "Entity" && currentCollider.gameObject.GetComponent<Entity>().size < size) {
        filteredCollisions.Add(currentCollider);
      }
    }

    // if we had no collisions, expand search range
    if (filteredCollisions.Count == 0) {
      if (searchDistance < maxSearchDistance) { return GetMeal(maxSearchDistance); } 
      else                                    { return null; }
    }
    
    // pick random meal in range
    int randMeal = Random.Range(0, filteredCollisions.Count);

    return filteredCollisions[randMeal].gameObject;
    
  }

  protected GameObject GetClosestByName(string targetName, float searchDistance) {

    float minDistance = maxSearchDistance * maxSearchDistance;
    GameObject closestObject = null;
    Vector3 currentPosition = transform.position;
    List<Collider2D> filteredCollisions = new List<Collider2D>();

    boxCollider.enabled = false;
    Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, searchDistance, blockingLayer);
    boxCollider.enabled = true;

    foreach (Collider2D currentCollider in collisions) {
      if (currentCollider.name == targetName) {
        filteredCollisions.Add(currentCollider);
      }
    }

    // if we had no collisions, expand search range
    if (filteredCollisions.Count == 0) {
      if (searchDistance < maxSearchDistance) { return GetClosestByName(targetName, searchDistance + 2f); } 
      else                                    { return null; }
    }

    foreach (Collider2D currentCollider in filteredCollisions) {

      Vector3 directionToTarget = currentCollider.transform.position - currentPosition;
      float dSqrToTarget = directionToTarget.sqrMagnitude;

      if(dSqrToTarget < minDistance) {
        minDistance = dSqrToTarget;
        closestObject = currentCollider.gameObject;
      }
    }

    return closestObject;
  }

  protected float DistanceByName(string targetName, float searchDistance) {
    GameObject closestObject = GetClosestByName(targetName, searchDistance);

    if (closestObject != null) {
      Vector3 currentPosition = transform.position;
      Vector3 directionToTarget = closestObject.transform.position - currentPosition;
      return directionToTarget.sqrMagnitude;
    } else {
      return maxSearchDistance;
    }
  }

  // default distance
  protected float DistanceByName(string targetName) {
    return DistanceByName(targetName, defaultSearchDistance);
  }

  private List<Path> GetAdjacentSquares(Path p) {
    List<Path> ret = new List<Path> ();
    int _x = p.x;
    int _y = p.y;

    for (int x = -1; x <= 1; x++) {
      for (int y = -1; y <= 1; y++) {
        int __x = _x + x; // easier than writing (_x + x) 5 times
        int __y = _y + y; // easier than writing (_y + y) 5 times

        // skip self
        if (x == 0 && y == 0) {
          continue;
        }
        else if (!CheckForPathCollision(new Vector2(_x,_y),new Vector2(__x,__y))) {
          ret.Add(new Path(p.g+1, BlocksToTarget(new Vector2(__x,__y), pathfindTarget.transform.position), p, __x, __y));
        }
      }
    }

    return ret;
  }

  Path getPathWithLowestFScore(List<Path> pathList) {
    if (pathList.Count <= 0) { return null; }

    Path returnPath = pathList[0];

    foreach (Path p in pathList) {
      if (p.f < returnPath.f) {
        returnPath = p;
      }
    }

    return returnPath;
  }

  int BlocksToTarget(Vector2 tileLoc, Vector2 targetLoc)
  {
      int final = (int)Mathf.Abs((tileLoc.x - targetLoc.x) * (tileLoc.x - targetLoc.x) + (tileLoc.y - targetLoc.y) * (tileLoc.y - targetLoc.y));
      return final;
  }
  
  private bool CheckForPathCollision(Vector2 start, Vector2 end) {
    this.GetComponent<BoxCollider2D>().enabled = false;
    RaycastHit2D hit = Physics2D.Linecast (start, end, blockingLayer);
    this.GetComponent<BoxCollider2D>().enabled = true;
    // trying to walk into a wall, change direction
    if (hit.transform != null)
    {

      // check to see if collision is our destination
      if (pathfindTarget != null) {
        if (hit.transform.position.x == pathfindTarget.transform.position.x
        &&  hit.transform.position.y == pathfindTarget.transform.position.y) {
          // don't consider collision
          return false;
        }
      }
      
      if (hit.transform.gameObject.tag == "Water") {
        return true;
      }
    }
    return false;
  }

  private void Die() {
    gameObject.SetActive(false);
  }
  
  public struct PropertyRange {

    public float min;
    public float max;

    public PropertyRange(float _base, float _var) {
      min = _base - _var;
      max = _base + _var;
    }

    public float randVal {
      get {
        return Random.Range(min, max);
      }
    }

  }
  
  float DistanceToWater() {
    return DistanceByName("Water", 2f);
  }
}

class Path:object
{
    public int g;         // Steps from A to this
    public int h;         // Steps from this to B
    public Path parent;   // Parent node in the path
    public int x;         // x coordinate
    public int y;         // y coordinate
    public Path (int _g, int _h, Path _parent, int _x, int _y)
    {
        g = _g;
        h = _h;
        parent = _parent;
        x = _x;
        y = _y;
    }
    public int f // Total score for this
    {
        get 
        {
            return g+h; 
        }
    }
}

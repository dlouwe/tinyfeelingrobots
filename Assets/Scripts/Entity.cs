using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {

  public LayerMask blockingLayer;
  public GameObject addToBrain = null;

  protected float energy;
  protected float minStartEnergy = 40f;
  protected float maxStartEnergy = 60f;
  protected float maxEnergy = 80f;
  protected float energyLoss = .35f;

  protected GameObject pathfindTarget = null;
  protected float speed = 0f;

  protected Collider2D[] surroundings;

  protected BoxCollider2D boxCollider;
  protected Rigidbody2D rb2D;
  protected SpriteRenderer spriteRenderer;

  private float maxSearchDistance = 300f;
  private float defaultSearchDistance = 2f;

  // Use this for initialization
  void Awake () {

    //Get a component reference to this object's BoxCollider2D
    boxCollider = GetComponent <BoxCollider2D> ();

    //Get a component reference to this object's Rigidbody2D
    rb2D = GetComponent <Rigidbody2D> ();

    //Get a component reference to this object's SpriteRenderer
    spriteRenderer = GetComponent <SpriteRenderer> ();

  }

  protected void MoveToTarget() {

    if (!pathfindTarget) { return; }
    
    List<Path> path = getBestPath();
    
    if (path != null && path.Count > 1) {
      transform.position = new Vector2(path[1].x, path[1].y);
    }
    if (path != null && path.Count == 1) {
      pathfindTarget.SetActive(false);
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

      // Debug.Log(currentSquare.x + " " + currentSquare.y);

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

  public abstract GameObject AttemptGrow();
  public abstract void _Update();

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

  protected GameObject GetClosestTag(string targetTag, float searchDistance, bool tagChecked) {

    // check to make sure target tag exists anywhere
    // ** POOR PERFORMANCE - Avoid tagChecked = false when possible ** //
    // perhaps cache checked tags for X cycles?
    if (!tagChecked) {
      GameObject[] gos = GameObject.FindGameObjectsWithTag(targetTag);
      if (gos.Length == 0) {
        return null;
      }
    }

    float minDistance = maxSearchDistance * maxSearchDistance;
    GameObject closestObject = null;
    Vector3 currentPosition = transform.position;
    List<Collider2D> filteredCollisions = new List<Collider2D>();

    boxCollider.enabled = false;
    Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, searchDistance, blockingLayer);
    boxCollider.enabled = true;

    foreach (Collider2D currentCollider in collisions) {
      if (currentCollider.tag == targetTag) {
        filteredCollisions.Add(currentCollider);
      }
    }

    // if we had no collisions, expand search range
    if (filteredCollisions.Count == 0) {
      return GetClosestTag(targetTag, searchDistance + 2f, true);
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

  protected float DistanceToTag(string targetTag, float searchDistance, bool tagChecked) {
    GameObject closestObject = GetClosestTag(targetTag, searchDistance, false);

    if (closestObject != null) {
      Vector3 currentPosition = transform.position;
      Vector3 directionToTarget = closestObject.transform.position - currentPosition;
      return directionToTarget.sqrMagnitude;
    } else {
      return maxSearchDistance;
    }
  }

  // default to tag not checked
  protected float DistanceToTag(string targetTag, float searchDistance) {
    return DistanceToTag(targetTag, searchDistance, false);
  }

  // default distance
  protected float DistanceToTag(string targetTag) {
    return DistanceToTag(targetTag, defaultSearchDistance);
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
        else if (!CheckForCollision(new Vector2(_x,_y),new Vector2(__x,__y))) {
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
  
  private bool CheckForCollision(Vector2 start, Vector2 end) {
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

      return true;
    }
    return false;
  }

  private void Die() {
    gameObject.SetActive(false);
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
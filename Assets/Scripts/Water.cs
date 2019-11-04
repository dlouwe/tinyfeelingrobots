using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Entity {

  private float defaultColourR = 0f;
  private float defaultColourG = 0f;
  private float defaultColourB = 255f;

  public void checkDepth() {

    surroundings = CheckSurroundings();

    int surroundingWater = 0;

    foreach (Collider2D currentCollider in surroundings) {
      if (currentCollider.tag == "Water") {
        surroundingWater++;
      }
    }

    float depth = (4 - surroundingWater)*-1;

    float newR = defaultColourR;
    float newG = defaultColourG;
    float newB = defaultColourB;

    if (depth > 0) {
      newB -= depth * 32f;
    }

    if (depth < 0) {
      newG += (depth*-1) * 16f;
    }

    spriteRenderer.color = new Color(newR/255, newG/255, newB/255, 1f);

  }
  
  public override GameObject AttemptGrow() {
    
    // pick a random direction
    int randX = Random.Range(-1, 2);
    int randY = Random.Range(-1, 2);

    // skip if not cardinal
    if (randX != 0 && randY != 0) {
      return null;
    }

    // skip if we pick center
    if (randX != 0 || randY != 0) {
      Vector2 randPosition = (Vector2) transform.position + new Vector2( randX, randY );
      Collider2D[] collisions = Physics2D.OverlapBoxAll( randPosition, new Vector2( 1, 1 ), 0f, blockingLayer );
  
      if (collisions.Length == 0) {
        return Instantiate(gameObject, randPosition, Quaternion.identity);
      }
    }

    return null;
    
  }

  public override void _Update() {}

  // Use this for initialization
  // void Start () {
    
  // }
  
  // Update is called once per frame
  // void Update () {
    
  // }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herbivore : Entity {
  
  public override GameObject AttemptGrow() {

    return null;
    
  }

  public override void _Update() {

    pathfindTarget = GetClosestTag("Grass", 6f, false);

    if (pathfindTarget != null) {
      MoveToTarget();
    }

  }

  // Use this for initialization
  // void Start () {
    
  // }
  
  // Update is called once per frame
  // void Update () {
    
  // }
}

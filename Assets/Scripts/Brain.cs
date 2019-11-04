using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain<TEntity> where TEntity : Entity {

  private int maxSize = 0;
  public int cyclesPerUpdate;
  public int cyclesPerUpdateMax = 7;
  public int cyclesPerUpdateMin = 4;
  // public int cyclesPerUpdateMax = 1;
  // public int cyclesPerUpdateMin = 1;

  private List <GameObject> entities = new List <GameObject> ();

  public int getMaxSize() {
    return maxSize;
  }

  public void setMaxSize( int maxSize ) {
    this.maxSize = maxSize;
  }

  void Awake () {
    
    // reset cycles per update
    resetCycles();

  }

  public void Update () {
    
    // wait for semi-random number of cycles
    if (entities.Count < 1  || cyclesPerUpdate-- > 0) {
      return;
    }
    else {
      resetCycles();
    }

    GameObject randEntity = GetRandomEntity();

    if (!randEntity.activeSelf) {
      RemoveEntity( randEntity );
    }
    
    TEntity entityScript = randEntity.GetComponent<TEntity>();
    entityScript._Update();

    if(entityScript.addToBrain != null) {

      // on max size, split to new brain
      if (entities.Count < maxSize) {
        AddEntity(entityScript.addToBrain);
      }
    }

  }

  bool AttemptGrow() {
    if (maxSize > 0 && entities.Count > 0 && entities.Count < maxSize) {
      GameObject randEntity = GetRandomEntity();
      return randEntity.GetComponent<TEntity>().AttemptGrow();
    }

    return false;
  }

  public void AddEntity( GameObject entity ) {
    if (maxSize == 0 || (maxSize > 0 && entities.Count < maxSize)) {
      entities.Add( entity );
    }
  }

  void RemoveEntity( GameObject entity ) {
    entities.Remove( entity );
  }
  
  void resetCycles() {
    cyclesPerUpdate = Random.Range(cyclesPerUpdateMin, cyclesPerUpdateMax+1);
  }

  GameObject GetRandomEntity() {
    int randIndex = Random.Range(0, entities.Count);
    return entities[randIndex];
  }
}

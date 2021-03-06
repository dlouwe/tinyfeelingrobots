﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain {

  private int maxSize = 0;
  public int cyclesPerUpdate;
  public int cyclesSinceUpdate = 0;
  public int cyclesPerUpdateMax = 20;
  public int cyclesPerUpdateMin = 10;

  private List <GameObject> entities = new List <GameObject> ();

  public int getMaxSize() {
    return maxSize;
  }
  
  public int getCurrentSize() {
    return entities.Count;
  }
  
  public void setEntities(List <GameObject> newEntities) {
    entities = newEntities;
  }

  public void setMaxSize( int maxSize ) {
    this.maxSize = maxSize;
  }
  
  public void setCyclesMax(int cycles) { cyclesPerUpdateMax = cycles; }
  public void setCyclesMin(int cycles) { cyclesPerUpdateMin = cycles; }
  public void setCyclesPer(int cycles) { cyclesPerUpdate = cycles; }

  void Awake () {
    
    // reset cycles per update
    resetCycles();

  }

  public void Update () {
    
    // wait for semi-random number of cycles
    if (entities.Count < 1 || cyclesPerUpdate > cyclesSinceUpdate) {
      cyclesSinceUpdate++;
      return;
    }
    else {
      resetCycles();
    }
    

    
    GameObject randEntity = GetRandomEntity();

    if (!randEntity.activeSelf) {
      RemoveEntity( randEntity );
    }
    
    Entity entityScript = randEntity.GetComponent<Entity>();
    entityScript._Update();

    if(entityScript.addToBrain != null) {

      if (entities.Count < maxSize) {
        AddEntity(entityScript.addToBrain);
      }
    }

  }

  bool AttemptGrow() {
    if (maxSize > 0 && entities.Count > 0 && entities.Count < maxSize) {
      GameObject randEntity = GetRandomEntity();
      return randEntity.GetComponent<Entity>().AttemptGrow();
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
    cyclesSinceUpdate = 0;
    setCyclesPer(Random.Range(cyclesPerUpdateMin, cyclesPerUpdateMax+1));
  }

  GameObject GetRandomEntity() {
    int randIndex = Random.Range(0, entities.Count);
    return entities[randIndex];
  }
  
  public List <GameObject> Split() {
    
    int length = entities.Count;
    float newLength = length / 2;
    
    List <GameObject> listOne = new List <GameObject> ();
    List <GameObject> listTwo = new List <GameObject> ();
    
    for (int x = 0; x < length; x++) {
      
      if (x < newLength) {
        listOne.Add(entities[x]);
      }
      else {
        listTwo.Add(entities[x]);
      }
      
    }
    
    setEntities(listOne);
    
    return listTwo;
    
  }
  
}

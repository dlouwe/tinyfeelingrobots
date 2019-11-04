using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grass : Entity {

  public float maxAge = 5000f;
  public float currentAge;

  private float maxWaterDistance = 100f;
  private float energyFromWaterMin;
  private float energyFromWaterMax;

  // private float energyPerEmptySquareMin = 0.04f;
  // private float energyPerEmptySquareMax = 0.065f;
  private float energyPerAdjacentGrassMin = -0.09f;
  private float energyPerAdjacentGrassMax = -0.105f;

  private float growthThreshold = 60f;
  private float growthChance = .7f;
  private float growthCost = 40f;
  private int growDistanceMax = 2;
  private int growDistanceMin = 1;

  private float healthyRed = 0f;
  private float healthyGreen = 75f;
  private float healthyBlue = 6f;
  private float dyingRed = 125f;
  private float dyingGreen = 190f;
  private float dyingBlue = 0f;

  // Use this for initialization
  void Start () {

    currentAge = 1f;

    energyFromWaterMin = 2f;
    energyFromWaterMax = 2.5f;

    // generate starting energy
    energy = Random.Range(minStartEnergy, maxStartEnergy);

  }

  // Update is called once per frame
  public override void _Update () {

    currentAge++;
    energy -= energyLoss;
    surroundings = CheckSurroundings();

    if (energy <= 1) {
      gameObject.SetActive(false);
      return;
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
    // var temp = Time.realtimeSinceStartup;
    float distanceToWater = Mathf.Min(maxWaterDistance, DistanceToWater());
    // Debug.Log( Time.realtimeSinceStartup - temp );
    float energyFromWaterRatio = Random.Range(energyFromWaterMin, energyFromWaterMax);
    float energyFromWater = energyFromWaterRatio * ((maxWaterDistance - distanceToWater) / maxWaterDistance);

    float energyPerAdjacentGrass = Random.Range(energyPerAdjacentGrassMin, energyPerAdjacentGrassMax);

    totalNewEnergy += energyFromWater;
    // energy += emptySquares * energyPerEmptySquare;
    totalNewEnergy += energyPerAdjacentGrass * surroundings.Length;

    // linearly reduce energy gain as age reaches max
    totalNewEnergy *= ((maxAge - currentAge) / maxAge);

    changeEnergy(totalNewEnergy);

  }

  public override GameObject AttemptGrow () {

    // do we meet conditions to grow?
    if (energy > growthThreshold && surroundings.Length < 8 && Random.Range(0f,1f) <= growthChance) {

      GameObject newGrass = Grow();
      if (newGrass != null) {
        energy -= growthCost;
        return newGrass;
      }

    }
    return null;
  }

  GameObject Grow() {

    int growDistance = Random.Range(growDistanceMin, growDistanceMax+1);
  
    // pick a random direction
    int randX = Random.Range(growDistance*-1, growDistance+1);
    int randY = Random.Range(growDistance*-1, growDistance+1);

    // skip if we pick center
    if (randX != 0 || randY != 0) {
      Vector2 randPosition = (Vector2) transform.position + new Vector2( randX, randY );
      
      // check if there's anything there
      Collider2D[] collisions = Physics2D.OverlapBoxAll( randPosition, new Vector2( 1, 1 ), 0f, blockingLayer );
    
      if (collisions.Length == 0) {
        return Instantiate(gameObject, randPosition, Quaternion.identity);
      }
    }

    return null;

  }

  float DistanceToWater() {
    return DistanceToTag("Water", 2f, true); // manually set tagChecked true for better performance
  }
  
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Random = UnityEngine.Random;

// public class Grass : Entity {

//   // Use this for initialization
//   void Start () {

//     // set properties
//     maxAge = 5000f;
//     currentAge = 1f;
//     maxEnergy = 80f;
//     energyLoss = .35f;

//     healthyRed = 0f;
//     healthyGreen = 75f;
//     healthyBlue = 6f;
//     dyingRed = 125f;
//     dyingGreen = 190f;
//     dyingBlue = 0f;

//     growthThreshold = 60f;
//     growthChance = .7f;
//     growthCost = 40f;
    
//     // set up property ranges
//     startEnergy = new PropertyRange(40f, 60f);
//     energyFromWater = new PropertyRange(2f, 2.5f);
//     energyPerAdjacentSame = new PropertyRange(-0.09f, -0.105f);
//     growDistance = new PropertyRange(1f,3f);

//     // set starting energy
//     energy = startEnergy.randVal;

//   }

//   // Update is called once per frame
//   public override void _Update () {

//     currentAge++;
//     energy -= energyLoss;
//     surroundings = CheckSurroundings();

//     if (energy <= 1) {
//       gameObject.SetActive(false);
//       return;
//     }

//     Feed();
//     addToBrain = AttemptGrow();

//     if (energy > maxEnergy) {
//       energy = maxEnergy;
//     }

//     // update colour
//     float healthRatio = energy / maxEnergy;

//     float redRange = healthyRed - dyingRed;
//     float greenRange = healthyGreen - dyingGreen;
//     float blueRange = healthyBlue - dyingBlue;
    
//     float redDiff = redRange * healthRatio;
//     float greenDiff = greenRange * healthRatio;
//     float blueDiff = blueRange * healthRatio;

//     float newRed = (dyingRed + redDiff) / 255;
//     float newGreen = (dyingGreen + greenDiff) / 255;
//     float newBlue = (dyingBlue + blueDiff) / 255;
    
//     spriteRenderer.color = new Color(newRed, newGreen, newBlue, 1f);
//   }

//   void Feed() {

//     float totalNewEnergy = 0;
//     float distanceToWater = Mathf.Min(maxWaterDistance, DistanceToWater());
//     float energyFromWaterTotal = energyFromWater.randVal * ((maxWaterDistance - distanceToWater) / maxWaterDistance);

//     // determine surrounding grass
//     int surroundingSame = 0;
//     foreach (Collider2D curSurrounding in surroundings) {
//       if (curSurrounding.name == gameObject.name) {
//         surroundingSame++;
//       }
//     }

//     totalNewEnergy += energyFromWaterTotal;
//     totalNewEnergy += energyPerAdjacentSame.randVal * surroundingSame;

//     // linearly reduce energy gain as age reaches max
//     totalNewEnergy *= ((maxAge - currentAge) / maxAge);

//     changeEnergy(totalNewEnergy);

//   }

//   public override GameObject AttemptGrow () {

//     // do we meet conditions to grow?
//     if (energy > growthThreshold && surroundings.Length < 8 && Random.Range(0f,1f) <= growthChance) {

//       GameObject newGrass = Grow();
//       if (newGrass != null) {
//         energy -= growthCost;
//         return newGrass;
//       }

//     }
//     return null;
//   }

//   GameObject Grow() {

//     int growDistanceVal = (int) growDistance.randVal;
  
//     // pick a random direction
//     int randX = Random.Range(growDistanceVal*-1, growDistanceVal+1);
//     int randY = Random.Range(growDistanceVal*-1, growDistanceVal+1);

//     // skip if we pick center
//     if (randX != 0 || randY != 0) {
//       Vector2 randPosition = (Vector2) transform.position + new Vector2( randX, randY );
      
//       // check if there's anything there
//       Collider2D[] collisions = Physics2D.OverlapBoxAll( randPosition, new Vector2( 1, 1 ), 0f, blockingLayer );
    
//       if (collisions.Length == 0) {
//         GameObject newGrass = Instantiate(gameObject, randPosition, Quaternion.identity);
//         newGrass.name = "Grass";
//         return newGrass;
//       }
//     }

//     return null;

//   }

//   float DistanceToWater() {
//     return DistanceToTag("Water", 2f);
//   }
  
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {

  public LayerMask blockingLayer;
  public GameObject addToBrain = null;

  protected Collider2D[] surroundings;

  protected BoxCollider2D boxCollider;
  protected Rigidbody2D rb2D;
  protected SpriteRenderer spriteRenderer;

  // Use this for initialization
  void Awake () {

    //Get a component reference to this object's BoxCollider2D
    boxCollider = GetComponent <BoxCollider2D> ();

    //Get a component reference to this object's Rigidbody2D
    rb2D = GetComponent <Rigidbody2D> ();

    //Get a component reference to this object's SpriteRenderer
    spriteRenderer = GetComponent <SpriteRenderer> ();

  }
  
  // Update is called once per frame
  // void Update () {
    
  // }

  public abstract GameObject AttemptGrow();
  public abstract void _Update();

  protected Collider2D[] CheckSurroundings() {

    boxCollider.enabled = false;
    Collider2D[] collisions = Physics2D.OverlapBoxAll( transform.position, new Vector2(3, 3), 0f, blockingLayer );
    boxCollider.enabled = true;

    return collisions;

  }
}

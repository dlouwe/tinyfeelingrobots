using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

  public static GameManager instance = null;
  private BoardManager boardScript;

  void Awake () {
    
    if (instance == null) {
      instance = this;
    }
    else {
      Destroy(gameObject);
    }

    DontDestroyOnLoad(gameObject);
    boardScript = GetComponent<BoardManager>();

    InitGame();

  }

  void InitGame() {
    boardScript.SetupGame();
  }
  
  // Update is called once per frame
  void Update () {
    
  }
}

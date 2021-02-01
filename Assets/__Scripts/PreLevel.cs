using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreLevel : MonoBehaviour
{
    public Text levelDisplay;
    public Text asteroidsDisplay;
    public Text childrenDisplay;

    public void Start()
    {
        InitializePreLevel();
    }

    void Update()
    {
        if (GameManager.GAME_STATE == GameManager.eGameState.preLevel)
            InitializePreLevel();
    }

    public void InitializePreLevel()
    {
        levelDisplay.text = "LEVEL " + (GameManager.curLevel + 1);
        asteroidsDisplay.text = "Asteroids: " + (GameManager.levelsConfiguration[GameManager.curLevel][2] - '0');
        childrenDisplay.text = "Children: " + (GameManager.levelsConfiguration[GameManager.curLevel][4] - '0');
        StartCoroutine(GameManager.S.NextLevel());
    }
}

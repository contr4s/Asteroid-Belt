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
        if (AsteraX.GAME_STATE == AsteraX.eGameState.preLevel)
            InitializePreLevel();
    }

    public void InitializePreLevel()
    {
        levelDisplay.text = "LEVEL " + (AsteraX.S.curLevel + 1);
        asteroidsDisplay.text = "Asteroids: " + (AsteraX.S.levelsConfiguration[AsteraX.S.curLevel][2] - '0');
        childrenDisplay.text = "Children: " + (AsteraX.S.levelsConfiguration[AsteraX.S.curLevel][4] - '0');
        StartCoroutine(AsteraX.S.NextLevel());
    }
}

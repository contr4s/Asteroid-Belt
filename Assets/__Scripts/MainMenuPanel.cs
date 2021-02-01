using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : MonoBehaviour
{
    public void PlayGame()
    {
        AsteraX.GAME_STATE = AsteraX.eGameState.preLevel;
    }

    public void DeleteSaveFile()
    {
        SaveGameManager.DeleteSave();
    }

}

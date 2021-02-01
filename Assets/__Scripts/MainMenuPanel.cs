using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager.GAME_STATE = GameManager.eGameState.preLevel;
    }

    public void DeleteSaveFile()
    {
        SaveGameManager.DeleteSave();
    }

}

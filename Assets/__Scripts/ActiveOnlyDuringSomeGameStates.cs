using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnlyDuringSomeGameStates : MonoBehaviour {

    public enum ePauseEffect
    {
        ignorePause,
        activeWhenPaused,
        activeWhenNotPaused
    }

    // eGameStates is a System.Flags enum, so many values can be stored in a single field.
    [EnumFlags] // This uses the EnumFlagsAttribute from EnumFlagsAttributePropertyDrawer
    public GameManager.eGameState   activeStates = GameManager.eGameState.all;
    public bool editorOnly = false;
    public ePauseEffect pauseEffect = ePauseEffect.ignorePause;

    // Use this for initialization
    public void Start () {
        // Register this callback with the static public delegates on AsteraX.
        GameManager.GAME_STATE_CHANGE_DELEGATE += DetermineActive;
        GameManager.PAUSED_CHANGE_DELEGATE += DetermineActive;

        // Also make sure to set self based on the current state when awakened
        DetermineActive();
	}

    protected void OnDestroy()
    {
        // Unregister this callback from the static public delegates on AsteraX.
        GameManager.GAME_STATE_CHANGE_DELEGATE -= DetermineActive;
        GameManager.PAUSED_CHANGE_DELEGATE -= DetermineActive;

    }


    protected virtual void DetermineActive()
    {
        // This line uses a bitwise AND (&) to compare each bit of activeStates and newState.
        // If the result is the same as newState, then the bit for that newState must also be
        //  true in activeStates, meaning that newState is one of the states where this
        //  GameObject should be active.
        bool shouldBeActive = (activeStates & GameManager.GAME_STATE) == GameManager.GAME_STATE;

        if (editorOnly && !Application.isEditor)
        {
            shouldBeActive = false;
        }

        switch (pauseEffect)
        {
            case ePauseEffect.activeWhenNotPaused:
                shouldBeActive = !GameManager.isPaused;
                break;
            case ePauseEffect.activeWhenPaused:
                shouldBeActive = GameManager.isPaused;
                break;
        }

        gameObject.SetActive(shouldBeActive);
    }
    
}

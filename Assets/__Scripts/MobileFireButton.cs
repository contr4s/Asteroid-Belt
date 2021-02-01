using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileFireButton : MonoBehaviour
{
    bool registeredWithPauseChanged = false;

    Image img;

    // Use this for initialization
    void Start()
    {
        img = GetComponent<Image>();
        // Initially disable the image 
        img.raycastTarget = false;

        if (Application.isMobilePlatform)
        {
            RegisterWithPauseChanged();
            PauseChangedCallback();
        }
        else
        {
            // If this is the editor, check every second for Unity Remote 5
#if UNITY_EDITOR
            StartCoroutine(CheckForUnityRemote(1));
#else
            // If this is not a mobile platform & not in editor, disable this button
            gameObject.SetActive(false);
#endif
        }
    }


    IEnumerator CheckForUnityRemote(float delay)
    {
        while (!registeredWithPauseChanged) {
            if (UnityEditor.EditorApplication.isRemoteConnected)
            {
                Debug.Log("MobileFireButton:CheckForUnityRemote() – Connected to Unity Remote!");
                RegisterWithPauseChanged();
                PauseChangedCallback();
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }


    void RegisterWithPauseChanged()
    {
        if (registeredWithPauseChanged) return;

        // Remove any previous registration
        GameManager.PAUSED_CHANGE_DELEGATE -= PauseChangedCallback;
        // Register
        GameManager.PAUSED_CHANGE_DELEGATE += PauseChangedCallback;

        registeredWithPauseChanged = true;
    }


    // Update is called once per frame
    void PauseChangedCallback()
    {
        // Enabling or disabling raycastTarget traps or ignores taps
        img.raycastTarget = !GameManager.isPaused;
    }
}

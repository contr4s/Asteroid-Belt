//#define DEBUG_AsteraX_LogMethods

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Private Singleton-style instance. Accessed by static property S later in script
    static private GameManager _S;

    [SerializeField]
    static List<Asteroid> ASTEROIDS;
    static List<Bullet> BULLETS;
    static private eGameState _GAME_STATE = eGameState.mainMenu;

    const float MIN_ASTEROID_DIST_FROM_PLAYER_SHIP = 5; //used for spawning asteroids

    public delegate void CallbackDelegate(); // Set up a generic delegate type.
    static public CallbackDelegate GAME_STATE_CHANGE_DELEGATE;
    static public CallbackDelegate PAUSED_CHANGE_DELEGATE;

    static public CallbackDelegate HIGH_SCORE_DELEGATE;
    static public CallbackDelegate HIGH_LEVEL_DELEGATE;

    static public int jumps = 3;
    static public bool is_jumping = false;

    static public int score = 0;
    static public bool getHighScore = false;

    static public int curLevel = 0;

    static public string[] levelsConfiguration;

    static public bool isPaused = false;
    // System.Flags changes how eGameStates are viewed in the Inspector and lets multiple 
    //  values be selected simultaneously (similar to how Physics Layers are selected).
    // It's only valid for the game to ever be in one state, but I've added System.Flags
    //  here to demonstrate it and to make the ActiveOnlyDuringSomeGameStates script easier
    //  to view and modify in the Inspector.
    // When you use System.Flags, you still need to set each enum value so that it aligns 
    //  with a power of 2. You can also define enums that combine two or more values,
    //  for example the all value below that combines all other possible values.
    [System.Flags]
    public enum eGameState
    {
        // Decimal      // Binary
        none = 0,       // 00000000
        mainMenu = 1,   // 00000001
        preLevel = 2,   // 00000010
        level = 4,      // 00000100
        postLevel = 8,  // 00001000
        gameOver = 16,  // 00010000
        all = 0xFFFFFFF // 11111111111111111111111111111111
    }

    [Header("Set in Inspector")]
    [Tooltip("This sets the AsteroidsScriptableObject to be used throughout the game.")]
    public AsteroidsScriptableObject asteroidsSO;

    public Text scoreBoard;
    public Text jumpsBoard;
    public Text finalScoreBoard;
    public Text finalLevelBoard;
    public Text finalScreenTitle;

    [SerializeField]
    private string levelToRestart = "_Scene_0";
    [SerializeField]
    private float timeUntilRestart = 4f;

    [SerializeField]
    private float timeOfShowingPrelevelPanel = 2f;

    [Header("This will be set by Remote Settings")]
    public string levelProgression = "1:3/2,2:4/2,3:3/3,4:4/3,5:5/3,6:3/4,7:4/4,8:5/4,9:6/4,10:3/5";
    
    [Header("These reflect static fields and are otherwise unused")]
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
        + "GAME_STATE_CHANGE_DELEGATE whenever GAME_STATE changes.")]
    protected eGameState _gameState; 
    
    private void Awake()
    {
        S = this;

        // This strange use of _gameState as an intermediary in the following lines 
        //  is solely to stop the Warning from popping up in the Console telling you 
        //  that _gameState was assigned but not used.
        _gameState = eGameState.mainMenu;
        GAME_STATE = _gameState;
    }


    void Start()
    {
        levelsConfiguration = levelProgression.Split(',');

        ASTEROIDS = new List<Asteroid>();

        SaveGameManager.Load();

        getHighScore = false;
    }

    void OnGUI()
    {
        scoreBoard.text = "Score: " + score.ToString();
        jumpsBoard.text = "Jumps: " + jumps.ToString();
    }
    
    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0;
            if (PAUSED_CHANGE_DELEGATE != null)
                PAUSED_CHANGE_DELEGATE();
        }
        else
        {
            isPaused = false;
            Time.timeScale = 1;
            if (PAUSED_CHANGE_DELEGATE != null)
                PAUSED_CHANGE_DELEGATE();
        }
    }

    public void StartLevel()
    { 
        asteroidsSO.numSmallerAsteroidsToSpawn = levelsConfiguration[curLevel][4] - '0'; //get this from remote settings

        // Spawn the parent Asteroids, child Asteroids are taken care of by them
        for (int i = 0; i < levelsConfiguration[curLevel][2] - '0'; i++)
        {
            SpawnParentAsteroid(i);
        }

        curLevel++;       
        if (curLevel >= AchievementManager.levelToReachSkillfullDodger && !AchievementManager.S.Achievements[5].complete)
        {
            HIGH_LEVEL_DELEGATE();
        }
        CustomAnalytics.SendLevelStart(curLevel);
    }

    void SpawnParentAsteroid(int i)
    {
        Asteroid ast = Asteroid.SpawnAsteroid();
        ast.gameObject.name = "Asteroid_" + i.ToString("00");

        // Find a good location for the Asteroid to spawn
        Vector3 pos;
        do
        {
            pos = ScreenBounds.RANDOM_ON_SCREEN_LOC;
        } 
        while ((pos - PlayerShip.POSITION).magnitude < MIN_ASTEROID_DIST_FROM_PLAYER_SHIP);

        ast.transform.position = pos;
        ast.size = asteroidsSO.initialSize;
    }

    public void ApplyDamage(GameObject player)
    {
        if (jumps <= 0)
        {
            StartCoroutine(GameOver(player));
        }
        else
        {
            jumps--;
            StartCoroutine(DamagePlayer(player));
        }
    }

    IEnumerator DamagePlayer(GameObject player)
    {
        Instantiate(player.GetComponent<PlayerShip>().disappearEffectPrefab, player.transform.position, Quaternion.identity);

        Vector3 safePosition;

        player.SetActive(false);               
        player.transform.position = new Vector3(10000, 10000, 0);

        yield return new WaitForSeconds(player.GetComponent<PlayerShip>().jumpingTime * 0.8f); 

        safePosition = player.GetComponent<PlayerShip>().FindSafePosition();
        player.transform.position = safePosition;
        //instantiate appear effect earlier than player appear
        Instantiate(player.GetComponent<PlayerShip>().appearEffectPrefab, safePosition, Quaternion.identity); 

        yield return new WaitForSeconds(player.GetComponent<PlayerShip>().jumpingTime * 0.2f);

        player.SetActive(true); //player appear
        is_jumping = false; //jump end
    }

    IEnumerator GameOver(GameObject player)
    {
        Destroy(player.GetComponent<PlayerShip>().shipExhaustEffectPrefab);
        Destroy(player);

        finalScoreBoard.text += score.ToString();
        finalLevelBoard.text += curLevel.ToString();

        if (getHighScore)
            finalScreenTitle.text = "New Highscore";
        else
            finalScreenTitle.text = "Game Over";

        GAME_STATE = eGameState.gameOver;

        SaveGameManager.CheckHighScore(score);
        SaveGameManager.Save();

        CustomAnalytics.SendFinalShipPartChoice();
        CustomAnalytics.SendGameOver();

        yield return new WaitForSeconds(timeUntilRestart);

        SceneManager.LoadScene(levelToRestart);
    }

    public IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(timeOfShowingPrelevelPanel);

        GAME_STATE = eGameState.level; 
        StartLevel();
    }

    // ---------------- Static Section ---------------- //

    /// <summary>
    /// <para>This static public property provides some protection for the Singleton _S.</para>
    /// <para>get {} does return null, but throws an error first.</para>
    /// <para>set {} allows overwrite of _S by a 2nd instance, but throws an error first.</para>
    /// <para>Another advantage of using a property here is that it allows you to place
    /// a breakpoint in the set clause and then look at the call stack if you fear that 
    /// something random is setting your _S value.</para>
    /// </summary>
    static public GameManager S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AsteraX:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AsteraX:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }


    static public AsteroidsScriptableObject AsteroidsSO
    {
        get
        {
            if (S != null)
            {
                return S.asteroidsSO;
            }
            return null;
        }
    }

    static public eGameState GAME_STATE
    {
        get
        {
            return _GAME_STATE;
        }
        set
        {
            if (value != _GAME_STATE)
            {
                _GAME_STATE = value;
                GameManager.S._gameState = _GAME_STATE;
                // Need to update all of the handlers
                // Any time you use a delegate, you run the risk of it not having any handlers
                //  assigned to it. In that case, it is null and will throw a null reference
                //  exception if you try to call it. So *any* time you call a delegate, you 
                //  should check beforehand to make sure it's not null.
                if (GAME_STATE_CHANGE_DELEGATE != null)
                {
                    GAME_STATE_CHANGE_DELEGATE();
                }
            }
        }
    }

    static public void AddAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) == -1)
        {
            ASTEROIDS.Add(asteroid);
        }
    }
    static public void RemoveAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) != -1)
        {
            ASTEROIDS.Remove(asteroid);
        }
        if (ASTEROIDS.Count == 0)
        {
            GAME_STATE = eGameState.preLevel;
        }
    }

}

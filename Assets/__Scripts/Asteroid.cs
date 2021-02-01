// These were used to test a case where some Asteroids were getting lost off screen.
//#define DEBUG_Asteroid_TestOOBVel 
//#define DEBUG_Asteroid_ShotOffscreenDebugLines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG_Asteroid_TestOOBVel
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OffScreenWrapper))]
public class Asteroid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public int size = 3;
    public bool immune = false;

    Rigidbody rigid; // protected
    OffScreenWrapper offScreenWrapper;

#if DEBUG_Asteroid_ShotOffscreenDebugLines
    [Header("ShotOffscreenDebugLines")]
	bool                trackOffscreen;
	Vector3             trackOffscreenOrigin;
#endif
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        offScreenWrapper = GetComponent<OffScreenWrapper>();
    }

    // Use this for initialization
    void Start()
    {
        GameManager.AddAsteroid(this);

        transform.localScale = Vector3.one * size * GameManager.AsteroidsSO.asteroidScale;
        if (parentIsAsteroid)
        {
            InitAsteroidChild();
        }
        else
        {
            InitAsteroidParent();
        }

        // Spawn child Asteroids
        if (size > 1)
        {
            Asteroid ast;
            for (int i = 0; i < GameManager.AsteroidsSO.numSmallerAsteroidsToSpawn; i++)
            {
                ast = SpawnAsteroid();
                ast.size = size - 1;
                ast.transform.SetParent(transform);
                Vector3 relPos = Random.onUnitSphere / 2;
                ast.transform.rotation = Random.rotation;
                ast.transform.localPosition = relPos;

                ast.gameObject.name = gameObject.name + "_" + i.ToString("00");
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.RemoveAsteroid(this);
    }

    public void InitAsteroidParent()
    {
#if DEBUG_Asteroid_ShotOffscreenDebugLines
		Debug.LogWarning(gameObject.name+" InitAsteroidParent() "+Time.time);
#endif
        offScreenWrapper.enabled = true;
        rigid.isKinematic = false;
        // Snap this GameObject to the z=0 plane
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
        // Initialize the velocity for this Asteroid
        InitVelocity();
    }

    public void InitAsteroidChild()
    {
        offScreenWrapper.enabled = false;
        rigid.isKinematic = true;
        // Make use of the ComponentDivision extension method in Vector3Extensions
        transform.localScale = transform.localScale.ComponentDivide(transform.parent.lossyScale);
    }

    public void InitVelocity()
    {
        Vector3 vel;

        // The initial velocity depends on whether the Asteroid is currently off screen or not
        if (ScreenBounds.OOB(transform.position))
        {
            // If the Asteroid is out of bounds, just point it toward a point near the center of the sceen
            vel = ((Vector3)Random.insideUnitCircle * 4) - transform.position;
            vel.Normalize();

#if DEBUG_Asteroid_TestOOBVel
            Debug.LogWarning("Asteroid:InitVelocity() - " + gameObject.name + " is OOB. Vel is: " + vel);
            EditorApplication.isPaused = true;
#endif

#if DEBUG_Asteroid_ShotOffscreenDebugLines
			Debug.DrawLine(transform.position, transform.position+vel, Color.red, 60);
			Debug.DrawLine(transform.position+Vector3.down, transform.position+Vector3.up, Color.cyan, 60);
            Debug.DrawLine(transform.position+Vector3.left, transform.position+Vector3.right, Color.cyan, 60);
			trackOffscreen = true;
			trackOffscreenOrigin = transform.position;
#endif

        }
        else
        {
            // If in bounds, choose a random direction, and make sure that when you Normalize it, it doesn't
            //  have a length of 0 (which might happen if Random.insideUnitCircle returned [0,0,0].
            do
            {
                vel = Random.insideUnitCircle;
                vel.Normalize();
            } while (Mathf.Approximately(vel.magnitude, 0f));
        }

        // Multiply the unit length of vel by the correct speed (randomized) for this size of Asteroid
        vel = vel * Random.Range(GameManager.AsteroidsSO.minVel, GameManager.AsteroidsSO.maxVel) / (float)size;
        rigid.velocity = vel;

        rigid.angularVelocity = Random.insideUnitSphere * GameManager.AsteroidsSO.maxAngularVel;
    }

#if DEBUG_Asteroid_ShotOffscreenDebugLines
	private void FixedUpdate()
	{
		if (trackOffscreen) {
			Debug.DrawLine(trackOffscreenOrigin, transform.position, Color.yellow, 0.1f);
		}
	}
#endif

    // NOTE: Allowing parentIsAsteroid and parentAsteroid to call GetComponent<> every
    //  time is inefficient, however, this only happens when a bullet hits an Asteroid
    //  which is rarely enough that it isn't a performance hit.
    bool parentIsAsteroid
    {
        get
        {
            return (parentAsteroid != null);
        }
    }

    Asteroid parentAsteroid
    {
        get
        {
            if (transform.parent != null)
            {
                Asteroid parentAsteroid = transform.parent.GetComponent<Asteroid>();
                if (parentAsteroid != null)
                {
                    return parentAsteroid;
                }
            }
            return null;
        }
    }

    public void OnCollisionEnter(Collision coll)
    {
        // If this is the child of another Asteroid, pass this collision up the chain
        if (parentIsAsteroid)
        {
            parentAsteroid.OnCollisionEnter(coll);
            return;
        }

        if (immune)
        {
            return;
        }

        GameObject otherGO = coll.gameObject;

        if (otherGO.tag == "Bullet" || otherGO.transform.root.gameObject.tag == "Player")
        {
            if (otherGO.tag == "Bullet")
            {               
                GameManager.score += GameManager.AsteroidsSO.pointsForAsteroidSize[size];
                if (GameManager.score >= AchievementManager.scoreToReachRookiePilot && !AchievementManager.S.Achievements[3].complete)
                {
                    GameManager.HIGH_SCORE_DELEGATE();
                }

                if (!GameManager.getHighScore && SaveGameManager.CheckHighScore(GameManager.score))
                {
                    GameManager.HIGH_SCORE_DELEGATE();
                    GameManager.getHighScore = true;
                }            

                Bullet.BULLET_HIT_ASTEROID_DELEGATE();
                if (otherGO.GetComponent<Bullet>().bDidWrap == true)
                    Bullet.LUCKY_SHOT_DELEGATE();

                Destroy(otherGO);
            }

            if (size > 1)
            {
                // Detach the children Asteroids
                Asteroid[] children = GetComponentsInChildren<Asteroid>();
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].immune = true;
                    if (children[i] == this || children[i].transform.parent != transform)
                    {
                        continue;
                    }
                    children[i].transform.SetParent(null, true);
                    children[i].InitAsteroidParent();
                }
            }
            int index = Random.Range(0, 2);
            GameObject go = GameManager.AsteroidsSO.asteroidExplosionPrefabs[index];
            go.transform.localScale = gameObject.transform.localScale;
            ParticleSystem particleSys = go.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSys.main;
            main.startSpeedMultiplier = 5 / gameObject.transform.localScale.x;
            Instantiate(go, transform.position, Quaternion.identity);
            Destroy(gameObject);       
        }
    }

    //work with position checkers
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Position checker")
        {
            coll.gameObject.GetComponent<PositionChecker>().isSafe = false;
        }
    }

    void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.tag == "Position checker")
        {
            coll.gameObject.GetComponent<PositionChecker>().isSafe = false;
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.tag == "Position checker")
        {
            coll.gameObject.GetComponent<PositionChecker>().isSafe = true;
        }
    }

    private void Update()
    {
        immune = false;
    }


    static public Asteroid SpawnAsteroid()
    {
        GameObject aGO = Instantiate<GameObject>(GameManager.AsteroidsSO.GetAsteroidPrefab());
        Asteroid ast = aGO.GetComponent<Asteroid>();
        return ast;
    }
}

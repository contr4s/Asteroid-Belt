#define DEBUG_PlayerShip_RespawnNotifications

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    // This is a somewhat protected private singleton for PlayerShip
    static private PlayerShip _S;
    static public PlayerShip S
    {
        get
        {
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogWarning("Second attempt to set PlayerShip singleton _S.");
            }
            _S = value;
        }
    }

    [Header("Set in Inspector")]
    public float shipSpeed = 10f;
    public float jumpingTime = 2f;
    public GameObject bulletPrefab;
    public GameObject shipExhaustEffectPrefab;
    public GameObject appearEffectPrefab;
    public GameObject disappearEffectPrefab;
    public GameObject positionCheckerPrefab;

    //private variables
    Rigidbody rigid;
    GameObject[] positionCheckers = new GameObject[15];

    void Awake()
    {
        S = this;

        // NOTE: We don't need to check whether or not rigid is null because of [RequireComponent()] above
        rigid = GetComponent<Rigidbody>();

        //set array of position checkers
        SetPositionCheckers();
    }

    void Update()
    {
        // Using Horizontal and Vertical axes to set velocity
        float aX = CrossPlatformInputManager.GetAxis("Horizontal");
        float aY = CrossPlatformInputManager.GetAxis("Vertical");

        Vector3 vel = new Vector3(aX, aY);
        if (vel.magnitude > 1)
        {
            // Avoid speed multiplying by 1.414 when moving at a diagonal
            vel.Normalize();
        }

        rigid.velocity = vel * shipSpeed;

        // Mouse input for firing
        if (CrossPlatformInputManager.GetButtonDown("Fire1") && GameManager.GAME_STATE == GameManager.eGameState.level && !GameManager.isPaused)
        {
            Fire();
        }
    }


    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "asteroid" && !GameManager.is_jumping)
        {
            GameManager.is_jumping = true;
            GameManager.S.ApplyDamage(gameObject);
        }
    }

    void SetPositionCheckers()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 position;
            if (i < 5)
                position = new Vector3((i % 5) * 6 - 12, 6, 0);
            else if (i < 10)
                position = new Vector3((i % 5) * 6 - 12, 0, 0);
            else
                position = new Vector3((i % 5) * 6 - 12, -6, 0);
            positionCheckers[i] = Instantiate(positionCheckerPrefab, position, Quaternion.identity);
        }
    }

    void Fire()
    {
        // Get direction to the mouse
        Vector3 mPos = Input.mousePosition;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 mPos3D = Camera.main.ScreenToWorldPoint(mPos);

        // Instantiate the Bullet and set its direction
        GameObject go = Instantiate<GameObject>(bulletPrefab);
        go.transform.position = transform.position;
        go.transform.LookAt(mPos3D);
    }

    public Vector3 FindSafePosition()
    {
        //iterations of search
        int it = 0;
        while (it < 100)
        {
            //i want from ship to reapear in different places
            int i = Random.Range(0, 15);
            if (positionCheckers[i].GetComponent<PositionChecker>().isSafe)
                return positionCheckers[i].transform.position;
            it++;
        }
        return new Vector3(0, 0, 0);
    }

    static public float MAX_SPEED
    {
        get
        {
            return S.shipSpeed;
        }
    }

    static public Vector3 POSITION
    {
        get
        {
            return S.transform.position;
        }
    }
}

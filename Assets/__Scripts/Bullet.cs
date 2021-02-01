using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OffScreenWrapper))]
public class Bullet : MonoBehaviour
{
    static private Transform _BULLET_ANCHOR;

    static public GameManager.CallbackDelegate BULLET_FIRED_DELEGATE;
    static public GameManager.CallbackDelegate BULLET_HIT_ASTEROID_DELEGATE;
    static public GameManager.CallbackDelegate LUCKY_SHOT_DELEGATE;

    static Transform BULLET_ANCHOR
    {
        get
        {
            if (_BULLET_ANCHOR == null)
            {
                GameObject go = new GameObject("BulletAnchor");
                _BULLET_ANCHOR = go.transform;
            }
            return _BULLET_ANCHOR;
        }
    }

    public float bulletSpeed = 20;
    public float lifeTime = 2;
    public GameObject bulletParticleSystem;
    public bool bDidWrap = false;
    void Start()
    {
        transform.SetParent(BULLET_ANCHOR, true);

        // Set Bullet to self-destruct in lifeTime seconds
        Invoke("DestroyMe", lifeTime);

        // Set the velocity of the Bullet
        GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;

        bulletParticleSystem.transform.LookAt(this.transform.position);

        BULLET_FIRED_DELEGATE();
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }
}

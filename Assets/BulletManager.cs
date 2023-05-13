using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance;
    public Transform muzzle;
    
    public Bullet bulletPrefab;
    private int amountOfBullets = 15;

    public IObjectPool<Bullet> bulletPool;
    // Start is called before the first frame update
    private void Awake()
    {
        // Init object pool
        bulletPool = new ObjectPool<Bullet>(
            CreateBullet, 
            OnGetBullet, 
            OnReleaseBullet, 
            OnDestroyBullet, 
            maxSize: amountOfBullets);
    }
    void Start()
    {
        Instance = this;
    }
    private void OnGetBullet(Bullet bullet)
    {
        // When bullet is requested, we activate the GO and init it
        bullet.gameObject.SetActive(true);
        bullet.Init(muzzle);
    }

    private void OnReleaseBullet(Bullet bullet)
    {
        // When releasing bullet, just deactivate it
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }

    private Bullet CreateBullet()
    {
        // When creating a bullet, Instantiate prefab to muzzle position
        // And setup the pool reference
        Bullet bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        bullet.SetPool(bulletPool);
        return bullet;
    }
}

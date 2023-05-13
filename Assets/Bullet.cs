using UnityEngine.Pool;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private IObjectPool<Bullet> bulletPool;
    private Rigidbody rb;
    public float bulletSpeed = 100;
    private MeshRenderer mesh;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshRenderer>();
        //For better physics collisions
        rb.solverIterations = 30;
    }

    public void Init(Transform targetTransform)
    {
        transform.position = targetTransform.position;
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        mesh.enabled = false;
    }

    public void Fire(Vector3 direction, float speedBooster)
    {
        // Force Impulse to fire the bullet
        rb.AddForce(direction * speedBooster * bulletSpeed, ForceMode.Impulse);
        // Visible when fired - could be finetuned for better effects
        mesh.enabled = true;
        // Update UI for bullet count
        UIManager.Instance.AddBullet();
        // Bullet disappears after 2seconds from being fired
        Invoke("Release", 2);
    }

    public void SetPool(IObjectPool<Bullet> pool)
    {
        bulletPool = pool;
    }


    private void Release()
    {
        bulletPool?.Release(this);
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        mesh.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log($"Collided with {other.transform.name}");
        rb.useGravity = true;
    }
}
using UnityEngine.Pool;
using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;
    private bool hasStartedDisabling = false;  
    private bool hasHitFloorOnce = false;
    private Rigidbody rb;

    private Coroutine disableCoroutine;
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        //For better physics collisions
        rb.solverIterations = 30;

        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale
            ;
        // Register block to blockManager
        BlockManager.Instance.RegisterBlock(this);
        // Update total score wheneve a new block is added
        UIManager.Instance.AddToTotalScore();
    }

    public void Reset()
    {
        // In case we reset scene while a block is still auto-disabling
        if(hasStartedDisabling)
        {
            StopCoroutine(disableCoroutine);  
            hasStartedDisabling = false;
        }

        hasHitFloorOnce = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        gameObject.SetActive(true);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other != null && other.gameObject.layer == LayerMask.NameToLayer("floor")) 
        {
            if(!hasHitFloorOnce)
            {
                // First time bock hits the floor, we count a point
                // And start counting for disappearing
                hasHitFloorOnce = true;
                UIManager.Instance.AddPoint();
                disableCoroutine = StartCoroutine(Disable());
            }
        }
    }
    private IEnumerator Disable()
    {
        hasStartedDisabling = true;
        yield return new WaitForSeconds(3);
        gameObject.SetActive(false);
        hasStartedDisabling = false;

    }
}
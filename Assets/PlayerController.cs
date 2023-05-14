using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerController : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject cameraOffset;

    public InputActionAsset InputActionMap;
    public InputActionReference ResetAction;
    public InputActionReference FireAction;
    public InputActionReference RightClickAction;
    public InputActionReference MousePositionAction;
    public InputActionReference MousePositionDeltaAction;
    public InputActionReference ScrollWheel;
    public InputActionReference ThumbStickAction;
    public InputActionReference GripAction;

    private bool isOrbiting = false;
    private bool isZooming = false;

    public Texture2D cursorMainTexture;
    public Texture2D cursorOrbitTexture;
    private Vector2 cursorMainHotspot;
    private Vector2 cursorOrbitHotspot;
    private CursorMode defaultCursorMode;

    public float orbitDistance;
    private const float maxOrbitDistance = -5f;
    private const float minOrbitDistance = -3f;
    public float maxRotationX;
    public float orbitSensitivity;
    public float scrollSensitivity;

    internal Camera mainCamera;
    Vector2 mousePositionDelta;
    Vector2 scrollDelta;
    Quaternion newRotation;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private float zPosition = 0.0f;
    private float zDelta = 0.0f;
    private Vector3 cameraLocalPosition;
    private Vector3 newCameraLocalPosition;

    public Transform CannonHolder;

    public LayerMask cannonMask;
    private RaycastHit hit;
    private Ray ray;
    private const int maxRayDistance = 100;

    Bullet currentBullet;
    public const float BulletVelocityBoosterDefault = 1.0f;
    private float bulletVelocityBooster;
    private float bulletVelocityBoosterMax = 2.0f;
    private Coroutine boosterCoroutine;
    [SerializeField]
    public Canvas canvasUI;

    public bool isVR = false;

    private void Awake()
    {
        if (UnityEngine.XR.XRSettings.loadedDeviceName != "")
        {
            Debug.Log("VR ENABLED");
            isVR = true;
        }
        else
        {
            Debug.Log("VR disabled");
            isVR = false;
        }
    }

    void Start()
    {

        mainCamera = Camera.main;
     

        if(isVR)
        {
            // Move UI to right controller when in VR, with custom measures
            canvasUI.renderMode = RenderMode.WorldSpace;
            canvasUI.transform.SetParent(rightHand.transform, false);
            var rect = canvasUI.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(450, 300);
            rect.pivot = new Vector2(0,0);
            canvasUI.transform.localPosition = Vector3.zero;
            canvasUI.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);

            leftHand.SetActive(false);
            rightHand.SetActive(true);

            CannonHolder.SetParent(rightHand.transform, false);

            orbitSensitivity = 0.5f;
            scrollSensitivity = 0.05f;
        }
        else
        {
            canvasUI.renderMode = RenderMode.ScreenSpaceOverlay;

            leftHand.SetActive(false); 
            rightHand.SetActive(false);
        }

        InputActionMap.Enable();
        defaultCursorMode = CursorMode.Auto;
        cursorMainHotspot = new Vector2(cursorMainTexture.width / 2, cursorMainTexture.height / 2);
        cursorOrbitHotspot = new Vector2(cursorOrbitTexture.width / 2, cursorOrbitTexture.height / 2);
        Cursor.SetCursor(cursorMainTexture, cursorMainHotspot, defaultCursorMode);

        RightClickAction.action.started += OnRightClickStarted;
        RightClickAction.action.canceled += OnRightClickReleased;

        ThumbStickAction.action.started += OnThumbStickStarted;
        ThumbStickAction.action.canceled += OnThumbStickCanceled; 
        
        GripAction.action.started += OnGripStarted;
        GripAction.action.canceled += OnGripReleased;

        ScrollWheel.action.started += OnScrollWheelStarted;
        ScrollWheel.action.canceled += OnScrollWheelReleased;

        ResetAction.action.performed += OnResetPerformed;

        FireAction.action.started += OnFireStarted;
        FireAction.action.canceled += OnFireReleased;
    }

    private void OnRightClickStarted(InputAction.CallbackContext context)
    {
        if (!isZooming)
        {
            Cursor.SetCursor(cursorOrbitTexture, cursorOrbitHotspot, defaultCursorMode);
            isOrbiting = true;
        }
    }
    private void OnRightClickReleased(InputAction.CallbackContext context) 
    {
        if (!isZooming)
        {
            Cursor.SetCursor(cursorMainTexture, cursorMainHotspot, CursorMode.Auto);
            isOrbiting = false;
        }
    }    
    private void OnThumbStickStarted(InputAction.CallbackContext context) 
    {
        if(!isZooming)
        {
            isOrbiting = true;
        }
    }
    private void OnThumbStickCanceled(InputAction.CallbackContext context) 
    {
        if (!isZooming)
        {
            isOrbiting = false;
        }
    }
    private void OnGripStarted(InputAction.CallbackContext context)
    {
        if(!isOrbiting)
        {
            isZooming = true;
        }
    }
    private void OnGripReleased(InputAction.CallbackContext context) 
    {
        if (!isOrbiting)
        {
            isZooming = false;
        }
    }
    private void OnScrollWheelStarted(InputAction.CallbackContext context)
    {
        if (!isOrbiting)
        {
            isZooming = true;
        }
    }
    private void OnScrollWheelReleased(InputAction.CallbackContext context)
    {
        if (!isOrbiting)
        {
            isZooming = false;
        }
    }
    private void Orbit()
    {
        if(isVR)
        {
            mousePositionDelta = ThumbStickAction.action.ReadValue<Vector2>();
            xRotation += mousePositionDelta.y * orbitSensitivity;
            yRotation -= mousePositionDelta.x * orbitSensitivity;
        }
        else
        {
            mousePositionDelta = MousePositionDeltaAction.action.ReadValue<Vector2>();
            xRotation -= mousePositionDelta.y * orbitSensitivity; 
            yRotation += mousePositionDelta.x * orbitSensitivity;
        }
            xRotation = Mathf.Clamp(xRotation, 0, maxRotationX);

            newRotation = Quaternion.Euler(xRotation, yRotation, 0);
            transform.rotation = newRotation;

    }

    private void Zoom()
    {

        cameraLocalPosition = cameraOffset.transform.localPosition;
        if(isVR)
        {
            scrollDelta = ThumbStickAction.action.ReadValue<Vector2>();
        }
        else
        {
            scrollDelta = ScrollWheel.action.ReadValue<Vector2>();
        }

        zDelta = (scrollDelta.y * scrollSensitivity);
        zPosition = cameraLocalPosition.z + zDelta;
        zPosition = Mathf.Clamp(zPosition, maxOrbitDistance, minOrbitDistance);
        newCameraLocalPosition = new Vector3(cameraLocalPosition.x, cameraLocalPosition.y, zPosition);
        cameraOffset.transform.localPosition = newCameraLocalPosition;
    }

    private void OnFireStarted(InputAction.CallbackContext context)
    {
        boosterCoroutine = StartCoroutine(ChargeBulletVelocityBooster());
    }

    private void OnFireReleased(InputAction.CallbackContext context) 
    {
        if (boosterCoroutine != null)
        {
            StopCoroutine(boosterCoroutine);
        }

        currentBullet = BulletManager.Instance.bulletPool.Get();
        currentBullet.Fire(CannonHolder.forward, bulletVelocityBooster);
        // Reset UI speed
        UIManager.Instance.SetSpeed(0, bulletVelocityBoosterMax, BulletVelocityBoosterDefault);
        // Reset booster
        bulletVelocityBooster = BulletVelocityBoosterDefault;
    }    
    private void OnResetPerformed(InputAction.CallbackContext context) 
    {
        // By resetting blocks and not Destroy/re-Instantiate, we increment performances
        BlockManager.Instance.ResetAllBlocks();
        UIManager.Instance.ResetUI();
    }

    private void MoveTurret()
    {
        // Turret follows mouse pointer
        ray = mainCamera.ScreenPointToRay(MousePositionAction.action.ReadValue<Vector2>());
        ray.origin = ray.GetPoint(maxRayDistance);
        ray.direction = -ray.direction;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cannonMask))
        {
            CannonHolder.LookAt(hit.point);
        }
    }

    void Update()
    {
        if(!isVR)
        {
            MoveTurret();
        }
        if(isOrbiting) 
        {
            Orbit();
        }   
        if(isZooming)
        {
            Zoom();
        }
    }

    private IEnumerator ChargeBulletVelocityBooster()
    {
        bulletVelocityBooster = BulletVelocityBoosterDefault;
        while(bulletVelocityBooster < bulletVelocityBoosterMax)
        {
            // We add value first, so we can show a small red bar in the UI at each shot
            bulletVelocityBooster += 0.1f;
            UIManager.Instance.SetSpeed(bulletVelocityBooster, bulletVelocityBoosterMax, BulletVelocityBoosterDefault);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
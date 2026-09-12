using UnityEngine;

/// <summary>
/// Simple follow camera for exploration scenes. Attach to the scene's Main Camera.
/// It finds the persistent player (Tag = Player) and follows with a fixed oblique offset.
/// </summary>
public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Tag used by the protagonist.")]
    public string playerTag = "Player";

    [Header("Follow")]
    [Tooltip("Camera position = player position + this offset.")]
    public Vector3 followOffset = new Vector3(0f, 3f, -10f);
    public bool lookAtTarget = true;

    [Header("Lens")]
    public bool applyFieldOfView = true;
    [Range(1f, 120f)] public float fieldOfView = 45f;

    [Header("Smoothing")]
    public bool smoothFollow = true;
    [Range(0f, 20f)] public float positionSmooth = 8f;

    private Transform target;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null && applyFieldOfView)
        {
            cam.fieldOfView = fieldOfView;
        }

        FindTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        Vector3 desiredPosition = target.position + followOffset;
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmooth * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }

        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            target = player.transform;
        }
    }
}

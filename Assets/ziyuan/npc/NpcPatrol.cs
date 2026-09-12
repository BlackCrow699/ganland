using UnityEngine;

/// <summary>Patrol controller for a 2D NPC with a single walk animation.</summary>
public class NpcPatrol : MonoBehaviour
{
    [Header("Route")]
    [Tooltip("Parent transform whose children are the waypoints, in patrol order.")]
    public Transform route;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float waypointTolerance = 0.2f;

    [Header("Behavior")]
    public bool pingPong = true;
    public float waitTime = 1f;

    [Header("Animation")]
    [Tooltip("Name of the single walk state in the Animator.")]
    public string walkState = "Walk";
    [Tooltip("Optional idle state name. Leave empty to freeze the walk animation while waiting.")]
    public string idleState = "";

    [Header("Obstacle Avoidance")]
    [Tooltip("Turn around when a wall or obstacle is detected ahead.")]
    public bool avoidObstacles = true;
    public float obstacleCheckDistance = 0.7f;
    [Tooltip("Height above the NPC's feet where the obstacle ray is cast.")]
    public float obstacleCheckHeight = 0.6f;
    public LayerMask obstacleMask = ~0;
    public float reverseCooldown = 0.4f;

    private Animator animator;
    private Transform[] waypoints;
    private int currentIndex;
    private int direction = 1;
    private bool isWaiting;
    private float waitTimer;
    private float reverseCooldownTimer;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (route == null)
        {
            Debug.LogWarning($"[NpcPatrol] {name}: no route assigned. Standing idle.", this);
            SetIdle();
            return;
        }

        waypoints = new Transform[route.childCount];
        for (int i = 0; i < route.childCount; i++)
        {
            waypoints[i] = route.GetChild(i);
        }

        if (waypoints.Length == 0)
        {
            Debug.LogWarning($"[NpcPatrol] {name}: route has no waypoint children. Standing idle.", this);
            SetIdle();
            return;
        }

        currentIndex = 0;
        direction = 1;
        isWaiting = false;
        waitTimer = 0f;
        reverseCooldownTimer = 0f;
    }

    void Update()
    {
        if (reverseCooldownTimer > 0f)
        {
            reverseCooldownTimer -= Time.deltaTime;
        }

        if (waypoints == null || waypoints.Length <= 1)
        {
            SetIdle();
            return;
        }

        if (isWaiting)
        {
            SetIdle();
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                AdvanceIndex();
            }
            return;
        }

        Transform targetWaypoint = waypoints[currentIndex];
        if (targetWaypoint == null)
        {
            AdvanceIndex();
            return;
        }

        Vector3 target = targetWaypoint.position;
        target.y = transform.position.y;

        Vector3 delta = target - transform.position;
        if (delta.magnitude <= waypointTolerance)
        {
            transform.position = target;
            waitTimer = 0f;
            isWaiting = true;
            SetIdle();
            return;
        }

        if (avoidObstacles && reverseCooldownTimer <= 0f && IsBlocked(delta))
        {
            ReverseDirection();
            reverseCooldownTimer = reverseCooldown;
            return;
        }

        SetWalking();
        transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
    }

    void AdvanceIndex()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return;
        }

        int next = currentIndex + direction;
        if (next >= waypoints.Length)
        {
            if (pingPong)
            {
                direction = -1;
                next = waypoints.Length - 2;
            }
            else
            {
                next = 0;
            }
        }
        else if (next < 0)
        {
            if (pingPong)
            {
                direction = 1;
                next = 1;
            }
            else
            {
                next = waypoints.Length - 1;
            }
        }

        currentIndex = next;
    }

    void SetWalking()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }

        PlayState(walkState);
    }

    void SetIdle()
    {
        if (HasState(idleState))
        {
            if (animator != null)
            {
                animator.speed = 1f;
            }
            PlayState(idleState);
        }
        else if (animator != null)
        {
            animator.speed = 0f;
        }
    }

    void PlayState(string stateName)
    {
        if (!HasState(stateName))
        {
            return;
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            animator.Play(stateName);
        }
    }

    bool HasState(string stateName)
    {
        return animator != null && animator.runtimeAnimatorController != null &&
               !string.IsNullOrEmpty(stateName) && animator.HasState(0, Animator.StringToHash(stateName));
    }

    bool IsBlocked(Vector3 delta)
    {
        Vector3 direction = delta;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * obstacleCheckHeight;
        return Physics.Raycast(origin, direction.normalized, obstacleCheckDistance, obstacleMask);
    }

    void ReverseDirection()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return;
        }

        int previousIndex = currentIndex - direction;

        if (pingPong)
        {
            direction = -direction;
            if (previousIndex < 0)
            {
                previousIndex = 0;
            }
            else if (previousIndex >= waypoints.Length)
            {
                previousIndex = waypoints.Length - 1;
            }
        }
        else
        {
            if (previousIndex < 0)
            {
                previousIndex = waypoints.Length - 1;
            }
        }

        currentIndex = previousIndex;
    }
}

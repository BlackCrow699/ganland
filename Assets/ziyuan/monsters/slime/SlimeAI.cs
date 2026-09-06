using UnityEngine;

/// <summary>Simple roaming controller for slimes. Encounters and combat are handled by shared systems.</summary>
public class SlimeAI : MonoBehaviour
{
    [Header("Wandering")]
    public float walkSpeed = 1.1f;
    public float runSpeed = 2.5f;
    public float wanderRadius = 3f;
    public float waypointTolerance = 0.1f;
    public float detectRadius = 5f;

    [Header("Animation")]
    public string walkAnimationState = "slimewalk";
    public string attackAnimationState = "slimeattack";

    private Animator animator;
    private Vector3 startPosition;
    private Vector3 waypoint;
    private Transform player;
    private bool isChasing;

    void Start()
    {
        animator = GetComponent<Animator>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        startPosition = transform.position;
        ChooseWaypoint();
        if (animator != null && animator.runtimeAnimatorController != null &&
            animator.HasState(0, Animator.StringToHash(walkAnimationState)) &&
            !animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnimationState))
        {
            animator.Play(walkAnimationState);
        }
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }

        isChasing = player != null && Vector3.Distance(transform.position, player.position) <= detectRadius;
        Vector3 target = isChasing ? player.position : waypoint;
        target.y = transform.position.y;
        float speed = isChasing ? runSpeed : walkSpeed;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (!isChasing && Vector3.Distance(transform.position, target) <= waypointTolerance)
            ChooseWaypoint();

        PlayAnimationIfNeeded(walkAnimationState);

        if (Mathf.Abs(target.x - transform.position.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (target.x >= transform.position.x ? 1f : -1f);
            transform.localScale = scale;
        }
    }

    void ChooseWaypoint()
    {
        Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, wanderRadius);
        waypoint = startPosition + new Vector3(offset.x, 0f, offset.y);
    }

    public void PlayAttackAnimation()
    {
        PlayAnimation(attackAnimationState);
    }

    void PlayAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName) || animator.runtimeAnimatorController == null) return;
        if (animator.HasState(0, Animator.StringToHash(stateName))) animator.Play(stateName);
    }

    void PlayAnimationIfNeeded(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName) || animator.runtimeAnimatorController == null) return;
        int hash = Animator.StringToHash(stateName);
        if (animator.HasState(0, hash) && !animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            animator.Play(stateName);
    }
}

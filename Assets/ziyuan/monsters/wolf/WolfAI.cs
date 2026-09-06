using UnityEngine;

public class WolfAI : MonoBehaviour
{
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;
    public float detectRadius = 5.0f;

    private Transform player;
    private Animator anim;
    private Vector3 startPos;
    private Vector3 targetPos;

    private bool isChasing = false;
    private bool isAlerting = false; 
    private bool isDead = false;  

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        startPos = transform.position;
        GetNewWaypoint();
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }
        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;
        if (distanceToPlayer < detectRadius && !isChasing && !isAlerting)
        {
            DiscoverPlayer();
        }

        if (isChasing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, runSpeed * Time.deltaTime);
            FlipSprite(player.position.x);
        }
        else if (!isAlerting)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);
            FlipSprite(targetPos.x);

            if (Vector3.Distance(transform.position, targetPos) < 0.1f) GetNewWaypoint();
        }
    }

    void DiscoverPlayer()
    {
        isAlerting = true;
        if (anim != null) anim.SetTrigger("DoAlert");
        Invoke("StartChasing", 0.8f); 
    }

    void StartChasing()
    {
        if (isDead) return;
        isAlerting = false;
        isChasing = true;
        if (anim != null) anim.SetBool("IsChasing", true);
    }

    public void Die()
    {
        isDead = true;
        isChasing = false;
        isAlerting = false;
        if (anim != null) anim.SetTrigger("DoDie");
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }

    void GetNewWaypoint()
    {
        float randomX = Random.Range(-3f, 3f);
        float randomZ = Random.Range(-3f, 3f);
        targetPos = new Vector3(startPos.x + randomX, transform.position.y, startPos.z + randomZ);
    }

    void FlipSprite(float targetX)
    {
        if (targetX > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
        else if (targetX < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
    }
}

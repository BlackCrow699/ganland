using UnityEngine;
using Cinemachine;

/// <summary>
/// Binds a Cinemachine Virtual Camera to the persistent player at runtime.
/// Use this in scenes that do not have a pre-placed Player object (the player arrives by teleport).
/// </summary>
public class CinemachinePlayerBinder : MonoBehaviour
{
    [Tooltip("Tag used by the player.")]
    public string playerTag = "Player";
    public bool bindFollow = true;
    public bool bindLookAt = true;

    [Header("Retry")]
    [Tooltip("If the player is not found yet, retry after this delay.")]
    public float retryInterval = 0.1f;

    private CinemachineVirtualCamera vcam;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        TryBind();
    }

    void TryBind()
    {
        if (vcam == null)
        {
            vcam = GetComponent<CinemachineVirtualCamera>();
        }

        if (vcam == null)
        {
            Debug.LogWarning("CinemachinePlayerBinder: CinemachineVirtualCamera component not found.", this);
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Invoke(nameof(TryBind), retryInterval);
            return;
        }

        if (bindFollow) vcam.Follow = player.transform;
        if (bindLookAt) vcam.LookAt = player.transform;
    }
}

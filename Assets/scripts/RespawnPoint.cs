using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public Transform respawnPosition;

    private static Transform currentCheckpoint;
    private static Vector3 startingPosition;
    private static Quaternion startingRotation;

    public static void SetStartingPosition(Transform player)
    {
        startingPosition = player.position;
        startingRotation = player.rotation;
        currentCheckpoint = null;
    }

    void Awake()
    {
        if (respawnPosition == null)
            respawnPosition = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentCheckpoint = respawnPosition;
            Debug.Log("Checkpoint activated: " + gameObject.name);
        }
    }

    public static void RespawnPlayer(GameObject player)
    {
        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        if (currentCheckpoint != null)
        {
            player.transform.SetPositionAndRotation(
                currentCheckpoint.position,
                currentCheckpoint.rotation
            );
        }
        else
        {
            player.transform.SetPositionAndRotation(
                startingPosition,
                startingRotation
            );
        }

        if (controller != null)
            controller.enabled = true;
    }
}

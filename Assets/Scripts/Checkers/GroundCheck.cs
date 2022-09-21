using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    PlayerController playerController;

    private void Awake()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController.SetIsGrounded(true);
    }

    private void OnTriggerExit(Collider other)
    {
        playerController.SetIsGrounded(false);
    }

    private void OnTriggerStay(Collider other)
    {
        playerController.SetIsGrounded(true);
    }
}

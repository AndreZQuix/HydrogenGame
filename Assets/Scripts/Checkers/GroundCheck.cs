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
        if (other.gameObject == playerController.gameObject)
            return;

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

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject == playerController.gameObject)
    //        return;

    //    playerController.SetIsGrounded(true);
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject == playerController.gameObject)
    //        return;

    //    playerController.SetIsGrounded(false);
    //}

    //private void OnCollisionStay(Collision collision)
    //{
    //    if (collision.gameObject == playerController.gameObject)
    //        return;

    //    playerController.SetIsGrounded(true);
    //}
}

using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag(playerTag) && other.transform.parent.TryGetComponent<PlayerMovement>(out var player))
            player.SetMovementState(PlayerMovement.MovementState.Climbing);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && other.transform.parent.TryGetComponent<PlayerMovement>(out var player))
        {
            player.SetMovementState(PlayerMovement.MovementState.Walking);
            player.StopClimb();
        }
    }
}

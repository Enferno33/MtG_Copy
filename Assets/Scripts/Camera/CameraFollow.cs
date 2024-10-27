using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f; // How smooth the camera movement is
    public Vector3 offset; // Offset position of the camera relative to the player

    private Transform playerTransform; // Reference to the player's transform

    void Start()
    {
        // Find the player GameObject at the start
        FindPlayer();
    }

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            // Desired position of the camera with the offset
            Vector3 desiredPosition = playerTransform.position + offset;
            // Smoothly move the camera towards the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Optionally, keep the camera looking straight at the player
            // transform.LookAt(playerTransform);
        }
        else
        {
            // Try to find the player again if it was not found initially
            FindPlayer();
        }
    }

    // Method to find the player GameObject
    void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player"); // Ensure the player prefab has the tag "Player"
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found. Make sure the player prefab has the tag 'Player'.");
        }
    }
}

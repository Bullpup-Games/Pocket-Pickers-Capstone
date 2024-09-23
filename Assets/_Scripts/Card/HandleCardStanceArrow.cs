using PlayerController;
using UnityEngine;

public class HandleCardStanceArrow : MonoBehaviour
{
    public Transform player;
    public GameObject directionalArrowPrefab;  // Arrow prefab to instantiate
    private GameObject _directionalArrowInstance;  // The instantiated arrow in the scene
    private PlayerMovementController _player;
    public float horizontalOffsetDistance = 2.25f;
    public float verticalOffsetDistance = 0.8f;

    private void Awake()
    {
        _player = player.GetComponent<PlayerMovementController>();
    }

    // Instantiates the directional arrow facing the horizontal direction of the player
    // Called in GameManager
    public void InstantiateDirectionalArrow()
    {
        // Determine the direction based on the player's FrameInput.x
        var playerDirection = _player.FrameInput.x < 0 ? Vector3.left : Vector3.right;

        var arrowPosition = player.position + (playerDirection * horizontalOffsetDistance) + new Vector3(0, verticalOffsetDistance, 0);

        _directionalArrowInstance = Instantiate(
            directionalArrowPrefab,
            arrowPosition,
            Quaternion.identity
        );

        // Calculate the rotation angle based on the direction
        var angle = (_player.FrameInput.x < 0) ? 90f : -90f; // Left: 90°, Right: -90°
        _directionalArrowInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log("Arrow shown");
    }

    // Destroys the directional arrow when the player leaves card stance or throws a card
    // Called in GameManager
    public void DestroyDirectionalArrow()
    {
        if (_directionalArrowInstance == null) return;
        Destroy(_directionalArrowInstance);
        Debug.Log("Arrow Hidden");
    }
}

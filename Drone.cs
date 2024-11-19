using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider2D))]
public class Drone : MonoBehaviour
{
    public int id; // Unique identifier for each drone
    public int Temperature { set; get; } = 0;
    private float runtime = 0f; // Track the elapsed time for each drone
    private float speed = 5f; // Default speed value for the drone

    private SpriteRenderer spriteRenderer; // Reference to SpriteRenderer
    private bool isFlashing = false;

    Flock agentFlock;
    public Flock AgentFlock { get { return agentFlock; } }

    Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }

    // Flag to track if the drone has been destroyed
    public bool isDestroyed = false;
    public float driveFactor = 8f;

    // Property to get and set the drone's speed
    public float Speed
    {
        get { return speed; }
        set { speed = Mathf.Clamp(value, 0, 100); } // Speed is clamped between 0 and 100 for safety
    }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        Temperature = (int)(Random.value * 100); // Simulate temperature change
        runtime += Time.deltaTime; // Increment runtime by the time passed since the last frame
    }

    // Public method to access the runtime of each drone
    public float GetRuntime()
    {
        return runtime;
    }

    public void Initialize(Flock flock, int id)
    {
        agentFlock = flock;
        this.id = id; // Assign the unique ID to this drone
    }

    public void Move(Vector2 velocity)
    {
        transform.up = velocity;
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
        isFlashing = false; // Stop flashing if back in range
    }

    public void SetFlashing(bool flashing)
    {
        if (!isFlashing && flashing)
        {
            StartCoroutine(FlashBeforeDestruction());
        }
        isFlashing = flashing;
    }

    IEnumerator FlashBeforeDestruction()
    {
        Color originalColor = spriteRenderer.color;
        while (isFlashing)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = Color.clear;
            yield return new WaitForSeconds(0.5f);
        }
        spriteRenderer.color = originalColor;
    }

    // New method to hide the drone (simulate self-destruct)
    public void SelfDestruct()
    {
        if (!isDestroyed)
        {
            gameObject.SetActive(false); // Deactivate the drone
            isDestroyed = true; // Mark the drone as destroyed
        }
    }

    // New method to show the drone (if needed to bring it back)
    public void Revive()
    {
        if (isDestroyed)
        {
            gameObject.SetActive(true); // Reactivate the drone
            isDestroyed = false; // Mark the drone as not destroyed
        }
    }

    public void Hide()
    {
        if (!isDestroyed)
        {
            gameObject.SetActive(false); // Deactivate the drone (simulate self-destruction)
            isDestroyed = true;          // Mark the drone as destroyed
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DirectionalMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    [Tooltip("How long to remember diagonal movement after key release (seconds)")]
    public float diagonalMemoryDuration = 0.15f; // Remember diagonal movement for a short time

    [Header("Rendering & Animation")]
    public Animator animator;                 // needs Float "Octant", Float "Speed"
    public SpriteRenderer spriteRenderer;     // we flip this for West facings

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 lastNonZeroInput;         // Remember last movement direction
    private Direction16 currentDir = Direction16.S;   // <- DEFAULT FACING
    private float lastOctant = 8f;            // <- Track the last octant value (default to South/8)
    private float diagonalMemoryTimer = 0f;   // Timer for diagonal memory
    private bool wasDiagonal = false;         // Was the last movement diagonal?

    // 0 = E … 15 = ESE
    public enum Direction16 { E, ENE, NE, NNE, N, NNW, NW, WNW, W, WSW, SW, SSW, S, SSE, SE, ESE }
    public Direction16 CurrentDirection => currentDir;
    public System.Action<Direction16> OnDirectionChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0;
        rb.freezeRotation = true;
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        lastNonZeroInput = new Vector2(0, -1); // Default to south
    }

    private void Start()   // ← set initial pose once
    {
        SetAnimatorFromDirection(currentDir);
    }

    private void Update()
    {
        // 1. Read input (legacy Input Manager)
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        bool isMoving = input.sqrMagnitude > 0.0001f;

        // Check if we're moving diagonally
        bool isDiagonal = isMoving && Mathf.Abs(input.x) > 0.01f && Mathf.Abs(input.y) > 0.01f;
        
        if (isMoving)
        {
            // Store last non-zero input direction when moving
            lastNonZeroInput = input;
            
            // Remember if we were moving diagonally
            wasDiagonal = isDiagonal;
            
            // Reset diagonal memory timer when moving
            diagonalMemoryTimer = 0f;
            
            // Calculate direction
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            int sector = Mathf.RoundToInt(angle / 22.5f) & 15;

            Direction16 newDir = (Direction16)sector;
            if (newDir != currentDir)
            {
                currentDir = newDir;
                OnDirectionChanged?.Invoke(currentDir);
                SetAnimatorFromDirection(currentDir);
            }
        }
        else
        {
            // Not moving - check if we were just moving diagonally
            if (wasDiagonal)
            {
                // Update diagonal memory timer
                diagonalMemoryTimer += Time.deltaTime;
                
                // If we're still within the memory duration, use the last diagonal input
                if (diagonalMemoryTimer <= diagonalMemoryDuration)
                {
                    // Calculate direction from remembered input
                    float angle = Mathf.Atan2(lastNonZeroInput.y, lastNonZeroInput.x) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;
                    int sector = Mathf.RoundToInt(angle / 22.5f) & 15;

                    Direction16 newDir = (Direction16)sector;
                    if (newDir != currentDir)
                    {
                        currentDir = newDir;
                        OnDirectionChanged?.Invoke(currentDir);
                        SetAnimatorFromDirection(currentDir);
                    }
                }
            }
        }

        // Always update "Speed" so Idle ↔ Walk transitions work
        if (animator) animator.SetFloat("Speed", input.sqrMagnitude);

        // Always force octant parameter to maintain its value, even during transitions
        if (animator) animator.SetFloat("Octant", lastOctant);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }

    // --------------------------------------------------------
    // Helpers
    // --------------------------------------------------------
    private void SetAnimatorFromDirection(Direction16 dir)
    {
        int sector = (int)dir;
        float octant;

        // For directions E through N (sectors 0-4)
        if (sector >= 0 && sector <= 4) {
            octant = sector;
        }
        // For directions NNW through W (sectors 5-8)
        else if (sector >= 5 && sector <= 8) {
            octant = 8 - sector; // Map to E through NE
        }
        // South direction
        else if (sector == 12) {
            octant = 8; // Use threshold 8 for South direction
        }
        // For directions WSW through SSW (sectors 9-11)
        else if (sector >= 9 && sector <= 11) {
            octant = 12 - sector + 4; // Map to octants 7-4
        }
        // For directions SSE through ESE (sectors 13-15)
        else {
            octant = sector - 8; // Map to octants 5-7
        }

        // Store the calculated octant value in our class variable
        lastOctant = octant;
        
        // Set it in the animator
        if (animator) animator.SetFloat("Octant", octant);

        // Flip sprite for W hemispheres (NNW through SSW)
        bool flip = sector >= 5 && sector <= 11;
        if (spriteRenderer) spriteRenderer.flipX = flip;
    }
}
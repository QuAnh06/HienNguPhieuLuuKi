using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRender;
    private bool dashLocked;

    public Vector2 CurrentMovement => movement;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRender = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();
    private void OnDestroy() => playerControls.Dispose();

    private void Update()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        if (movement.sqrMagnitude > 1f) movement.Normalize();
        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);
    }

    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        if (!dashLocked) rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public void SetDashLocked(bool locked) => dashLocked = locked;

    private void AdjustPlayerFacingDirection()
    {
        if (Mouse.current == null || Camera.main == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position);
        mySpriteRender.flipX = mousePos.x < screen.x;
    }
}

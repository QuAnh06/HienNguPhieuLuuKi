using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerCombat : MonoBehaviour, IWeapon
{
    // ── IWeapon ───────────────────────────────────────────────────────────────
    [Header("Sword Weapon Info")]
    [SerializeField] private WeaponInfo swordWeaponInfo;

    public void Attack() => TryAttack();
    public WeaponInfo GetWeaponInfo() => swordWeaponInfo;

    /// <summary>Called by ActiveWeapon to show/hide the sword when switching weapons.</summary>
    public void SetSwordActive(bool active)
    {
        swordActive = active;
        if (swordVisual)   swordVisual.gameObject.SetActive(active);
        if (slashRenderer) slashRenderer.enabled = false;
        if (!active && isAttacking) isAttacking = false; // cancel any ongoing swing display
    }
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Sword Configuration")]
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private float hitDelay = 0.10f;
    [SerializeField] private float slashDuration = 0.22f;
    [SerializeField] private float swordUpAngle = 135f;
    [SerializeField] private float swordDownAngle = -120f;
    [SerializeField] private Vector2 handOffsetUp = new Vector2(0.16f, -0.06f);
    [SerializeField] private Vector2 handOffsetDown = new Vector2(-0.08f, -0.30f);
    [SerializeField] private float swingForwardExtension = 0.08f;
    [SerializeField] private Vector2 slashOffset = new Vector2(0.35f, 0.05f);
    [SerializeField] private bool initialSwordUp = true;

    [Header("References")]
    [SerializeField] private MeleeHitbox hitbox;
    [SerializeField] private SpriteRenderer slashRenderer;
    [SerializeField] private Sprite[] slashFrames = new Sprite[0];
    [SerializeField] private Transform swordVisual;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.14f;
    [SerializeField] private float dashCooldown = 0.65f;
    [SerializeField] private float ghostInterval = 0.045f;
    [SerializeField] private float ghostLifetime = 0.18f;
    [SerializeField] private Color ghostColor = new Color(0.45f, 0.85f, 1f, 0.7f);

    private Rigidbody2D body;
    private PlayerController movement;
    private SpriteRenderer playerRenderer;
    private SpriteRenderer swordRenderer;
    private float nextAttackTime, nextDashTime;
    private bool isDashing, isAttacking;
    private float facingSign = 1f;
    private bool isSwordUp = true;
    private bool swordActive = true;  // controlled by ActiveWeapon / ActiveInventory

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerController>();
        playerRenderer = GetComponent<SpriteRenderer>();

        if (!hitbox) hitbox = GetComponent<MeleeHitbox>();
        if (!slashRenderer && transform.Find("SlashEffect")) slashRenderer = transform.Find("SlashEffect").GetComponent<SpriteRenderer>();
        if (!swordVisual && transform.Find("SwordVisual")) swordVisual = transform.Find("SwordVisual");
        if (swordVisual) swordRenderer = swordVisual.GetComponent<SpriteRenderer>();

        if (swordRenderer) swordRenderer.enabled = true;
        if (slashRenderer) slashRenderer.enabled = false;

        isSwordUp = initialSwordUp;
        UpdateFacing();
        UpdateSwordRestPose();
    }

    private void Update()
    {
        UpdateFacing();
        if (!isAttacking && swordActive) UpdateSwordRestPose();

        // Dash input stays in PlayerCombat; attack input is routed via ActiveWeapon.
        if ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current    != null && Mouse.current.rightButton.wasPressedThisFrame))
        {
            TryDash();
        }
    }

    private void UpdateFacing()
    {
        if (playerRenderer) facingSign = playerRenderer.flipX ? -1f : 1f;
    }

    private Vector2 GetCurrentHandOffset(bool upPose)
    {
        Vector2 baseOffset = upPose ? handOffsetUp : handOffsetDown;
        return new Vector2(baseOffset.x * facingSign, baseOffset.y);
    }

    private void UpdateSwordRestPose()
    {
        if (!swordVisual) return;

        // Position sword at the distinct hand anchor position (Up vs Down)
        swordVisual.localPosition = GetCurrentHandOffset(isSwordUp);

        // Keep local scale positive
        Vector3 scale = swordVisual.localScale;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);
        swordVisual.localScale = scale;

        // Calculate rest angle based on whether sword is UP or DOWN
        float forwardAngle = facingSign > 0 ? 0f : 180f;
        float offset = isSwordUp ? swordUpAngle : swordDownAngle;
        float angle = forwardAngle + facingSign * offset;
        swordVisual.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Always render sword in front of player and background so it is never hidden
        if (swordRenderer && playerRenderer)
        {
            swordRenderer.enabled = true;
            swordRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            swordRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }
    }

    private void TryAttack()
    {
        if (isAttacking || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;
        StartCoroutine(SwordSwing());
    }

    private IEnumerator SwordSwing()
    {
        isAttacking = true;
        bool isDownSwing = isSwordUp;

        if (swordRenderer && playerRenderer)
        {
            swordRenderer.enabled = true;
            swordRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            swordRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }

        if (slashRenderer && playerRenderer)
        {
            slashRenderer.enabled = true;
            slashRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            slashRenderer.sortingOrder = playerRenderer.sortingOrder + 2;
            slashRenderer.transform.localPosition = new Vector3(slashOffset.x * facingSign, slashOffset.y, 0f);
            slashRenderer.flipX = facingSign < 0;
            slashRenderer.flipY = !isDownSwing; // Flip vertically when swinging up
            slashRenderer.transform.localRotation = Quaternion.identity;
        }

        float forwardAngle = facingSign > 0 ? 0f : 180f;
        float startOffset = isDownSwing ? swordUpAngle : swordDownAngle;
        float endOffset = isDownSwing ? swordDownAngle : swordUpAngle;

        Vector2 startHand = isDownSwing ? handOffsetUp : handOffsetDown;
        Vector2 endHand = isDownSwing ? handOffsetDown : handOffsetUp;

        float elapsed = 0f;
        bool hitDone = false;

        while (elapsed < slashDuration)
        {
            float t = elapsed / slashDuration;

            // Interpolate sword rotation through forward arc
            float currentOffset = Mathf.Lerp(startOffset, endOffset, t);
            float angle = forwardAngle + facingSign * currentOffset;

            // Interpolate hand position from start hand to end hand, with arm extension
            Vector2 hand = Vector2.Lerp(startHand, endHand, t);
            hand.x = (hand.x + Mathf.Sin(t * Mathf.PI) * swingForwardExtension) * facingSign;

            if (swordVisual)
            {
                swordVisual.localPosition = new Vector3(hand.x, hand.y, 0f);
                swordVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Animate slash frames
            if (slashRenderer && slashFrames != null && slashFrames.Length > 0)
            {
                int frame = Mathf.Clamp(Mathf.FloorToInt(t * slashFrames.Length), 0, slashFrames.Length - 1);
                slashRenderer.sprite = slashFrames[frame];
            }

            // Trigger hitbox strike at hitDelay
            if (!hitDone && elapsed >= hitDelay)
            {
                if (hitbox) hitbox.Strike(new Vector2(facingSign, 0f));
                hitDone = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Toggle sword position for next alternating attack
        isSwordUp = !isDownSwing;

        if (slashRenderer) slashRenderer.enabled = false;
        UpdateSwordRestPose();
        isAttacking = false;
    }

    private void TryDash()
    {
        if (isDashing || Time.time < nextDashTime) return;
        nextDashTime = Time.time + dashCooldown;
        Vector2 direction = movement != null && movement.CurrentMovement.sqrMagnitude > 0.01f
            ? movement.CurrentMovement.normalized
            : new Vector2(facingSign, 0f);
        StartCoroutine(Dash(direction));
    }

    private IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        if (movement) movement.SetDashLocked(true);
        float elapsed = 0f, ghostTimer = 0f;
        while (elapsed < dashDuration)
        {
            body.linearVelocity = direction.normalized * dashSpeed;
            ghostTimer += Time.fixedDeltaTime;
            if (ghostTimer >= ghostInterval)
            {
                ghostTimer = 0f;
                SpawnDashGhost();
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        body.linearVelocity = Vector2.zero;
        if (movement) movement.SetDashLocked(false);
        isDashing = false;
    }

    private void SpawnDashGhost()
    {
        if (!playerRenderer || !playerRenderer.sprite) return;
        GameObject ghost = new GameObject("DashAfterimage");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.lossyScale;
        SpriteRenderer r = ghost.AddComponent<SpriteRenderer>();
        r.sprite = playerRenderer.sprite;
        r.flipX = playerRenderer.flipX;
        r.flipY = playerRenderer.flipY;
        r.sortingLayerID = playerRenderer.sortingLayerID;
        r.sortingOrder = playerRenderer.sortingOrder - 1;
        r.color = ghostColor;
        Destroy(ghost, ghostLifetime);
    }
}

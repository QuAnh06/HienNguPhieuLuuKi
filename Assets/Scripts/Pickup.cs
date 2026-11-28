using System.Collections;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { Gold, Heart }

    [SerializeField] private PickupType pickupType = PickupType.Gold;
    [SerializeField] private int amount = 1;
    [SerializeField] private float pickUpDistance = 2.5f;
    [SerializeField] private float accelartionRate = 1.2f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float maxSpeed = 4.5f;
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float heightY = 1.0f;
    [SerializeField] private float magnetDelay = 0.5f;
    [SerializeField] private float lifetime = 20f;
    [SerializeField] private AnimationCurve animCurve;

    private Vector3 moveDir;
    private Rigidbody2D rb;
    private float currentSpeed;
    private float spawnTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentSpeed = moveSpeed;
        StartCoroutine(AnimCurveSpawnRoutine());
        Destroy(gameObject, lifetime);
    }

    public void SetAmount(int value) => amount = Mathf.Max(1, value);

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer < magnetDelay) return; // Don't pull during pop arc

        if (PlayerController.Instance == null) return;

        Vector3 playerPos = PlayerController.Instance.transform.position;
        float dist = Vector3.Distance(transform.position, playerPos);

        if (dist < pickUpDistance)
        {
            moveDir = (playerPos - transform.position).normalized;
            currentSpeed = Mathf.Min(currentSpeed + accelartionRate * Time.deltaTime, maxSpeed);
        }
        else
        {
            moveDir = Vector3.zero;
            currentSpeed = moveSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = (Vector2)moveDir * currentSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spawnTimer < 0.2f) return;

        if (other.GetComponentInParent<PlayerController>())
        {
            if (pickupType == PickupType.Gold && EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(amount);
            }
            else if (pickupType == PickupType.Heart && PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.HealPlayer(amount);
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator AnimCurveSpawnRoutine()
    {
        Vector2 startPoint = transform.position;
        Vector2 endPoint = startPoint + new Vector2(Random.Range(-1.2f, 1.2f), Random.Range(-0.8f, 0.8f));

        if (animCurve == null || animCurve.length == 0)
        {
            animCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 2f, 2f),
                new Keyframe(0.5f, 1f, 0f, 0f),
                new Keyframe(1f, 0f, -2f, -2f)
            );
        }

        float timePassed = 0f;
        while (timePassed < popDuration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / popDuration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);

            transform.position = Vector2.Lerp(startPoint, endPoint, linearT) + new Vector2(0f, height);
            yield return null;
        }
    }
}

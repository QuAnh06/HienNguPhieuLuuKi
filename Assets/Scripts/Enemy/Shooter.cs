using System.Collections;
using UnityEngine;

/// <summary>
/// Ranged enemy attack controller. Fires staggered or oscillating bursts of projectiles
/// towards the player within a cone of influence.
/// Implements IEnemy so EnemyAI triggers Attack() during Attacking state.
/// </summary>
public class Shooter : MonoBehaviour, IEnemy
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletMoveSpeed = 4.5f;

    [Header("Burst Settings")]
    [SerializeField] private int burstCount = 2;
    [SerializeField] private int projectilesPerBurst = 8;
    [SerializeField, Range(0, 359)] private float angleSpread = 70f;
    [SerializeField] private float startingDistance = 0.25f;
    [SerializeField] private float timeBetweenBursts = 0.8f;
    [SerializeField] private float restTime = 1.5f;
    [SerializeField] private bool stagger = true;
    [Tooltip("Stagger must be enabled for oscillate to function properly.")]
    [SerializeField] private bool oscillate = true;

    private bool isShooting = false;

    private void OnValidate()
    {
        if (oscillate) { stagger = true; }
        if (projectilesPerBurst < 1) { projectilesPerBurst = 1; }
        if (burstCount < 1) { burstCount = 1; }
        if (timeBetweenBursts < 0.1f) { timeBetweenBursts = 0.1f; }
        if (restTime < 0.1f) { restTime = 0.1f; }
        if (startingDistance < 0.05f) { startingDistance = 0.05f; }
        if (bulletMoveSpeed <= 0) { bulletMoveSpeed = 0.1f; }
    }

    public void Attack()
    {
        if (!isShooting && gameObject.activeInHierarchy)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;

        float startAngle, currentAngle, angleStep, endAngle;
        float timeBetweenProjectiles = 0f;

        TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out endAngle);

        if (stagger && projectilesPerBurst > 0)
        {
            timeBetweenProjectiles = timeBetweenBursts / projectilesPerBurst;
        }

        for (int i = 0; i < burstCount; i++)
        {
            if (!oscillate)
            {
                TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out endAngle);
            }

            if (oscillate && i % 2 != 1)
            {
                TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out endAngle);
            }
            else if (oscillate)
            {
                currentAngle = endAngle;
                endAngle = startAngle;
                startAngle = currentAngle;
                angleStep *= -1;
            }

            for (int j = 0; j < projectilesPerBurst; j++)
            {
                if (bulletPrefab != null)
                {
                    Vector2 pos = FindBulletSpawnPos(currentAngle);
                    GameObject newBullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
                    newBullet.transform.right = (Vector3)pos - transform.position;

                    if (newBullet.TryGetComponent(out Projectile projectile))
                    {
                        projectile.UpdateMoveSpeed(bulletMoveSpeed);
                    }
                }

                currentAngle += angleStep;

                if (stagger) { yield return new WaitForSeconds(timeBetweenProjectiles); }
            }

            currentAngle = startAngle;

            if (!stagger) { yield return new WaitForSeconds(timeBetweenBursts); }
        }

        yield return new WaitForSeconds(restTime);
        isShooting = false;
    }

    private void TargetConeOfInfluence(out float startAngle, out float currentAngle, out float angleStep, out float endAngle)
    {
        Vector2 targetDirection = Vector2.down;
        if (PlayerController.Instance != null)
        {
            targetDirection = (Vector2)PlayerController.Instance.transform.position - (Vector2)transform.position;
        }

        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        startAngle = targetAngle;
        endAngle = targetAngle;
        currentAngle = targetAngle;
        float halfAngleSpread = 0f;
        angleStep = 0f;

        if (angleSpread != 0 && projectilesPerBurst > 1)
        {
            angleStep = angleSpread / (projectilesPerBurst - 1);
            halfAngleSpread = angleSpread / 2f;
            startAngle = targetAngle - halfAngleSpread;
            endAngle = targetAngle + halfAngleSpread;
            currentAngle = startAngle;
        }
    }

    private Vector2 FindBulletSpawnPos(float currentAngle)
    {
        float x = transform.position.x + startingDistance * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        float y = transform.position.y + startingDistance * Mathf.Sin(currentAngle * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }
}

using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State { Roaming, Chasing, Attacking, Hurt, Dead }

    [Header("Detection & Combat Settings")]
    [SerializeField] private float detectionRange = 4.5f;
    [SerializeField] private float attackRange = 1.1f;
    [SerializeField] private float attackCooldown = 1.6f;
    [SerializeField] private float roamChangeInterval = 2.5f;

    private State state = State.Roaming;
    private EnemyPathfinding pathfinding;
    private IEnemy enemyCombat;
    private Animator animator;
    private bool canAttack = true;
    private Vector2 roamDirection;
    private float roamTimer;

    public State CurrentState => state;

    private void Awake()
    {
        pathfinding = GetComponent<EnemyPathfinding>();
        enemyCombat = GetComponent<IEnemy>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        PickNewRoamDirection();
    }

    private void Update()
    {
        if (state == State.Dead) return;

        float distToPlayer = PlayerController.Instance != null
            ? Vector2.Distance(transform.position, PlayerController.Instance.transform.position)
            : float.MaxValue;

        switch (state)
        {
            case State.Roaming:
                roamTimer += Time.deltaTime;
                if (roamTimer >= roamChangeInterval) PickNewRoamDirection();
                pathfinding.MoveTo(roamDirection);
                UpdateAnimation(true);

                if (distToPlayer <= detectionRange) state = State.Chasing;
                break;

            case State.Chasing:
                if (distToPlayer > detectionRange * 1.4f)
                {
                    state = State.Roaming;
                    PickNewRoamDirection();
                    break;
                }

                if (distToPlayer <= attackRange)
                {
                    state = State.Attacking;
                    break;
                }

                Vector2 dirToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
                pathfinding.MoveTo(dirToPlayer);
                UpdateAnimation(true);
                break;

            case State.Attacking:
                pathfinding.StopMoving();
                UpdateAnimation(false);

                if (canAttack && distToPlayer <= attackRange * 1.3f)
                {
                    StartCoroutine(AttackRoutine());
                }
                else if (distToPlayer > attackRange)
                {
                    state = State.Chasing;
                }
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        if (enemyCombat != null) enemyCombat.Attack();
        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        state = State.Chasing;
    }

    private void PickNewRoamDirection()
    {
        roamTimer = 0f;
        roamDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void UpdateAnimation(bool moving)
    {
        if (animator != null) animator.SetBool("isWalking", moving);
    }

    public void OnHurt()
    {
        if (state == State.Dead) return;
        state = State.Hurt;
        pathfinding.StopMoving();
        if (animator != null) animator.SetTrigger("Hurt");
        StartCoroutine(RecoverFromHurtRoutine());
    }

    private IEnumerator RecoverFromHurtRoutine()
    {
        yield return new WaitForSeconds(0.35f);
        if (state != State.Dead) state = State.Chasing;
    }

    public void OnDeath()
    {
        state = State.Dead;
        pathfinding.StopMoving();
        if (animator != null)
        {
            animator.SetBool("isDead", true);
            animator.SetTrigger("Death");
        }
    }
}

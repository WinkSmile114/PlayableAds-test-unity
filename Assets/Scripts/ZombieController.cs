using System.Collections;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [Header("Settings")]
    public float chaseRange = 6f;
    public float moveSpeed = 2f;
    public float attackRange = 1.3f;
    public int damagePerHit = 10;
    public int zombieHP = 100;

    [Header("References")]
    public Transform player;
    public SpriteRenderer sr;
    [SerializeField] GameObject damageEffectPrefab;

    // Animation Sets
    public Sprite[] idle_N, idle_NE, idle_E, idle_SE, idle_S, idle_SW, idle_W, idle_NW;
    public Sprite[] run_N, run_NE, run_E, run_SE, run_S, run_SW, run_W, run_NW;
    public Sprite[] attack_N, attack_NE, attack_E, attack_SE, attack_S, attack_SW, attack_W, attack_NW;
    public Sprite[] hit_N, hit_NE, hit_E, hit_SE, hit_S, hit_SW, hit_W, hit_NW;
    public Sprite[] die_N, die_NE, die_E, die_SE, die_S, die_SW, die_W, die_NW;

    // ---- internal state ----
    private Sprite[] currentAnim;
    private float animTimer = 0f;
    private int animIndex = 0;

    public bool isDead = false;
    private bool isHit = false;
    private bool isAttacking = false;

    private string lastDirection = "S";

    private bool didHitFrame4 = false;
    private bool didHitFrame8 = false;

    void Start()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || isDead)
        {
            PlayAnimation();
            return;
        }

        if (isHit)
        {
            PlayAnimation();
            return;
        }

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist <= attackRange)
            Attack(dir);
        else if (dist <= chaseRange)
            Move(dir);
        else
            Idle();

        PlayAnimation();
    }

    void Move(Vector3 dir)
    {
        if (isHit) return; 

        isAttacking = false;
        Vector3 moveDir = dir.normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        currentAnim = GetDirectionAnim(
            moveDir,
            run_N, run_NE, run_E, run_SE, run_S, run_SW, run_W, run_NW
        );
    }

    void Idle()
    {
        isAttacking = false;

        currentAnim = GetDirectionAnim(
            GetDirectionVector(lastDirection),
            idle_N, idle_NE, idle_E, idle_SE, idle_S, idle_SW, idle_W, idle_NW
        );
    }
    void Attack(Vector3 dir)
    {
        isAttacking = true;

        currentAnim = GetDirectionAnim(
            dir.normalized,
            attack_N, attack_NE, attack_E, attack_SE, attack_S, attack_SW, attack_W, attack_NW
        );
    }

    void DoAttackDamage()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= attackRange + 0.2f)
        {
            var p = player.GetComponent<PlayerAnimationController>();
            if (p != null)
            {
                p.TakeDamage(damagePerHit);
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        GameObject effect = Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        zombieHP -= dmg;

        if (zombieHP <= 0)
        {
            StopAllCoroutines(); 
            Die();                
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(HitRoutine());
        }
    }


    IEnumerator HitRoutine()
    {
        isHit = true;
        isAttacking = false;

        Sprite[] hitAnim = GetDirectionAnim(
            GetDirectionVector(lastDirection),
            hit_N, hit_NE, hit_E, hit_SE, hit_S, hit_SW, hit_W, hit_NW
        );

        currentAnim = hitAnim;
        animIndex = 0;

        foreach (var frame in hitAnim)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(0.06f);
        }

        isHit = false;
    }
    void Die()
    {
        if (isDead) return;

        isDead = true;
        moveSpeed = 0f;
        isAttacking = false;
        isHit = false;

        transform.position = transform.position;

        currentAnim = GetDirectionAnim(
            GetDirectionVector(lastDirection),
            die_N, die_NE, die_E, die_SE, die_S, die_SW, die_W, die_NW
        );

        animIndex = 0;
        animTimer = 0f;

        FindFirstObjectByType<DropManager>().OnZombieKilled(
            new Vector3(transform.position.x, transform.position.y, -0.5f)
        );

        StartCoroutine(PlayDeathAnimation());
    }


    IEnumerator PlayDeathAnimation()
    {
        while (animIndex < currentAnim.Length)
        {
            sr.sprite = currentAnim[animIndex];
            animIndex++;
            yield return new WaitForSeconds(0.08f);
        }

        animIndex = currentAnim.Length - 1;
        sr.sprite = currentAnim[animIndex];
    }

    void PlayAnimation()
    {
        if (currentAnim == null || currentAnim.Length == 0) return;

        if (isHit)
        {
            sr.sprite = currentAnim[Mathf.Clamp(animIndex, 0, currentAnim.Length - 1)];
            return;
        }

        animTimer += Time.deltaTime;

        if (animTimer > 0.08f)
        {
            animTimer = 0f;
            animIndex++;

            if (isDead && animIndex >= currentAnim.Length)
            {
                animIndex = currentAnim.Length - 1;
                sr.sprite = currentAnim[animIndex];
                return;
            }

            if (animIndex >= currentAnim.Length)
                animIndex = 0;

            if (isAttacking)
            {
                if (animIndex == 3 && !didHitFrame4)
                {
                    DoAttackDamage();
                    didHitFrame4 = true;
                }
                else if (animIndex == 7 && !didHitFrame8)
                {
                    DoAttackDamage();
                    didHitFrame8 = true;
                }
            }

            sr.sprite = currentAnim[animIndex];
        }
    }

    Sprite[] GetDirectionAnim(
        Vector3 dir,
        Sprite[] N, Sprite[] NE, Sprite[] E, Sprite[] SE,
        Sprite[] S, Sprite[] SW, Sprite[] W, Sprite[] NW)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f) { lastDirection = "E"; return E; }
        if (angle >= 22.5f && angle < 67.5f) { lastDirection = "NE"; return NE; }
        if (angle >= 67.5f && angle < 112.5f) { lastDirection = "N"; return N; }
        if (angle >= 112.5f && angle < 157.5f) { lastDirection = "NW"; return NW; }
        if (angle >= 157.5f || angle < -157.5f) { lastDirection = "W"; return W; }
        if (angle >= -157.5f && angle < -112.5f) { lastDirection = "SW"; return SW; }
        if (angle >= -112.5f && angle < -67.5f) { lastDirection = "S"; return S; }
        if (angle >= -67.5f && angle < -22.5f) { lastDirection = "SE"; return SE; }

        lastDirection = "S";
        return S;
    }

    Vector3 GetDirectionVector(string dir)
    {
        switch (dir)
        {
            case "N": return Vector3.up;
            case "NE": return new Vector3(1, 1, 0);
            case "E": return Vector3.right;
            case "SE": return new Vector3(1, -1, 0);
            case "S": return Vector3.down;
            case "SW": return new Vector3(-1, -1, 0);
            case "W": return Vector3.left;
            case "NW": return new Vector3(-1, 1, 0);
        }
        return Vector3.down;
    }
}
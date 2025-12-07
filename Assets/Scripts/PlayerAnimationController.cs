using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Core")]
    public SpriteRenderer sr;
    public float moveSpeed = 4f;
    public int maxHP = 100;
    private int currentHP;
    [SerializeField] FloatingJoystick joystick;
    [SerializeField] Image _healthBar;
    [SerializeField] GameObject shootEffectPrefab;

    [Header("Weapon Settings")]
    public int currentWeapon = 1;  // 1 or 2
    public float animationFPS = 10f;

    private float animTimer = 0f;
    private int animIndex = 0;

    private Vector2 moveDir;
    private string currentState = "Idle";
    private string currentDirection = "SW";
    bool isShooting = false, isHit = false, isDead = false;
    private bool allowAction => !isDead && !isHit && !isShooting;

    [Header("Auto Target Settings")]
    public float autoShootRange = 6f;
    private Transform currentTarget;
    private float shootCooldown = 0.4f;
    private float shootTimer = 0f;

    [Header("Weapon 1 Sprites")]
    public Sprite[] W1_Idle_N, W1_Idle_NE, W1_Idle_E, W1_Idle_SE, W1_Idle_S, W1_Idle_SW, W1_Idle_W, W1_Idle_NW;
    public Sprite[] W1_Run_N, W1_Run_NE, W1_Run_E, W1_Run_SE, W1_Run_S, W1_Run_SW, W1_Run_W, W1_Run_NW;
    public Sprite[] W1_Shoot_N, W1_Shoot_NE, W1_Shoot_E, W1_Shoot_SE, W1_Shoot_S, W1_Shoot_SW, W1_Shoot_W, W1_Shoot_NW;
    public Sprite[] W1_Hit_N, W1_Hit_NE, W1_Hit_E, W1_Hit_SE, W1_Hit_S, W1_Hit_SW, W1_Hit_W, W1_Hit_NW;
    public Sprite[] W1_Die_N, W1_Die_NE, W1_Die_E, W1_Die_SE, W1_Die_S, W1_Die_SW, W1_Die_W, W1_Die_NW;


    [Header("Weapon 2 Sprites")]
    public Sprite[] W2_Idle_N, W2_Idle_NE, W2_Idle_E, W2_Idle_SE, W2_Idle_S, W2_Idle_SW, W2_Idle_W, W2_Idle_NW;
    public Sprite[] W2_Run_N, W2_Run_NE, W2_Run_E, W2_Run_SE, W2_Run_S, W2_Run_SW, W2_Run_W, W2_Run_NW;
    public Sprite[] W2_Shoot_N, W2_Shoot_NE, W2_Shoot_E, W2_Shoot_SE, W2_Shoot_S, W2_Shoot_SW, W2_Shoot_W, W2_Shoot_NW;
    public Sprite[] W2_Hit_N, W2_Hit_NE, W2_Hit_E, W2_Hit_SE, W2_Hit_S, W2_Hit_SW, W2_Hit_W, W2_Hit_NW;
    public Sprite[] W2_Die_N, W2_Die_NE, W2_Die_E, W2_Die_SE, W2_Die_S, W2_Die_SW, W2_Die_W, W2_Die_NW;

    private float targetScanTimer = 0f;
    private float targetScanInterval = 0.2f;


    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        if (isDead)
        {
            PlayAnimation();
            return;
        }

        shootTimer -= Time.deltaTime;

        HandleMovement();


        if (moveDir.magnitude < 0.1f)
        {
            targetScanTimer -= Time.deltaTime;
            if (targetScanTimer <= 0)
            {
                FindTarget();
                targetScanTimer = targetScanInterval;
            }
        }
        else
        {
            currentTarget = null;
        }

        if (currentTarget != null && moveDir.magnitude < 0.1f && allowAction)
        {
            AimAtTarget();

            if (shootTimer <= 0)
            {
                Shoot();
                shootTimer = shootCooldown;
            }
        }

        PlayAnimation();
    }


    void HandleMovement()
    {
        if (isDead) return;
        if (isShooting) return; 

        float h = joystick.Horizontal;
        float v = joystick.Vertical;
        moveDir = new Vector2(h, v).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            if (isShooting)
            {
                isShooting = false;
                currentState = "Run";
            }
            transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;
            currentState = isHit ? "Hit" : "Run";
            currentDirection = GetDirection(moveDir);
        }
        else
        {
            currentState = isHit ? "Hit" : "Idle";
        }
    }

    string GetDirection(Vector2 dir)
    {
        if (dir.y > 0.5f)
        {
            if (dir.x > 0.5f) return "NE";
            if (dir.x < -0.5f) return "NW";
            return "N";
        }
        else if (dir.y < -0.5f)
        {
            if (dir.x > 0.5f) return "SE";
            if (dir.x < -0.5f) return "SW";
            return "S";
        }
        else
        {
            if (dir.x > 0.5f) return "E";
            if (dir.x < -0.5f) return "W";
        }
        return "S";
    }

    Sprite[] GetSpriteArray(string state, string dir)
    {
        string weaponPrefix = currentWeapon == 1 ? "W1_" : "W2_";
        string fieldName = weaponPrefix + state + "_" + dir;

        var field = GetType().GetField(fieldName);
        return field != null ? (Sprite[])field.GetValue(this) : null;
    }

    void PlayAnimation()
    {
        Sprite[] sprites = GetSpriteArray(currentState, currentDirection);
        if (sprites == null || sprites.Length == 0) return;

        animTimer += Time.deltaTime;

        if (animTimer >= 1f / animationFPS)
        {
            animTimer = 0f;
            animIndex++;

            if (currentState == "Die")
            {
                animIndex = Mathf.Clamp(animIndex, 0, sprites.Length - 1);
                sr.sprite = sprites[animIndex];
                StartCoroutine(FindFirstObjectByType<DropManager>().Died());
                return;
            }

            if (currentState == "Hit" && animIndex >= sprites.Length)
            {
                isHit = false;
                currentState = moveDir.magnitude > 0.1f ? "Run" : "Idle";
                animIndex = 0;
                sprites = GetSpriteArray(currentState, currentDirection);
            }

            if (currentState == "Shoot" && animIndex >= sprites.Length)
            {
                isShooting = false;
                currentState = moveDir.magnitude > 0.1f ? "Run" : "Idle";
                animIndex = 0;
                sprites = GetSpriteArray(currentState, currentDirection);
            }

            if (animIndex >= sprites.Length)
                animIndex = 0;
        }

        sr.sprite = sprites[Mathf.Clamp(animIndex, 0, sprites.Length - 1)];
    }
    void FindTarget()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject z in zombies)
        {
            ZombieController zScript = z.GetComponent<ZombieController>();
            if (zScript != null && zScript.isDead == true) continue;

            float dist = Vector2.Distance(transform.position, z.transform.position);
            if (dist < closestDist && dist <= autoShootRange)
            {
                closestDist = dist;
                nearest = z.transform;
            }
        }

        currentTarget = nearest;
    }


    void AimAtTarget()
    {
        if (currentTarget == null) return;

        Vector2 dir = (currentTarget.position - transform.position).normalized;
        currentDirection = GetDirection(dir);
    }

    public void Shoot()
    {
        if (isDead || isHit || isShooting || currentTarget == null) return;

        isShooting = true;
        currentState = "Shoot";
        animIndex = 0;
        animTimer = 0f;

        PlayShootEffect();
        ZombieController z = currentTarget.GetComponent<ZombieController>();
        if (z != null && !z.isDead)
        {
            int weaponDamage = currentWeapon == 1 ? 50 : 100;
            z.TakeDamage(weaponDamage);
        }
    }



    public void PlayShootEffect()
    {
        GameObject effect = Instantiate(shootEffectPrefab, transform.position, Quaternion.identity);

        switch (currentDirection)
        {
            case "N":
                effect.transform.position = new Vector3(0.15f, 1.11f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case "NE":
                effect.transform.position = new Vector3(1f, 0.65f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 0, 45);
                break;
            case "E":
                effect.transform.position = new Vector3(1.1f, 0.02f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case "SE":
                effect.transform.position = new Vector3(0.8f, -0.55f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 0, -30);
                break;
            case "S":
                effect.transform.position = new Vector3(-0.2f, -0.6f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case "SW":
                effect.transform.position = new Vector3(-0.9f, -0.45f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 180, -45);
                break;
            case "W":
                effect.transform.position = new Vector3(-1.1f, 0.22f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 180f, 0);
                break;
            case "NW":
                effect.transform.position = new Vector3(-0.75f, 0.75f, 0) + transform.position;
                effect.transform.rotation = Quaternion.Euler(0, 180f, 30f);
                break;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;

        _healthBar.fillAmount = (float)currentHP / maxHP;
        if (currentHP <= 0)
        {
            Die();
            return;
        }
        if (!isShooting)
        {
            isHit = true;
            currentState = "Hit";
            animIndex = 0;
            animTimer = 0f;
        }
    }



    void Die()
    {
        isDead = true;
        currentState = "Die";
        animIndex = 0;
        animTimer = 0f;
        moveSpeed = 0f;
        isShooting = false;
        isHit = false;
    }

    public void SwitchWeapon(int newWeapon)
    {
        currentWeapon = newWeapon;
    }
}


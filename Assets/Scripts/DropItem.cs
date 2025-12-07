using UnityEngine;
using System.Collections;

public class DropItem : MonoBehaviour
{
    public enum ItemType { Weapon, Money }
    public ItemType itemType;

    public SpriteRenderer sr;
    public float pickupRange = 1.5f;

    private bool isPicked = false;
    private Transform player;

    [Header("Animations")]
    public Sprite[] dropAnim;
    public Sprite[] idleAnim;
    public Sprite[] pickupAnim;

    private int animIndex = 0;
    private float animTimer = 0f;

    private Sprite[] currentAnim;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.transform;

        currentAnim = dropAnim;
        StartCoroutine(PlayDropThenIdle());
    }

    IEnumerator PlayDropThenIdle()
    {
        for (int i = 0; i < dropAnim.Length; i++)
        {
            sr.sprite = dropAnim[i];
            yield return new WaitForSeconds(0.07f);
        }
        if (!isPicked && itemType == ItemType.Money)
        {
            StartCoroutine(Pickup());
        }
        currentAnim = idleAnim;
    }

    void Update()
    {
        if (player == null || isPicked) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= pickupRange)
        {
            if (itemType == ItemType.Money)
            {

            }
            else
            {
                StartCoroutine(Pickup());
            }
        }
        PlayIdleAnimation();
    }

    void PlayIdleAnimation()
    {
        if (currentAnim == null || currentAnim.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= 0.08f)
        {
            animTimer = 0;
            animIndex++;

            if (animIndex >= currentAnim.Length)
            {
                animIndex = 0;
            }

            sr.sprite = currentAnim[animIndex];
        }
    }

    IEnumerator Pickup()
    {
        isPicked = true;

        currentAnim = pickupAnim;
        animIndex = 0;

        // play pickup animation
        for (int i = 0; i < pickupAnim.Length; i++)
        {
            sr.sprite = pickupAnim[i];
            yield return new WaitForSeconds(0.1f);
        }

        // Inform DropManager
        if (itemType == ItemType.Weapon)
        {
            DropManager.instance.UnlockWeapon();
        }
        else
        {
            DropManager.instance.AddMoney(100);
        }

        Destroy(gameObject);
    }
}

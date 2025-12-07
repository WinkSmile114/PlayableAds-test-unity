using UnityEngine;

public class ShootEffect : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite[] effectSprites;
    public float fps = 20f;

    private int index = 0;
    private float timer = 0f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer = 0;
            index++;

            if (index >= effectSprites.Length)
            {
                Destroy(gameObject);
                return;
            }

            sr.sprite = effectSprites[index];
        }
    }
}

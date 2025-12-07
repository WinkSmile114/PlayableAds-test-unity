using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DropManager : MonoBehaviour
{
    public static DropManager instance;
    [SerializeField] Text money;
    [SerializeField] GameObject gamePanel, retryPanel, winPanel;
    [SerializeField] Button retryBtn, playAgainBtn;

    [Header("Prefabs")]
    public GameObject weaponDropPrefab;
    public GameObject moneyDropPrefab;

    private bool weaponDropped = false;

    public int totalMoney = 0;
    public int killCount = 0;

    void Awake()
    {
        Time.timeScale = 1;
        retryBtn.onClick.AddListener(Restart);
        playAgainBtn.onClick.AddListener(Restart);
        instance = this;
    }

    public IEnumerator Died()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        retryPanel.gameObject.SetActive(true);
        gamePanel.SetActive(false);

        Time.timeScale = 0;
    }


    public IEnumerator Win()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        retryPanel.gameObject.SetActive(false);
        gamePanel.SetActive(false);
        winPanel.SetActive(true);

        Time.timeScale = 0;
    }

    void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void OnZombieKilled(Vector3 position)
    {
        killCount++;

        if (!weaponDropped)
        {
            Instantiate(weaponDropPrefab, position, Quaternion.identity);
            weaponDropped = true;
        }
        else
        {
            Instantiate(moneyDropPrefab, position, Quaternion.identity);
        }
    }

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        money.text = "$ " + totalMoney.ToString();
        if (totalMoney > 600)
        {
            StartCoroutine(Win());
        }
    }

    public void UnlockWeapon()
    {
        var player = GameObject.FindWithTag("Player")?.GetComponent<PlayerAnimationController>();
        if (player != null)
            player.SwitchWeapon(2);
    }
}

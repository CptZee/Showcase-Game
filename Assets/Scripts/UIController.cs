using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;

public class UIController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The player controller")]
    private PlayerController playerController;
    [SerializeField]
    [Tooltip("The Game Over Panel")]
    private GameObject gameOverPanel;
    public TextMeshProUGUI text;
    protected TextMeshProUGUI HPText;
    protected TextMeshProUGUI cointText;
    protected CompositeDisposable disposables;

    void Awake()
    {
        HPText = GameObject.Find("HP").GetComponentInChildren<TextMeshProUGUI>();
        cointText = GameObject.Find("Gold").GetComponentInChildren<TextMeshProUGUI>();
        disposables = new CompositeDisposable();
    }
    void Start()
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not set in " + name);
            return;
        }

        if (HPText == null || cointText == null)
        {
            Debug.LogWarning("TextMeshPro not set in " + name);
            return;
        }

        playerController.coins.Subscribe(coins => cointText.text = coins.ToString()).AddTo(disposables);
        playerController.hp.Subscribe(hp =>
        {
            if (hp <= 0)
            {
                StartCoroutine(FadeInPanel());
            }
            else
            {
                HPText.text = hp.ToString() + "/" + playerController.GetComponent<Damageable>().MaxHealth;
            }
        }).AddTo(disposables);

    }

    private IEnumerator FadeInPanel()
    {
        float alpha = 0f;
        Image imagePanel = gameOverPanel.GetComponent<Image>();

        Color panelColor = imagePanel.color;
        panelColor.a = alpha;
        imagePanel.color = panelColor;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime / 2f;
            panelColor.a = alpha;
            gameOverPanel.GetComponent<Image>().color = panelColor;
            yield return null;
        }

        panelColor.a = 1f;
        text.enabled = true;
        Time.timeScale = 0;
        imagePanel.color = panelColor;
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }
}

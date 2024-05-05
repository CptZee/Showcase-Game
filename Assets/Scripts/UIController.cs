using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class UIController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The player controller")]
    private PlayerController playerController;
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
        playerController.hp.Subscribe(hp => HPText.text = hp.ToString() + "/" + playerController.GetComponent<Damageable>().MaxHealth).AddTo(disposables);

    }

    void OnDestroy()
    {
        disposables.Dispose();
    }
}

using UnityEngine;
using UniRx;

public class CoinController : MonoBehaviour, IInteractable
{
    [SerializeField]
    [Tooltip("DEBUG ONLY - DO NOT EDIT")]
    private PlayerController playerController;
    [SerializeField]
    [Tooltip("Amount of coins to give to the player")]
    private int amount = 1;

    protected Animator animator;
    protected CompositeDisposable disposables;

    public float fadeTime = 0.5f;
    public float FadeTime { get => fadeTime; set => fadeTime = value; }

    private float timeElapsed = 0f;
    public float TimeElapsed { get => timeElapsed; set => timeElapsed = value; }

    protected SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    protected GameObject objToRemove;
    public GameObject ObjectToRemove { get => objToRemove; set => objToRemove = value; }

    protected Color startColor;
    public Color StartColor { get => startColor; set => startColor = value; }

    protected bool _hasInteracted = false;
    public bool HasInteracted { get => _hasInteracted; set => _hasInteracted = value; }

    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        animator = GetComponent<Animator>();
        disposables = new CompositeDisposable();
    }

    public void OnInteract()
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not found in scene");
            return;
        }
        //Makes sure that there are no repeatition of giving coins. E.G, if the coin is still being destroyed (fading)
        if (HasInteracted)
            return;
        playerController.GiveCoins(amount);
        HasInteracted = true;
        timeElapsed = 0f;
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        objToRemove = animator.gameObject;

        Observable.EveryFixedUpdate()
            .Subscribe(_ =>
            {
                timeElapsed += Time.deltaTime;
                float newAlpha = startColor.a * (1 - timeElapsed / fadeTime);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

                if (timeElapsed > fadeTime)
                    Destroy(objToRemove);
            })
            .AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }
}

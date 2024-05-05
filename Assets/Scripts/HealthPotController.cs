using UnityEngine;
using UniRx;

public class HealthPotController : MonoBehaviour, IInteractable
{
    [SerializeField]
    [Tooltip("The amount of health the potion will heal the player")]
    private float healAmount = 20f;
    protected Animator animator;
    protected CompositeDisposable disposables;
    protected PlayerController playerController;

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
        if (HasInteracted)
            return;
        HasInteracted = true;
        
        HasInteracted = true;
        timeElapsed = 0f;
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        objToRemove = animator.gameObject;

        playerController.GetComponent<IDamageable>().Heal(healAmount);

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

using UnityEngine;
using UniRx;
public class BombController : MonoBehaviour, IInteractable
{
    [SerializeField]
    [Tooltip("The amount of damage the bomb will deal to the player")]
    private float damageAmount = 10f;
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
        public bool HasExploded
    {
        get
        {
            return animator.GetBool(StaticStrings.hasExploded);
        }
    }

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
        objToRemove = animator.gameObject;

        animator.SetTrigger(StaticStrings.explodeTrigger);
        playerController.CanMove = false;
        
        Debug.Log("Player hit by bomb");
        playerController.GetComponent<IDamageable>().TakeDamage(damageAmount, new Vector2(4, 2));

        Observable.Timer(System.TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                playerController.CanMove = true; 
            }).AddTo(disposables);

        Observable.EveryFixedUpdate()
            .Where(_ => HasExploded)
            .Subscribe(_ =>
            {
                Destroy(objToRemove);
            }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }
}

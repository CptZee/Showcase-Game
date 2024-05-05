using UnityEngine;
public interface IInteractable
{
    float FadeTime { get; set; }
    float TimeElapsed { get; set; }
    SpriteRenderer SpriteRenderer { get; }
    GameObject ObjectToRemove { get; set; }
    Color StartColor { get; set; }
    void OnInteract();
}
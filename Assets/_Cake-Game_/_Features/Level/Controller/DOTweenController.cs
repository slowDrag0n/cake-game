using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[System.Serializable]
public class DOTweenEvent
{
    public UnityEvent onStart;
    public UnityEvent onComplete;
    public UnityEvent onKill;
    public UnityEvent onUpdate;
}

public class DOTweenController : MonoBehaviour
{
    // Enum to select the type of tween in the Inspector
    public enum TweenType { Move, Rotate, Scale, LocalMove }

    [System.Serializable]
    public class TweenAction
    {
        public string TweenDesc;
        public TweenType tweenType;
        public float duration = 1f;
        public float delay = 0f;
        public Vector3 targetPosition;
        public Vector3 targetRotation;
        public Vector3 targetScale = Vector3.one;
        public Ease easeType = Ease.Linear;
        public LoopType loopType = LoopType.Restart;
        public int loops = 0;
        public DOTweenEvent tweenEvents;
    }

    public Transform targetTransform;

    // Tween actions array to hold multiple tweens
    public TweenAction[] tweenActions;

    // Event handlers exposed to Inspector


    [SerializeField] private int currentTweenIndex = 0; // Track the current tween
    private Tween currentTween;

    void Start()
    {
        // Start the first tween
        PlayNextTween();
    }
    [ContextMenu("PlayTween")]
    void PlayNextTween()
    {
        if(currentTweenIndex >= tweenActions.Length)
        {
            return; // No more tweens to play
        }

        TweenAction action = tweenActions[currentTweenIndex];

        switch(action.tweenType)
        {
            case TweenType.Move:
                currentTween = targetTransform.DOMove(action.targetPosition, action.duration)
                    .SetDelay(action.delay)
                    .SetEase(action.easeType)
                    .SetLoops(action.loops, action.loopType);
                break;

            case TweenType.Rotate:
                currentTween = targetTransform.DORotate(action.targetRotation, action.duration)
                    .SetDelay(action.delay)
                    .SetEase(action.easeType)
                    .SetLoops(action.loops, action.loopType);
                break;

            case TweenType.Scale:
                currentTween = targetTransform.DOScale(action.targetScale, action.duration)
                    .SetDelay(action.delay)
                    .SetEase(action.easeType)
                    .SetLoops(action.loops, action.loopType);
                break;
            case TweenType.LocalMove:
                currentTween = targetTransform.DOLocalMove(action.targetPosition, action.duration)
                   .SetDelay(action.delay)
                   .SetEase(action.easeType)
                   .SetLoops(action.loops, action.loopType);
                break;
        }

        // Apply event handlers
        currentTween.OnStart(() => action.tweenEvents.onStart?.Invoke())
                    .OnComplete(() => action.tweenEvents.onComplete?.Invoke())
                    .OnKill(() => action.tweenEvents.onKill?.Invoke())
                    .OnUpdate(() => action.tweenEvents.onUpdate?.Invoke());

        currentTween.Play(); // Start the tween
    }

    // This method is called when a tween completes
    void OnTweenComplete()
    {
        tweenActions[currentTweenIndex].tweenEvents.onComplete?.Invoke();

        // Move to the next tween in the array
        currentTweenIndex++;
        Debug.Log($"Current{currentTweenIndex}");
        // Optionally, call PlayNextTween here automatically, or control it externally
        // Uncomment the line below if you want the next tween to play automatically
        // PlayNextTween();
    }

    // This method can be called to manually trigger the next tween
    public void TriggerNextTween()
    {
        KillCurrentTween();
        currentTweenIndex++;
        Debug.Log($"TriggerNextTween");
        PlayNextTween();
    }

    public void PlayCurrentTween()
    {

        currentTween?.Play();
    }

    public void PauseCurrentTween()
    {
        currentTween?.Pause();
    }

    public void KillCurrentTween()
    {
        currentTween?.Kill();
    }

    public void RestartCurrentTween()
    {
        currentTween?.Restart();
    }


    public void UpdateTweenIndex(int value)
    {
        currentTweenIndex += value;
    }
    private void OnDestroy()
    {
        if(currentTween != null)
        {
            currentTween.Kill();
        }
    }
}


//switch (selectedTween)
//{
//    case TweenType.Move:
//        targetTransform.position = new Vector3(StartPosition.x, StartPosition.y, StartPosition.z);
//        break;
//    case TweenType.Rotate:
//        targetTransform.rotation = new Quaternion(StartRotation.x, StartRotation.y, StartRotation.z, transform.rotation.w);
//        break;
//    case TweenType.Scale:
//        targetTransform.localScale = new Vector3(StartScale.x, StartScale.y, StartScale.z);
//        break;
//}

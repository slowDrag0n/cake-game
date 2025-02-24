using DG.Tweening;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

public class InputAnimationController : MonoBehaviour
{
    public Animator Controller;
    [Tooltip("Make sure to set this state's speed to 0 in animator.")]
    public string AnimationStateName = "";
    public float ProgressSpeed = .05f;
    [Space]
    public UnityEvent OnStart;
    public float OnCompleteDelay;
    public UnityEvent OnComplete;

    bool _hasStarted;
    bool _isAnimating;
    float _animatingProgress;

    private void Start()
    {
        ResetProgress();
    }

    public void Update()
    {
        if(_isAnimating == false)
            return;

        _animatingProgress += Time.deltaTime * ProgressSpeed;
        _animatingProgress = Mathf.Clamp01(_animatingProgress);

        Controller.Play(AnimationStateName, 0, _animatingProgress);

        if(_animatingProgress == 1f)
        {
            _isAnimating = false;

            DOVirtual.DelayedCall(OnCompleteDelay, delegate { OnComplete?.Invoke(); });
        }
    }

    public void StartMixing()
    {
        if(_animatingProgress >= 1f)
            return;

        _isAnimating = true;

        if(!_hasStarted)
        {
            OnStart?.Invoke(); _hasStarted = true;
        }
    }

    public void StopMixing()
    {
        _isAnimating = false;
    }

    public void ResetProgress()
    {
        _isAnimating = false;
        _animatingProgress = 0;
    }
}

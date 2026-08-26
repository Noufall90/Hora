using System.Collections;
using UnityEngine;

public class TalkingAnimation : MonoBehaviour
{
    [Header("Animator Component")]
    [SerializeField] private Animator animator;

    [Header("Animation Timing Settings")]
    [SerializeField] private float idleDuration = 10f;
    [SerializeField] private float talkingDuration = 2f;

    [Header("Animator Parameter Names")]
    [SerializeField] private string isTalkingTriggerName = "IsTalking";
    [SerializeField] private string isIdleBoolName = "IsIdle";

    private Coroutine talkingCoroutine;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
        }
        talkingCoroutine = StartCoroutine(TalkingRoutine());
    }

    private void OnDisable()
    {
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
        }
    }

    private IEnumerator TalkingRoutine()
    {
        while (true)
        {
            if (animator != null)
            {
                animator.SetBool(isIdleBoolName, true);
            }

            yield return new WaitForSeconds(idleDuration);

            if (animator != null)
            {
                animator.SetBool(isIdleBoolName, false);
                animator.SetTrigger(isTalkingTriggerName);
            }

            yield return new WaitForSeconds(talkingDuration);

            if (animator != null)
            {
                animator.SetBool(isIdleBoolName, true);
            }
        }
    }
}
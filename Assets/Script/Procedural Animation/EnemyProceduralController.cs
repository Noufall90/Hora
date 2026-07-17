using UnityEngine;

namespace procedural_animation
{
    [RequireComponent(typeof(EnemyProceduralAnimator))]
    public class EnemyProceduralController : MonoBehaviour
    {
        private EnemyProceduralAnimator _animator;
        private void Awake()
        {
            _animator = GetComponent<EnemyProceduralAnimator>();
        }
        public bool IsMoving => _animator.IsMoving;
        public void SetLookTarget(Transform target) => _animator.SetLookTarget(target);
        public void ClearLookTarget() => _animator.ClearLookTarget();
    }
}

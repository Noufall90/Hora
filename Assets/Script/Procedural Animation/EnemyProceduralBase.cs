using UnityEngine;

namespace procedural_animation
{
    public abstract class EnemyProceduralBase : MonoBehaviour
    {
        [Header("Global")]
        [SerializeField] protected LayerMask _groundLayerMask = default;

        [Header("Debug")]
        [SerializeField] protected bool _showDebugRays = true;
        [SerializeField] protected float _debugDuration = 0.1f;

        public abstract bool IsMoving { get; }
        public abstract void SetLookTarget(Transform target);
        public abstract void ClearLookTarget();
        protected abstract void Initialize();
        protected abstract void Tick();
        protected virtual void Start() => Initialize();
        protected virtual void FixedUpdate() => Tick();
    }
}

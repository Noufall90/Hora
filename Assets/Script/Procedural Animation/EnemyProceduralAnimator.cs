using System.Collections;
using UnityEngine;

namespace procedural_animation
{
    public class EnemyProceduralAnimator : EnemyProceduralBase
    {

        private class ProceduralLimb
        {
            public Transform IKTarget;
            public Vector3 defaultPosition;
            public Vector3 lastPosition;
            public bool moving;
        }

        [Header("Steps")]
        [SerializeField] private Transform[] _limbTargets;
        [SerializeField] private float _stepSize = 2f;
        [SerializeField] private float _stepHeight = 0.5f;
        [SerializeField] private int _smoothness = 2;
        [SerializeField] private float _raycastRange = 1f;
        [SerializeField] private float _feetOffset = 0.05f;
        [SerializeField] private float _stepLeadMultiplier = 5f;

        [Header("Gait Control")]
        [Tooltip("Tentukan indeks kaki mana saja yang tidak boleh melangkah bersamaan. Contoh: Kiri Depan tidak boleh bareng Kanan Belakang.")]
        [SerializeField] private int[] _gaitPairings;

        [Header("Look At")]
        [SerializeField] private Transform _lookTargetIK;
        [SerializeField] private float _lookSpeed = 5f;
        [SerializeField] private string _lookAtTag = "Player";

        private int _nLimbs;
        private ProceduralLimb[] _limbs;

        private Vector3 _lastBodyPosition;
        private Vector3 _velocity;
        private bool _allLimbsResting;

        private Transform _currentTarget;
        public override bool IsMoving => !_allLimbsResting;
        public override void SetLookTarget(Transform target) => _currentTarget = target;
        public override void ClearLookTarget() => _currentTarget = null;

        protected override void Initialize()
        {
            _nLimbs = _limbTargets.Length;
            _limbs = new ProceduralLimb[_nLimbs];

            for (int i = 0; i < _nLimbs; i++)
            {
                Transform t = _limbTargets[i];

                _limbs[i] = new ProceduralLimb()
                {
                    IKTarget = t,
                    defaultPosition = transform.InverseTransformPoint(t.position),
                    lastPosition = t.position,
                    moving = false
                };
            }

            // Default pairing jika tidak diisi di Inspector (Asumsi 4 kaki)
            if (_gaitPairings == null || _gaitPairings.Length != _nLimbs)
            {
                _gaitPairings = new int[] { 3, 2, 1, 0 };
            }

            _lastBodyPosition = transform.position;
            _allLimbsResting = true;

            // Auto-find look target by tag
            GameObject taggedTarget = GameObject.FindGameObjectWithTag(_lookAtTag);
            if (taggedTarget != null)
                _currentTarget = taggedTarget.transform;
        }

        // ─── Lifecycle: Tick (FixedUpdate) ───────────────────────────────────────

        protected override void Tick()
        {
            _velocity = transform.position - _lastBodyPosition;

            if (_velocity.magnitude > 0.001f)
            {
                _HandleMovement();
            }
            else if (!_allLimbsResting)
            {
                _BackToRestPosition();
            }
        }

        // ─── Lifecycle: Update (Look At) ─────────────────────────────────────────

        protected virtual void Update()
        {
            if (_currentTarget != null && _lookTargetIK != null)
            {
                _lookTargetIK.position = Vector3.Lerp(
                    _lookTargetIK.position,
                    _currentTarget.position,
                    Time.deltaTime * _lookSpeed);
            }
        }

        // ─── Gait / Stepping Logic ───────────────────────────────────────────────

        private void _HandleMovement()
        {
            _lastBodyPosition = transform.position;

            float greatestDistance = _stepSize;
            int limbToMove = -1;

            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i].moving)
                    continue;

                int partnerIndex = _gaitPairings[i];
                if (partnerIndex < _nLimbs && _limbs[partnerIndex].moving)
                    continue;

                Vector3 desiredPosition = transform.TransformPoint(_limbs[i].defaultPosition);
                Vector3 predictedPos = desiredPosition + (_velocity * _stepLeadMultiplier);
                float dist = (predictedPos - _limbs[i].lastPosition).magnitude;

                if (dist > greatestDistance)
                {
                    greatestDistance = dist;
                    limbToMove = i;
                }
            }

            for (int i = 0; i < _nLimbs; i++)
            {
                if (i != limbToMove && !_limbs[i].moving)
                    _limbs[i].IKTarget.position = _limbs[i].lastPosition;
            }

            if (limbToMove != -1)
            {
                Vector3 baseTarget = transform.TransformPoint(_limbs[limbToMove].defaultPosition);
                Vector3 targetPoint = baseTarget + (_velocity * _stepLeadMultiplier);

                targetPoint = _RaycastToGround(targetPoint, transform.up);
                targetPoint += transform.up * _feetOffset;

#if UNITY_EDITOR
                if (_showDebugRays)
                {
                    Debug.DrawLine(
                        _limbs[limbToMove].lastPosition,
                        targetPoint,
                        Color.cyan,
                        _debugDuration);
                }
#endif

                _allLimbsResting = false;
                StartCoroutine(_Stepping(limbToMove, targetPoint));
            }
        }

        private void _BackToRestPosition()
        {
            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i].moving)
                    continue;

                Vector3 targetPoint =
                    _RaycastToGround(
                        transform.TransformPoint(_limbs[i].defaultPosition),
                        transform.up)
                    + transform.up * _feetOffset;

                float dist = (targetPoint - _limbs[i].lastPosition).magnitude;

                if (dist > 0.005f)
                {
                    StartCoroutine(_Stepping(i, targetPoint));
                    return;
                }
            }

            _allLimbsResting = true;
        }

        private Vector3 _RaycastToGround(Vector3 pos, Vector3 up)
        {
            Vector3 point = pos;
            Vector3 rayOrigin = pos + (_raycastRange * up);
            Vector3 rayDirection = -up;

            Ray ray = new Ray(rayOrigin, rayDirection);

            if (Physics.Raycast(ray, out RaycastHit hit, 2f * _raycastRange, _groundLayerMask))
            {
                point = hit.point;

#if UNITY_EDITOR
                if (_showDebugRays)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.green, _debugDuration);
                    Debug.DrawLine(hit.point, hit.point + hit.normal * 0.2f, Color.blue, _debugDuration);
                }
#endif
            }

            return point;
        }

        private IEnumerator _Stepping(int limbIdx, Vector3 targetPosition)
        {
            _limbs[limbIdx].moving = true;
            Vector3 startPosition = _limbs[limbIdx].lastPosition;

            for (int i = 1; i <= _smoothness; i++)
            {
                float t = i / (_smoothness + 1f);

                _limbs[limbIdx].IKTarget.position =
                    Vector3.Lerp(startPosition, targetPosition, t)
                    + transform.up * Mathf.Sin(t * Mathf.PI) * _stepHeight;

                yield return new WaitForFixedUpdate();
            }

            _limbs[limbIdx].IKTarget.position = targetPosition;
            _limbs[limbIdx].lastPosition = targetPosition;
            _limbs[limbIdx].moving = false;
        }
    }
}

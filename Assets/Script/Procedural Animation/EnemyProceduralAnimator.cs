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
        [Tooltip("Tentukan indeks kaki mana saja yang tidak boleh melangkah bersamaan.")]
        [SerializeField] private int[] _gaitPairings;

        [Header("Look At & Detection (LOS)")]
        [SerializeField] private Transform _lookTargetIK;
        [SerializeField] private float _lookSpeed = 5f;
        [SerializeField] private float _scanSpeed = 2f;
        [SerializeField] private float _scanAngle = 80f;
        [SerializeField] private float _viewDistance = 15f;
        [Range(0, 360)]
        [SerializeField] private float _fovAngle = 120f;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private LayerMask _obstacleLayer;

        private int _nLimbs;
        private ProceduralLimb[] _limbs;
        private Vector3 _lastBodyPosition;
        private Vector3 _velocity;
        private bool _allLimbsResting;
        private Transform _currentTarget;
        private bool _playerDetected;
        private float _scanTimer;
        private bool _isAttacking;
        private Enemy.EnemyBrain _brain;

        public bool PlayerDetected => _playerDetected;
        public override bool IsMoving => !_allLimbsResting;
        public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }

        public void SetAttacking(bool attacking, Transform target = null)
        {
            _isAttacking = attacking;
            if (target != null)
            {
                _currentTarget = target;
            }
        }

        public override void SetLookTarget(Transform target) => _currentTarget = target;
        public override void ClearLookTarget() => _currentTarget = null;

        protected override void Initialize()
        {
            _brain = GetComponentInParent<Enemy.EnemyBrain>() ?? GetComponent<Enemy.EnemyBrain>();
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

            if (_gaitPairings == null || _gaitPairings.Length != _nLimbs)
            {
                _gaitPairings = new int[] { 3, 2, 1, 0 };
            }

            _lastBodyPosition = transform.position;
            _allLimbsResting = true;
        }

        protected override void Tick()
        {
            _velocity = transform.position - _lastBodyPosition;

            bool isBodyMovingOrRotating = _velocity.magnitude > 0.001f;

            // Cek apakah ada kaki yang terlalu jauh karena rotasi badan
            if (!isBodyMovingOrRotating)
            {
                for (int i = 0; i < _nLimbs; i++)
                {
                    if (!_limbs[i].moving)
                    {
                        Vector3 desiredPosition = transform.TransformPoint(_limbs[i].defaultPosition);
                        if (Vector3.Distance(desiredPosition, _limbs[i].lastPosition) > _stepSize)
                        {
                            isBodyMovingOrRotating = true;
                            break;
                        }
                    }
                }
            }

            if (isBodyMovingOrRotating)
            {
                _HandleMovement();
            }
            else if (!_allLimbsResting)
            {
                _BackToRestPosition();
            }

            // Kunci posisi kaki yang diam agar tidak bergeser mengikuti badan
            for (int i = 0; i < _nLimbs; i++)
            {
                if (!_limbs[i].moving)
                {
                    _limbs[i].IKTarget.position = _limbs[i].lastPosition;
                }
            }

            _CheckLineOfSight();
        }

        protected virtual void Update()
        {
            if (_lookTargetIK == null) return;

            Transform targetToLook = _currentTarget;
            if (targetToLook == null && _brain != null)
            {
                targetToLook = _brain.PlayerTarget;
            }

            if ((_isAttacking || _playerDetected) && targetToLook != null)
            {
                _lookTargetIK.position = Vector3.Lerp(
                    _lookTargetIK.position,
                    targetToLook.position,
                    Time.deltaTime * _lookSpeed);
            }
            else
            {
                _scanTimer += Time.deltaTime * _scanSpeed;
                float currentAngle = Mathf.Sin(_scanTimer) * _scanAngle;
                
                Vector3 scanDirection = Quaternion.AngleAxis(currentAngle, transform.up) * transform.forward;
                
                Vector3 targetScanPos = transform.position + (transform.up * 1f) + (scanDirection * 3f);

                _lookTargetIK.position = Vector3.Lerp(
                    _lookTargetIK.position,
                    targetScanPos,
                    Time.deltaTime * _lookSpeed);
            }
        }

        private void _CheckLineOfSight()
        {
            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, _viewDistance, _playerLayer);

            if (targetsInRadius.Length > 0)
            {
                Transform target = targetsInRadius[0].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dirToTarget) < _fovAngle / 2f)
                {
                    float dstToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position + transform.up * 1f, dirToTarget, dstToTarget, _obstacleLayer))
                    {
                        _playerDetected = true;
                        _currentTarget = target;
                        return;
                    }
                }
            }

            _playerDetected = false;
            _currentTarget = null;
        }

        private void _HandleMovement()
        {
            _lastBodyPosition = transform.position;

            float greatestDistance = _stepSize;
            int limbToMove = -1;

            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i].moving) continue;

                int partnerIndex = _gaitPairings[i];
                if (partnerIndex < _nLimbs && _limbs[partnerIndex].moving) continue;

                Vector3 desiredPosition = transform.TransformPoint(_limbs[i].defaultPosition);
                Vector3 predictedPos = desiredPosition + (_velocity * _stepLeadMultiplier);
                float dist = (predictedPos - _limbs[i].lastPosition).magnitude;

                if (dist > greatestDistance)
                {
                    greatestDistance = dist;
                    limbToMove = i;
                }
            }

            // Pinning posisi dipindah ke Tick() agar selalu dieksekusi

            if (limbToMove != -1)
            {
                Vector3 baseTarget = transform.TransformPoint(_limbs[limbToMove].defaultPosition);
                Vector3 targetPoint = baseTarget + (_velocity * _stepLeadMultiplier);

                targetPoint = _RaycastToGround(targetPoint, transform.up);
                targetPoint += transform.up * _feetOffset;

                _allLimbsResting = false;
                StartCoroutine(_Stepping(limbToMove, targetPoint));
            }
        }

        private void _BackToRestPosition()
        {
            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i].moving) continue;

                Vector3 targetPoint = _RaycastToGround(transform.TransformPoint(_limbs[i].defaultPosition), transform.up) + transform.up * _feetOffset;
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
                _limbs[limbIdx].IKTarget.position = Vector3.Lerp(startPosition, targetPosition, t) + transform.up * Mathf.Sin(t * Mathf.PI) * _stepHeight;
                yield return new WaitForFixedUpdate();
            }

            _limbs[limbIdx].IKTarget.position = targetPosition;
            _limbs[limbIdx].lastPosition = targetPosition;
            _limbs[limbIdx].moving = false;
        }

        private Vector3 _DirFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegrees += transform.eulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugRays) return;

            Vector3 eyePos = transform.position + transform.up * 1f;

            Gizmos.color = Color.yellow;
            Vector3 viewAngleA = _DirFromAngle(-_fovAngle / 2f, false);
            Vector3 viewAngleB = _DirFromAngle(_fovAngle / 2f, false);

            Gizmos.DrawLine(eyePos, eyePos + viewAngleA * _viewDistance);
            Gizmos.DrawLine(eyePos, eyePos + viewAngleB * _viewDistance);

            int segments = 20;
            Vector3 lastPoint = eyePos + _DirFromAngle(-_fovAngle / 2f, false) * _viewDistance;
            for (int i = 1; i <= segments; i++)
            {
                float stepAngle = (-_fovAngle / 2f) + (_fovAngle / segments) * i;
                Vector3 nextPoint = eyePos + _DirFromAngle(stepAngle, false) * _viewDistance;
                Gizmos.DrawLine(lastPoint, nextPoint);
                lastPoint = nextPoint;
            }

            if (_lookTargetIK != null)
            {
                Gizmos.color = _playerDetected ? Color.green : Color.red;
                Gizmos.DrawLine(eyePos, _lookTargetIK.position);
                Gizmos.DrawWireSphere(_lookTargetIK.position, 0.15f);
            }
        }
    }
}
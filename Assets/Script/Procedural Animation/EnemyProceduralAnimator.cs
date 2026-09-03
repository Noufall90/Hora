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
            public Vector3 currentStepPosition;
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

        private LayerMask ObstacleLayer => _brain != null ? _brain.ObstacleLayer : default;

        private int _nLimbs;
        private ProceduralLimb[] _limbs;
        private Vector3 _lastBodyPosition;
        private Vector3 _rawVelocity;
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
            _nLimbs = _limbTargets != null ? _limbTargets.Length : 0;
            _limbs = new ProceduralLimb[_nLimbs];

            for (int i = 0; i < _nLimbs; i++)
            {
                Transform t = _limbTargets[i];
                if (t == null) continue;

                Vector3 initialPos = t.position;
                Vector3 groundedPos = _RaycastToGround(initialPos, transform.up) + (transform.up * _feetOffset);

                _limbs[i] = new ProceduralLimb()
                {
                    IKTarget = t,
                    defaultPosition = transform.InverseTransformPoint(initialPos),
                    lastPosition = groundedPos,
                    currentStepPosition = groundedPos,
                    moving = false
                };

                t.position = groundedPos;
            }

            if (_gaitPairings == null || _gaitPairings.Length != _nLimbs)
            {
                _gaitPairings = new int[] { 3, 2, 1, 0 };
            }

            _lastBodyPosition = transform.position;
            _rawVelocity = Vector3.zero;
            _velocity = Vector3.zero;
            _allLimbsResting = true;
        }

        protected override void Tick()
        {
            float dt = Time.fixedDeltaTime;
            if (dt <= 0.0001f) dt = 0.02f;

            if (_brain != null && _brain.HasActiveNavMeshAgent && _brain.Agent.velocity.sqrMagnitude > 0.001f)
            {
                _rawVelocity = _brain.Agent.velocity;
            }
            else
            {
                _rawVelocity = (transform.position - _lastBodyPosition) / dt;
            }

            // Smooth velocity dengan low-pass filter untuk menghilangkan spike framerate
            _velocity = Vector3.Lerp(_velocity, _rawVelocity, 1f - Mathf.Exp(-15f * dt));
            _lastBodyPosition = transform.position;

            float speed = _velocity.magnitude;
            bool isBodyMoving = speed > 0.05f;

            // Cek apakah ada kaki yang terlalu jauh karena rotasi badan atau posisi tertinggal
            bool needsStepDueToDistance = false;
            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i] == null || _limbs[i].moving) continue;

                Vector3 desiredPosition = transform.TransformPoint(_limbs[i].defaultPosition);
                if (Vector3.Distance(desiredPosition, _limbs[i].lastPosition) > _stepSize)
                {
                    needsStepDueToDistance = true;
                    break;
                }
            }

            if (isBodyMoving || needsStepDueToDistance)
            {
                _HandleMovement();
            }
            else if (!_allLimbsResting)
            {
                _BackToRestPosition();
            }

            _CheckLineOfSight();
        }

        protected virtual void Update()
        {
            if (_lookTargetIK == null) return;

            Transform targetToLook = _currentTarget;
            if (targetToLook == null && _brain != null && _brain.IsPlayerDetected())
            {
                targetToLook = _brain.PlayerTarget;
            }

            bool isTargeting = (_isAttacking || _playerDetected || _currentTarget != null || (_brain != null && _brain.IsPlayerDetected())) && targetToLook != null;

            if (isTargeting)
            {
                _lookTargetIK.position = Vector3.Lerp(
                    _lookTargetIK.position,
                    targetToLook.position + Vector3.up * 1f,
                    Time.deltaTime * _lookSpeed);
            }
            else if (_brain != null && _brain.IsInvestigating)
            {
                Vector3 targetPos = _brain.LastKnownPlayerPosition;
                _lookTargetIK.position = Vector3.Lerp(
                    _lookTargetIK.position,
                    targetPos + Vector3.up * 1f,
                    Time.deltaTime * _lookSpeed);
            }
            else if (_brain != null && _brain.HasActiveNavMeshAgent && _brain.Agent.hasPath && _brain.Agent.remainingDistance > 0.3f)
            {
                Vector3 moveTargetPos = _brain.Agent.destination + Vector3.up * 1f;
                _lookTargetIK.position = Vector3.Lerp(
                    _lookTargetIK.position,
                    moveTargetPos,
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

        protected virtual void LateUpdate()
        {
            if (_limbs == null) return;

            // Kunci posisi kaki di LateUpdate agar tidak bergeser mengikuti pergerakan transform di Update
            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i] == null || _limbs[i].IKTarget == null) continue;

                if (_limbs[i].moving)
                {
                    _limbs[i].IKTarget.position = _limbs[i].currentStepPosition;
                }
                else
                {
                    _limbs[i].IKTarget.position = _limbs[i].lastPosition;
                }
            }
        }

        private void _CheckLineOfSight()
        {
            Transform target = null;
            if (_brain != null && _brain.PlayerTarget != null)
            {
                target = _brain.PlayerTarget;
            }
            else
            {
                LayerMask searchMask = _playerLayer.value != 0 ? _playerLayer : Physics.DefaultRaycastLayers;
                Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, _viewDistance, searchMask);
                if (targetsInRadius != null && targetsInRadius.Length > 0)
                {
                    target = targetsInRadius[0].transform;
                }
            }

            if (target != null)
            {
                Vector3 eyePos = transform.position + transform.up * 1f;
                Vector3 targetEyePos = target.position + Vector3.up * 1f;
                Vector3 dirToTarget = (targetEyePos - eyePos).normalized;
                float dstToTarget = Vector3.Distance(eyePos, targetEyePos);

                if (dstToTarget <= _viewDistance)
                {
                    if (Vector3.Angle(transform.forward, dirToTarget) <= _fovAngle * 0.5f)
                    {
                        LayerMask obsMask = ObstacleLayer;
                        if (obsMask.value == 0 || !Physics.Raycast(eyePos, dirToTarget, dstToTarget, obsMask))
                        {
                            _playerDetected = true;
                            _currentTarget = target;
                            return;
                        }
                    }
                }
            }

            _playerDetected = false;
            _currentTarget = null;
        }

        private void _HandleMovement()
        {
            float greatestDistance = _stepSize;
            int limbToMove = -1;

            // Perhitungan lead time foot prediction berbasis detik yang independen dari framerate
            float leadTime = Mathf.Max(0.02f, _stepLeadMultiplier * 0.03f);
            Vector3 lead = _velocity * leadTime;

            // Batasi jarak lead agar kaki tidak terlempar terlalu jauh saat terjadi lonjakan kecepatan sesaat
            float maxLead = _stepSize * 0.75f;
            if (lead.sqrMagnitude > maxLead * maxLead)
            {
                lead = lead.normalized * maxLead;
            }

            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i] == null || _limbs[i].moving) continue;

                int partnerIndex = (_gaitPairings != null && i < _gaitPairings.Length) ? _gaitPairings[i] : -1;
                if (partnerIndex >= 0 && partnerIndex < _nLimbs && _limbs[partnerIndex] != null && _limbs[partnerIndex].moving)
                {
                    continue;
                }

                Vector3 desiredPosition = transform.TransformPoint(_limbs[i].defaultPosition);
                Vector3 predictedPos = desiredPosition + lead;
                float dist = Vector3.Distance(predictedPos, _limbs[i].lastPosition);

                if (dist > greatestDistance)
                {
                    greatestDistance = dist;
                    limbToMove = i;
                }
            }

            if (limbToMove != -1)
            {
                Vector3 baseTarget = transform.TransformPoint(_limbs[limbToMove].defaultPosition);
                Vector3 targetPoint = baseTarget + lead;

                targetPoint = _RaycastToGround(targetPoint, transform.up);
                targetPoint += transform.up * _feetOffset;

                _allLimbsResting = false;
                StartCoroutine(_Stepping(limbToMove, targetPoint));
            }
        }

        private void _BackToRestPosition()
        {
            // Ambang batas aman untuk menghindari getaran mikro saat diam
            float restThreshold = Mathf.Max(0.15f, _stepSize * 0.2f);

            for (int i = 0; i < _nLimbs; i++)
            {
                if (_limbs[i] == null || _limbs[i].moving) continue;

                int partnerIndex = (_gaitPairings != null && i < _gaitPairings.Length) ? _gaitPairings[i] : -1;
                if (partnerIndex >= 0 && partnerIndex < _nLimbs && _limbs[partnerIndex] != null && _limbs[partnerIndex].moving)
                {
                    continue;
                }

                Vector3 desiredRestWorld = transform.TransformPoint(_limbs[i].defaultPosition);
                Vector3 targetPoint = _RaycastToGround(desiredRestWorld, transform.up) + (transform.up * _feetOffset);
                float dist = Vector3.Distance(targetPoint, _limbs[i].lastPosition);

                if (dist > restThreshold)
                {
                    StartCoroutine(_Stepping(i, targetPoint));
                    return;
                }
            }
            _allLimbsResting = true;
        }

        private Vector3 _RaycastToGround(Vector3 pos, Vector3 up)
        {
            LayerMask mask = _groundLayerMask.value != 0 ? _groundLayerMask : Physics.DefaultRaycastLayers;

            float upOffset = Mathf.Max(_raycastRange * 2f, 2f);
            float totalDistance = upOffset + Mathf.Max(_raycastRange * 2f, 2.5f);

            Vector3 rayOrigin = pos + (up * upOffset);
            Vector3 rayDirection = -up;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, totalDistance, mask))
            {
                return hit.point;
            }

            // Fallback: Vertical raycast dari ketinggian badan jika raycast sudut utama meleset
            Vector3 fallbackOrigin = new Vector3(pos.x, transform.position.y + 1f, pos.z);
            if (Physics.Raycast(fallbackOrigin, Vector3.down, out RaycastHit fallbackHit, 6f, mask))
            {
                return fallbackHit.point;
            }

            return pos;
        }

        private IEnumerator _Stepping(int limbIdx, Vector3 initialTargetPosition)
        {
            _limbs[limbIdx].moving = true;
            Vector3 startPosition = _limbs[limbIdx].lastPosition;

            // Durasi langkah mulus berbasis waktu nyata (detik)
            float stepDuration = Mathf.Clamp(_smoothness * 0.04f + 0.06f, 0.12f, 0.25f);
            float speed = _velocity.magnitude;
            if (speed > 2f)
            {
                stepDuration = Mathf.Max(0.10f, stepDuration * (2f / speed));
            }

            float elapsed = 0f;
            Vector3 currentLandingPos = initialTargetPosition;

            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // Tracking target dinamis selama kaki melayang agar mendarat di depan badan yang sedang bergerak maju
                float leadTime = Mathf.Max(0.02f, _stepLeadMultiplier * 0.03f);
                Vector3 dynamicLead = _velocity * leadTime;
                float maxLead = _stepSize * 0.75f;
                if (dynamicLead.sqrMagnitude > maxLead * maxLead)
                {
                    dynamicLead = dynamicLead.normalized * maxLead;
                }

                Vector3 dynamicDesired = transform.TransformPoint(_limbs[limbIdx].defaultPosition) + dynamicLead;
                Vector3 dynamicGround = _RaycastToGround(dynamicDesired, transform.up) + (transform.up * _feetOffset);

                currentLandingPos = Vector3.Lerp(initialTargetPosition, dynamicGround, smoothT);

                Vector3 horizontalPos = Vector3.Lerp(startPosition, currentLandingPos, smoothT);
                Vector3 arcOffset = transform.up * (Mathf.Sin(t * Mathf.PI) * _stepHeight);

                Vector3 stepPos = horizontalPos + arcOffset;
                _limbs[limbIdx].currentStepPosition = stepPos;
                _limbs[limbIdx].IKTarget.position = stepPos;

                yield return null;
            }

            // Pendaratan akhir
            float finalLeadTime = Mathf.Max(0.02f, _stepLeadMultiplier * 0.03f);
            Vector3 finalLead = _velocity * finalLeadTime;
            float finalMaxLead = _stepSize * 0.75f;
            if (finalLead.sqrMagnitude > finalMaxLead * finalMaxLead)
            {
                finalLead = finalLead.normalized * finalMaxLead;
            }

            Vector3 finalDesired = transform.TransformPoint(_limbs[limbIdx].defaultPosition) + finalLead;
            Vector3 finalGroundPos = _RaycastToGround(finalDesired, transform.up) + (transform.up * _feetOffset);

            _limbs[limbIdx].IKTarget.position = finalGroundPos;
            _limbs[limbIdx].lastPosition = finalGroundPos;
            _limbs[limbIdx].currentStepPosition = finalGroundPos;
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
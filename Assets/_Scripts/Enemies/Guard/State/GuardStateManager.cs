using _Scripts.Enemies.ViewTypes;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Guard.State
{
    public class EnemyStateManager : MonoBehaviour
    {
        public IEnemyState PatrollingState { get; private set; }
        public IEnemyState DetectingState { get; private set; }
        public IEnemyState AggroState { get; private set; }
        public IEnemyState SearchingState { get; private set; }
        public IEnemyState ReturningState { get; private set; }
        public IEnemyState StunnedState { get; private set; }
        public IEnemyState DisabledState { get; private set; }
        public IEnemyState CurrentState { get; private set; }
        public IEnemyState PreviousState { get; private set; }
        
        [SerializeField] private GuardState enumState;

        [HideInInspector] public EnemySettings Settings;
        [HideInInspector] public Rigidbody2D Rigidbody2D;
        [HideInInspector] public Collider2D Collider2D;
        [HideInInspector] public IViewType[] ViewTypes;

        [Header("Gizmo Settings")]
        [SerializeField] private Color patrolPathColor = Color.green;
        [SerializeField] private float patrolPointRadius = 0.1f;

        [HideInInspector] public LayerMask environmentLayer;
        [HideInInspector] public LayerMask enemyLayer;
        [HideInInspector] public LayerMask playerLayer;
        
        [Header("Sin Values")]
        public int sinPenalty;

        private void Awake()
        {
            Settings = GetComponent<EnemySettings>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Collider2D = GetComponent<Collider2D>();
            ViewTypes = GetComponents<IViewType>();
            
            environmentLayer = LayerMask.GetMask("Environment");
            playerLayer = LayerMask.GetMask("Enemy");
            playerLayer = LayerMask.GetMask("Player");

            PatrollingState = new GuardPatrollingState(this, transform.position ,Settings.leftPatrolDistance, Settings.rightPatrolDistance);
            DetectingState = new GuardDetectingState();
            AggroState = new GuardAggroState();
            SearchingState = new GuardSearchingState();
            ReturningState = new GuardReturningState();
            StunnedState = new GuardStunnedState();
            DisabledState = new GuardDisabledState();

            // Set the initial state
            CurrentState = PatrollingState;
            CurrentState.EnterState(this);
        }

        private void Update()
        {
            // Handle ground detection and gravity in every state
            Settings.HandleGroundDetection();
            Settings.HandleGravity();

            // Update the current state via its UpdateState function
            CurrentState.UpdateState();
            
            // TODO: Eventually the EnemyState ENUM can be removed entirely if we want
            // ENUM State is currently ONLY for debugging in inspector
            if (IsPatrollingState())
            {
                enumState = GuardState.Patrolling;
            }
            else if (IsDetectingState())
            {
                enumState = GuardState.Detecting;
            }
            else if (IsAggroState())
            {
                enumState = GuardState.Aggro;
            }
            else if (IsSearchingState())
            {
                enumState = GuardState.Searching;
            }
            else if (IsReturningState())
            {
                enumState = GuardState.Returning;
            }
            else if (IsStunnedState())
            {
                enumState = GuardState.Stunned;
            }
            else if (IsDisabledState())
            {
                enumState = GuardState.Disabled;
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            CurrentState.OnCollisionEnter2D(col);
        }

        private void OnCollisionStay2D(Collision2D col)
        {
            CurrentState.OnCollisionStay2D(col);
        }

        public void TransitionToState(IEnemyState newState)
        {
            if (CurrentState == newState)
                return;
            
            // Disable state trap
            if (CurrentState == DisabledState)
            {
                Debug.Log("Tried to exit from DisabledState");
                return;
            }

            // Should immediately go to search state after being stunned, unless being disabled.
            if (CurrentState == StunnedState && (newState != SearchingState && newState != DisabledState))
            {
                Debug.Log("Tried to exit stunned to bad state");
                return;
            }

            PreviousState = CurrentState;
            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState(this);
        }

        public void Move(Vector2 direction, float speed)
        {
            Rigidbody2D.velocity = new Vector2(direction.x * speed, Rigidbody2D.velocity.y);
            // Rigidbody2D.velocity = Vector2.Lerp(transform.position, new Vector2(direction.x * speed, Rigidbody2D.velocity.y), Time.deltaTime);
        }

        public void StopMoving()
        {
            Rigidbody2D.velocity = new Vector2(0, Rigidbody2D.velocity.y);
        }

        public void StopFalling()
        {
            Rigidbody2D.velocity = new Vector2(Rigidbody2D.velocity.x, 0f); 
        }

        public bool IsPlayerDetected()
        {
            foreach (var viewType in ViewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame())
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool IsPlayerDetectedWithQuickDetect()
        {
            foreach (var viewType in ViewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame())
                {
                    if (viewType.QuickDetection())
                    {
                        return true;
                    }
                }
            }
            return false;
        } 
        
        private void OnDrawGizmos()
        {
            if (PatrollingState != null)
            {
                var patrolState = PatrollingState as GuardPatrollingState;
                if (patrolState != null)
                {
                    // Get patrol points
                    Vector2 originPosition = patrolState.OriginPosition;
                    Vector2 leftPoint = patrolState.LeftPatrolPoint();
                    Vector2 rightPoint = patrolState.RightPatrolPoint();

                    // If not in play mode, use the transform position as origin
                    if (!Application.isPlaying)
                    {
                        originPosition = transform.position;
                        leftPoint = originPosition + Vector2.left * Settings.leftPatrolDistance;
                        rightPoint = originPosition + Vector2.right * Settings.rightPatrolDistance;
                    }

                    // Draw patrol path
                    Gizmos.color = patrolPathColor;
                    Gizmos.DrawLine(leftPoint, rightPoint);

                    // Draw patrol points
                    Gizmos.DrawSphere(leftPoint, patrolPointRadius);
                    Gizmos.DrawSphere(rightPoint, patrolPointRadius);
                }
            }
        }
        
        public void KillEnemy()
        {

            PlayerVariables.Instance.CommitSin(sinPenalty);
            TransitionToState(this.DisabledState);
        }

        #region State Getters
        public bool IsPatrollingState()
        {
            return CurrentState is GuardPatrollingState;
        }
        public bool IsDetectingState()
        {
            return CurrentState is GuardDetectingState;
        }
        public bool IsAggroState()
        {
            return CurrentState is GuardAggroState;
        }
        public bool IsSearchingState()
        {
            return CurrentState is GuardSearchingState;
        }
        public bool IsReturningState()
        {
            return CurrentState is GuardReturningState;
        }
        public bool IsStunnedState()
        {
            return CurrentState is GuardStunnedState;
        }
        public bool IsDisabledState()
        {
            return CurrentState is GuardDisabledState;
        }
        
        // Alerted is not an actual state but is used to denote an increase in view radius / distance
        public bool IsAlertedState()
        {
            return CurrentState is GuardAggroState || CurrentState is GuardSearchingState;
        }
        #endregion
    }
}
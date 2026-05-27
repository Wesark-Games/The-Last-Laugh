using UnityEngine;

namespace Project.Combat
{
    public enum EnemyState { Idle, Chasing, Attacking }

    public class EnemyCore : MonoBehaviour
    {
        [SerializeField] private EnemyState currentState = EnemyState.Idle;
        public EnemyState CurrentState => currentState;
        public Transform Target { get; private set; }

        private DetectionModule _detection;
        private MovementModule  _movement;
        private ShootingModule  _shooting;

        private void Awake()
        {
            _detection = GetComponent<DetectionModule>();
            _movement  = GetComponent<MovementModule>();
            _shooting  = GetComponent<ShootingModule>();

            _detection?.Init(this);
            _movement?.Init(this);
            _shooting?.Init(this);
        }

        private void Update()
        {
            _detection?.UpdateModule();
            _shooting?.UpdateModule();
        }

        public void SetTarget(Transform target) => Target = target;
        public void SetState(EnemyState state)  => currentState = state;
    }
}

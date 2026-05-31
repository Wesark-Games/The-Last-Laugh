using UnityEngine;

namespace Project.Combat
{
    public enum EnemyState { Idle, Chasing, Attacking }

    public class EnemyCore : MonoBehaviour
    {
        [SerializeField] private EnemyState currentState = EnemyState.Idle;
        public EnemyState CurrentState => currentState;
        public Transform Target { get; private set; }

        private EnemyModule[] _modules;

        private void Awake()
        {
            // Находим и инициализируем ВСЕ модули на враге
            _modules = GetComponents<EnemyModule>();
            foreach (var module in _modules)
                module.Init(this);
        }

        private void Update()
        {
            // Вызываем UpdateModule у тех модулей, что используют его
            // (Detection, Shooting). Movement и Patrol работают через FixedUpdate.
            if (_modules == null) return;
            foreach (var module in _modules)
                if (module != null) module.UpdateModule();
        }

        public void SetTarget(Transform target) => Target = target;
        public void SetState(EnemyState state)  => currentState = state;
    }
}

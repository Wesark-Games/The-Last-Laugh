using UnityEngine;

namespace Project.Combat
{
    public abstract class EnemyModule : MonoBehaviour
    {
        protected EnemyCore Core { get; private set; }

        public virtual void Init(EnemyCore core)
        {
            Core = core;
        }

        public abstract void UpdateModule();
    }
}

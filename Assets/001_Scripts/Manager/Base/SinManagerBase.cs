using UnityEngine;

namespace _001_Scripts.Manager.Base
{
    public class SinManagerBase<T> : GameBehaviour where T : Component
    {
        public static T instance { get; private set; }
        
        protected virtual void Awake()
        {
            if (instance != null)
            {
                if (instance != this as T)
                {
                    Destroy(gameObject);
                }
                return;
            }

            instance = this as T;
            
            DontDestroyOnLoad(gameObject);
        }
    }
}
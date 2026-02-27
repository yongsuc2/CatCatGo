using UnityEngine;
using CatCatGo.Services;

namespace CatCatGo.Presentation.Core
{
    public abstract class BaseScreen : MonoBehaviour
    {
        protected GameManager Game => GameManager.Instance;
        protected UIManager UI => UIManager.Instance;

        public virtual void OnScreenEnter()
        {
            Refresh();
        }

        public virtual void OnScreenExit() { }

        public abstract void Refresh();
    }
}

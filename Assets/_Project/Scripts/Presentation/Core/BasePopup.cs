using UnityEngine;

namespace CatCatGo.Presentation.Core
{
    public class BasePopup : MonoBehaviour
    {
        protected object PopupData;

        public virtual void Show(object data = null)
        {
            PopupData = data;
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            UIManager.Instance.ClosePopup();
        }
    }
}

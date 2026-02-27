using System.Collections.Generic;
using UnityEngine;

namespace CatCatGo.Presentation.Battle
{
    public class DamagePopupPool : MonoBehaviour
    {
        private readonly Queue<DamagePopup> _pool = new Queue<DamagePopup>();

        public DamagePopup Get(Transform parent)
        {
            DamagePopup popup;
            if (_pool.Count > 0)
            {
                popup = _pool.Dequeue();
                popup.transform.SetParent(parent, false);
            }
            else
            {
                var go = new GameObject("DamagePopup");
                go.transform.SetParent(parent, false);
                var rt = go.GetComponent<RectTransform>();

                if (rt == null) rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(120f, 30f);
                popup = go.AddComponent<DamagePopup>();
                popup.OnAnimationComplete = Return;
            }
            popup.gameObject.SetActive(true);
            return popup;
        }

        public void Return(DamagePopup popup)
        {
            if (popup == null) return;
            popup.gameObject.SetActive(false);
            popup.transform.SetParent(transform, false);
            _pool.Enqueue(popup);
        }

        public void ReturnAll()
        {
            var active = GetComponentsInChildren<DamagePopup>(true);
            foreach (var popup in active)
            {
                popup.ForceStop();
                if (!_pool.Contains(popup))
                {
                    popup.transform.SetParent(transform, false);
                    _pool.Enqueue(popup);
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using CatCatGo.Services;

namespace CatCatGo.Presentation.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            EnsureGameManager();
            EnsureSpriteManager();
            EnsureEventSystem();
            EnsureUIManager();
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        private void EnsureSpriteManager()
        {
            if (SpriteManager.Instance != null) return;

            var go = new GameObject("SpriteManager");
            DontDestroyOnLoad(go);
            go.AddComponent<SpriteManager>();
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private void EnsureUIManager()
        {
            if (UIManager.Instance != null) return;

            var go = new GameObject("UIManager");
            DontDestroyOnLoad(go);
            go.AddComponent<UIManager>();
        }
    }
}

using System;
using System.Collections.Generic;

namespace CatCatGo.Infrastructure
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type].Remove(handler);
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type)) return;

            foreach (var handler in _handlers[type].ToArray())
                ((Action<T>)handler)(eventData);
        }

        public static void Clear()
        {
            _handlers.Clear();
        }
    }
}

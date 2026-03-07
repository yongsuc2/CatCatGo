using System.Collections.Generic;

namespace CatCatGo.Network
{
    public static class ErrorCodeMessages
    {
        private static readonly Dictionary<string, string> _messages = new Dictionary<string, string>
        {
            { "INSUFFICIENT_GOLD", "\uace8\ub4dc\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4" },
            { "INSUFFICIENT_GEMS", "\ubcf4\uc11d\uc774 \ubd80\uc871\ud569\ub2c8\ub2e4" },
            { "INSUFFICIENT_EQUIPMENT_STONE", "\uac15\ud654\uc11d\uc774 \ubd80\uc871\ud569\ub2c8\ub2e4" },
            { "INSUFFICIENT_PET_EGG", "\ud3ab \uc54c\uc774 \ubd80\uc871\ud569\ub2c8\ub2e4" },
            { "INSUFFICIENT_PET_FOOD", "\ud3ab \uba39\uc774\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4" },
            { "INSUFFICIENT_STAMINA", "\uc2a4\ud0dc\ubbf8\ub098\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4" },
            { "EQUIPMENT_NOT_FOUND", "\uc7a5\ube44\ub97c \ucc3e\uc744 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4" },
            { "PET_NOT_FOUND", "\ud3ab\uc744 \ucc3e\uc744 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4" },
            { "NO_AVAILABLE_SLOT", "\ube48 \uc2ac\ub86f\uc774 \uc5c6\uc2b5\ub2c8\ub2e4" },
            { "SESSION_ALREADY_ACTIVE", "\uc774\ubbf8 \uc9c4\ud589 \uc911\uc778 \uc138\uc158\uc774 \uc788\uc2b5\ub2c8\ub2e4" },
            { "NO_ACTIVE_SESSION", "\uc9c4\ud589 \uc911\uc778 \uc138\uc158\uc774 \uc5c6\uc2b5\ub2c8\ub2e4" },
            { "MAX_SUB_LEVEL_REACHED", "\ucd5c\ub300 \ub808\ubca8\uc5d0 \ub3c4\ub2ec\ud588\uc2b5\ub2c8\ub2e4" },
            { "ALREADY_CLAIMED", "\uc774\ubbf8 \uc218\ub839\ud588\uc2b5\ub2c8\ub2e4" },
            { "LEVEL_NOT_REACHED", "\ub808\ubca8\uc774 \ubd80\uc871\ud569\ub2c8\ub2e4" },
        };

        public static string GetMessage(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode)) return null;
            return _messages.TryGetValue(errorCode, out var msg) ? msg : null;
        }

        public static string GetMessageOrDefault(string errorCode, string defaultMessage = "\uc77c\uc2dc\uc801\uc778 \uc624\ub958\uac00 \ubc1c\uc0dd\ud588\uc2b5\ub2c8\ub2e4")
        {
            return GetMessage(errorCode) ?? defaultMessage;
        }
    }
}

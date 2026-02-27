using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[InitializeOnLoad]
public static class GameViewPortraitSetter
{
    static GameViewPortraitSetter()
    {
        EditorApplication.delayCall += SetPortraitResolution;
    }

    [MenuItem("CatCatGo/Set Game View 1080x1920")]
    static void SetPortraitResolution()
    {
        try
        {
            var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gameViewType == null) return;

            var window = EditorWindow.GetWindow(gameViewType);
            if (window == null) return;

            var sizesType = Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
            if (sizesType == null) return;

            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            var sizesInstance = instanceProp.GetValue(null);

            var getGroupMethod = sizesType.GetMethod("GetGroup");
            var currentGroupTypeProp = sizesType.GetProperty("currentGroupType");
            var currentGroupType = currentGroupTypeProp.GetValue(sizesInstance);
            var group = getGroupMethod.Invoke(sizesInstance, new object[] { currentGroupType });

            var groupType = group.GetType();
            var getTotalCount = groupType.GetMethod("GetTotalCount");
            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            int totalCount = (int)getTotalCount.Invoke(group, null);

            var gameViewSizeType = Type.GetType("UnityEditor.GameViewSize,UnityEditor");
            var widthProp = gameViewSizeType.GetProperty("width");
            var heightProp = gameViewSizeType.GetProperty("height");

            for (int i = 0; i < totalCount; i++)
            {
                var size = getGameViewSize.Invoke(group, new object[] { i });
                int w = (int)widthProp.GetValue(size);
                int h = (int)heightProp.GetValue(size);
                if (w == 1080 && h == 1920)
                {
                    SetSelectedSizeIndex(gameViewType, window, i);
                    return;
                }
            }

            var gameViewSizeTypeEnum = Type.GetType("UnityEditor.GameViewSizeType,UnityEditor");
            var fixedResolution = Enum.Parse(gameViewSizeTypeEnum, "FixedResolution");
            var ctor = gameViewSizeType.GetConstructor(new Type[] { gameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) });
            var newSize = ctor.Invoke(new object[] { fixedResolution, 1080, 1920, "CatCatGo (1080x1920)" });

            var addCustomSize = groupType.GetMethod("AddCustomSize");
            addCustomSize.Invoke(group, new object[] { newSize });

            totalCount = (int)getTotalCount.Invoke(group, null);
            SetSelectedSizeIndex(gameViewType, window, totalCount - 1);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CatCatGo] Failed to set Game View resolution: {e.Message}");
        }
    }

    static void SetSelectedSizeIndex(Type gameViewType, EditorWindow window, int index)
    {
        var prop = gameViewType.GetProperty("selectedSizeIndex",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
            prop.SetValue(window, index);
            window.Repaint();
        }
    }
}

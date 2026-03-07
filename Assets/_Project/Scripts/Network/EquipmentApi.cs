using System;
using System.Collections.Generic;

namespace CatCatGo.Network
{
    public static class EquipmentApi
    {
        public static void Upgrade(string equipmentId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/equipment/upgrade", new { equipmentId }, callback);
        }

        public static void Equip(string equipmentId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/equipment/equip", new { equipmentId }, callback);
        }

        public static void Unequip(string slotType, int slotIndex, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/equipment/unequip", new { slotType, slotIndex }, callback);
        }

        public static void Sell(string equipmentId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/equipment/sell", new { equipmentId }, callback);
        }

        public static void Forge(List<string> equipmentIds, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/equipment/forge", new { equipmentIds }, callback);
        }

        public static void BulkForge(Action<ApiResponse<ServerResponse<BulkForgeResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/equipment/bulk-forge", new { }, callback);
        }
    }
}

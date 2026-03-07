namespace CatCatGo.Infrastructure
{
    public struct ResourcesChangedEvent { }
    public struct TalentChangedEvent { }
    public struct HeritageChangedEvent { }
    public struct EquipmentChangedEvent { }
    public struct InventoryChangedEvent { }
    public struct PetChangedEvent { }
    public struct ChapterStateChangedEvent { }
    public struct TowerChangedEvent { }
    public struct DungeonChangedEvent { }
    public struct CatacombChangedEvent { }
    public struct QuestChangedEvent { }
    public struct AttendanceChangedEvent { }
    public struct PlayerStatsChangedEvent { }

    public struct NetworkModeChangedEvent
    {
        public NetworkMode Mode;
    }

    public enum NetworkMode
    {
        ONLINE,
        OFFLINE
    }
}

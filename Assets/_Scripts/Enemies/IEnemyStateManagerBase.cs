namespace _Scripts.Enemies
{
    public interface IEnemyStateManagerBase
    {
        void KillEnemy();
        // Shared States between different enemy types
        bool IsPatrollingState();
        bool IsDetectingState();
        bool IsAlertedState();
        bool IsInvestigatingState();
        bool IsDisabledState();
        
        // For alternate enemy types just return false

        // Guard-Specific
        bool IsAggroState(); // Guard & Bat
        bool IsStunnedState();
        
        // Sniper Specific
        bool IsChargingState();
        bool IsReloadingState();
    }
}
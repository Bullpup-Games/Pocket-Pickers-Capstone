namespace _Scripts.Enemies.AggroTypes
{
    public interface IAggroType
    {
        /*
         * Movement during aggro state
         * Default: Move towards player or last known position.
         * Bat: nothing, return
         * Sharpshooter: TBD
         */
        void Movement();
        /*
         * Action performed during aggro state
         * Default: Grab player when in range
         * Bat: Screech to alert other guards of intruder
         * Sharpshooter: Prime and fire shot
         */
        void Action();
    }
}
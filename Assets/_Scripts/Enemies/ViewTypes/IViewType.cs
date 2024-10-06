using System;

namespace _Scripts.Enemies
{
    /*
     * Interface to handle different types of guard views
     */
    public interface IViewType
    {
        // Event called when the player is sighted
        event Action<bool> PlayerDetected;

        /*
         * Event called when the player is out of view
         * This may seem unnecessary but is an easy way to tell when the player has left an enemies sight,
         * and handle the state change accordingly
         */
        event Action NoPlayerDetected;

        /*
         * Create the enemy-type specific cast
         * (Cone, sphere, ray, etc)
         * Also, apply the modifier defined in GuardSettings.cs
         */
        void SetView();

        /*
         * Event listener function that modifies the necessary variable when the view modifier is updated
         * (angle, radius, length)
         */
        void UpdateView(float modifier);

        bool IsPlayerDetectedThisFrame();
    }
}
using System;

namespace _Scripts.Guards
{
    /*
     * Interface to handle different types of guard views
     */
    public interface IViewType
    {
        event Action PlayerDetected; 

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

    }
}
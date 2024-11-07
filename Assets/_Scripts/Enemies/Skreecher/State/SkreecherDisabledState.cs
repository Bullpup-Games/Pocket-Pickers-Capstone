using UnityEngine;

namespace _Scripts.Enemies.Skreecher.State
{
    public class SkreecherDisabledState : IEnemyState<SkreecherStateManager>
    {
    public void EnterState(SkreecherStateManager enemy) {}

    public void UpdateState() {}

    public void ExitState() {}

    public void OnCollisionEnter2D(Collision2D col) {}

    public void OnCollisionStay2D(Collision2D col) {}  
    }
}
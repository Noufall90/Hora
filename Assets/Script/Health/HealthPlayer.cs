using UnityEngine;

public class PlayerHealth : Health
{
    protected override void Die()
    {
        // Game Over

        base.Die();
    }
}
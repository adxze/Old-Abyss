using UnityEngine;

// This script contains all state machine behaviors needed for the enemy animator

// For the idle state to maintain directional facing
public class EnemyIdleEnterBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the EnemyController component
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            // Force octant parameter to maintain correct direction
            enemy.ForceOctantParameter();
        }
    }
}

// For the attack state
public class EnemyAttackBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the EnemyController component
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            // Force octant parameter to ensure we attack in the right direction
            enemy.ForceOctantParameter();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Additional cleanup if needed when exiting attack state
    }
}

// For the hit reaction state
public class EnemyHitReactionBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the EnemyController component
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            // Force octant parameter for correct direction during hit reaction
            enemy.ForceOctantParameter();
        }
    }
}

// For the death state
public class EnemyDeathBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the EnemyController component
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            // Force octant parameter for correct death animation direction
            enemy.ForceOctantParameter();
        }
    }
}
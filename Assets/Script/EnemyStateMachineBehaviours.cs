using UnityEngine;


public class EnemyIdleEnterBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            enemy.ForceOctantParameter();
        }
    }
}

public class EnemyAttackBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            enemy.ForceOctantParameter();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}

public class EnemyHitReactionBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            enemy.ForceOctantParameter();
        }
    }
}

public class EnemyDeathBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyController enemy = animator.GetComponent<EnemyController>();
        
        if (enemy != null)
        {
            enemy.ForceOctantParameter();
        }
    }
}
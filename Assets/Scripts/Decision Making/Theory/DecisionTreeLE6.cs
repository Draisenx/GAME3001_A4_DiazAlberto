using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TreeNode
{
    // Base for action vs decision nodes
    public abstract TreeNode Evaluate();

    // Evaluate every node in the tree!
    public static void Traverse(TreeNode node)
    {
        // Action (leaf) nodes return null because they're the end of the tree,
        // so simply do a null-check to exit the loop!
        if (node != null)
        {
            Traverse(node.Evaluate());
        }
    }
}

public abstract class DecisionNode : TreeNode
{
    public TreeNode yes = null;
    public TreeNode no = null;
}

public class ActionNode : TreeNode
{
    // Return null because actions are leaf nodes (ends of the tree)
    public override TreeNode Evaluate()
    {
        return null;
    }
}

public class VisibleDecision : DecisionNode
{
    public bool visible;

    public override TreeNode Evaluate()
    {
        return visible ? yes : no;
    }
}

public class AudibleDecision : DecisionNode
{
    public bool audible;

    public override TreeNode Evaluate()
    {
        return audible ? yes : no;
    }
}

public class NearDecision : DecisionNode
{
    public bool near;

    public override TreeNode Evaluate()
    {
        return near ? yes : no;
    }
}

public class FlankDecision : DecisionNode
{
    public bool flank;

    public override TreeNode Evaluate()
    {
        return flank ? yes : no;
    }
}

public class CreepAction : ActionNode
{
    public override TreeNode Evaluate()
    {
        Debug.Log("Creeping. . .");
        return base.Evaluate();
    }
}

public class AttackAction : ActionNode
{
    public override TreeNode Evaluate()
    {
        Debug.Log("Attacking!!!");
        return base.Evaluate();
    }
}

public class MoveAction : ActionNode
{
    public override TreeNode Evaluate()
    {
        Debug.Log("Moving...");
        return base.Evaluate();
    }
}

public class DecisionTreeLE6 : MonoBehaviour
{
    void Start()
    {
        VisibleDecision visible = new VisibleDecision();
        AudibleDecision audible = new AudibleDecision();
        NearDecision near = new NearDecision();
        FlankDecision flank = new FlankDecision();
        
        ActionNode creep = new CreepAction();
        ActionNode attack = new AttackAction();
        ActionNode move = new MoveAction();

        visible.no = audible;
        visible.yes = near;

        audible.yes = creep;
        audible.no = null;

        near.no = flank;
        near.yes = attack;

        flank.no = attack;
        flank.yes = move;

        // Outputs "Creeping. . ."
        visible.visible = false;
        audible.audible = true;
        near.near = false;
        flank.flank = false;
        TreeNode.Traverse(visible);

        // For full marks, your output should match the following comments:
        // "Creeping. . ."
        // Nothing
        // "Attacking!!!"
        // "Moving..."
        // "Attacking!!!"

        // Homework:
        // Change the 4 decision booleans to achieve the desired result, then call Traverse!
        // (Do this once for each case)
    }
}

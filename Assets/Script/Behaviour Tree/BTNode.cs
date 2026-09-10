using System;
using System.Collections.Generic;

namespace BT.Core
{
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    public abstract class BTNode
    {
        public NodeState State { get; protected set; }

        public abstract NodeState Evaluate();
    }

    /// <summary>
    /// Selector (OR logic): Menjalankan child secara sekuensial.
    /// Jika ada child yang mengembalikan Success -> Selector mengembalikan Success.
    /// Jika ada child yang mengembalikan Running -> Selector mengembalikan Running.
    /// Jika semua child mengembalikan Failure -> Selector mengembalikan Failure.
    /// </summary>
    public class SelectorNode : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public SelectorNode() { }

        public SelectorNode(List<BTNode> children)
        {
            this.children = children ?? new List<BTNode>();
        }

        public SelectorNode(params BTNode[] children)
        {
            this.children = new List<BTNode>(children);
        }

        public void AddChild(BTNode child)
        {
            if (child != null) children.Add(child);
        }

        public override NodeState Evaluate()
        {
            foreach (var child in children)
            {
                if (child == null) continue;

                NodeState childState = child.Evaluate();
                if (childState == NodeState.Success)
                {
                    State = NodeState.Success;
                    return State;
                }
                if (childState == NodeState.Running)
                {
                    State = NodeState.Running;
                    return State;
                }
            }

            State = NodeState.Failure;
            return State;
        }
    }

    /// <summary>
    /// Sequence (AND logic): Menjalankan child secara sekuensial.
    /// Jika ada child yang mengembalikan Failure -> Sequence mengembalikan Failure.
    /// Jika ada child yang mengembalikan Running -> Sequence mengembalikan Running.
    /// Jika semua child mengembalikan Success -> Sequence mengembalikan Success.
    /// </summary>
    public class SequenceNode : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public SequenceNode() { }

        public SequenceNode(List<BTNode> children)
        {
            this.children = children ?? new List<BTNode>();
        }

        public SequenceNode(params BTNode[] children)
        {
            this.children = new List<BTNode>(children);
        }

        public void AddChild(BTNode child)
        {
            if (child != null) children.Add(child);
        }

        public override NodeState Evaluate()
        {
            foreach (var child in children)
            {
                if (child == null) continue;

                NodeState childState = child.Evaluate();
                if (childState == NodeState.Failure)
                {
                    State = NodeState.Failure;
                    return State;
                }
                if (childState == NodeState.Running)
                {
                    State = NodeState.Running;
                    return State;
                }
            }

            State = NodeState.Success;
            return State;
        }
    }

    /// <summary>
    /// Inverter: Membalik hasil evaluasi child. Success -> Failure, Failure -> Success. Running tetap Running.
    /// </summary>
    public class InverterNode : BTNode
    {
        protected BTNode child;

        public InverterNode(BTNode child)
        {
            this.child = child;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                State = NodeState.Failure;
                return State;
            }

            NodeState childState = child.Evaluate();
            if (childState == NodeState.Success)
            {
                State = NodeState.Failure;
            }
            else if (childState == NodeState.Failure)
            {
                State = NodeState.Success;
            }
            else
            {
                State = NodeState.Running;
            }

            return State;
        }
    }

    /// <summary>
    /// Condition Node: Leaf node yang mengevaluasi kondisi boolean.
    /// </summary>
    public class ConditionNode : BTNode
    {
        private readonly Func<bool> condition;

        public ConditionNode(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override NodeState Evaluate()
        {
            if (condition == null)
            {
                State = NodeState.Failure;
                return State;
            }

            State = condition() ? NodeState.Success : NodeState.Failure;
            return State;
        }
    }

    /// <summary>
    /// Action Node: Leaf node yang mengeksekusi aksi dan mengembalikan NodeState.
    /// </summary>
    public class ActionNode : BTNode
    {
        private readonly Func<NodeState> action;

        public ActionNode(Func<NodeState> action)
        {
            this.action = action;
        }

        public ActionNode(Action simpleAction)
        {
            this.action = () =>
            {
                simpleAction?.Invoke();
                return NodeState.Success;
            };
        }

        public override NodeState Evaluate()
        {
            if (action == null)
            {
                State = NodeState.Failure;
                return State;
            }

            State = action();
            return State;
        }
    }
}
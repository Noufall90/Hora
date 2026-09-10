namespace BT.Core
{
    public class BehaviourTree
    {
        protected BTNode rootNode;

        public BTNode RootNode => rootNode;

        public virtual void Initialize(BTNode root)
        {
            rootNode = root;
        }

        public virtual NodeState UpdateTree()
        {
            if (rootNode == null) return NodeState.Failure;
            return rootNode.Evaluate();
        }
    }
}

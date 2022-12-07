using Godot;

namespace ZealousGodotUtils
{
    public interface IPoolItem
    {
        void RegisterToPool(Pool pool, Guid id);
        Guid GetPoolItemId();

        void RestoreForPool();

        void RecycleForPool();
    }

    public class Pool
    {
        private Dictionary<Guid, IPoolItem> _liveItems = new ();
        private List<IPoolItem> _deadItems = new ();
        private Node3D _rootNode;

        public Pool(Node3D rootNode)
        {
            _rootNode = rootNode;
        }

        public void RegisterItemForPool(IPoolItem item)
        {
            Guid id = Guid.NewGuid();
            item.RegisterToPool(this, id);
            _deadItems.Add(item);
        }

        public IPoolItem? GetFreeItem()
        {

            int i = _deadItems.Count;
            if (i == 0) { return null; }
            IPoolItem item = _deadItems[i - 1];
            _deadItems.RemoveAt(i - 1);
            _liveItems.Add(item.GetPoolItemId(), item);
            _rootNode.AddChild(item as Node);
            item.RestoreForPool();
            //GD.Print($"Freed {item.GetPoolItemId()} live {_liveItems.Keys.Count} dead {_deadItems.Count}");
            return item;
        }

        public void RecycleItem(IPoolItem item)
        {
            Guid id = item.GetPoolItemId();
            if (!_liveItems.ContainsKey(id))
            {
                throw new ArgumentException($"The provided pool item {id} is not in the live list");
            }
            item.RecycleForPool();
            _rootNode.RemoveChild(item as Node);
            _liveItems.Remove(id);
            _deadItems.Add(item);
            //GD.Print($"Recycled {item.GetPoolItemId()} live {_liveItems.Keys.Count} dead {_deadItems.Count}");
        }
    }
}

namespace RJCP.Diagnostics.Watchdog.Timer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// A TimerList for ordered expiry items.
    /// </summary>
    /// <remarks>
    /// Items in this list can be added to indicate when they should next expire, and determine at what time the next
    /// item will expire. Items can be enabled, disabled and changed.
    /// </remarks>
    internal class TimerList
    {
        private ITimerSource m_TimerSource;
        private Dictionary<string, LinkedListNode<TimerItem>> m_Items = new Dictionary<string, LinkedListNode<TimerItem>>();
        private LinkedList<TimerItem> m_Sorted = new LinkedList<TimerItem>();

        public TimerList() : this(new MonotonicTimerSource()) { }

        public TimerList(ITimerSource timerSource)
        {
            if (timerSource == null) throw new ArgumentNullException(nameof(timerSource));
            m_TimerSource = timerSource;
        }

        public int Count { get { return m_Items.Count; } }

        public int Active { get { return m_Sorted.Count; } }

        public void Add(string item, int timeout)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (m_Items.ContainsKey(item)) throw new ArgumentException("An element with the same name already exists", nameof(item));

            TimerItem timerItem = new TimerItem(item);
            LinkedListNode<TimerItem> timerNode = new LinkedListNode<TimerItem>(timerItem);
            m_Items.Add(item, timerNode);
            Add(timerNode, timeout);
        }

        private void Add(LinkedListNode<TimerItem> timerItem, int timeout)
        {
            timerItem.Value.Enabled = (timeout >= 0);
            if (!timerItem.Value.Enabled) return;

            LinkedListNode<TimerItem> node = m_Sorted.First;
            int currentTime = m_TimerSource.GetClock();
            unchecked {
                while (node != null && (node.Value.Expiry - currentTime) <= timeout) {
                    node = node.Next;
                }
                timerItem.Value.Expiry = currentTime + timeout;
            }

            if (node == null) {
                m_Sorted.AddLast(timerItem);
            } else {
                m_Sorted.AddBefore(node, timerItem);
            }
        }

        public void Change(string item, int timeout)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!m_Items.TryGetValue(item, out LinkedListNode<TimerItem> timerNode))
                throw new ArgumentException("Item doesn't exist in timer list", nameof(item));

            if (timerNode.Value.Enabled) m_Sorted.Remove(timerNode);
            Add(timerNode, timeout);
        }

        public bool Remove(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!m_Items.TryGetValue(item, out LinkedListNode<TimerItem> timerNode)) return false;

            m_Items.Remove(item);
            if (timerNode.Value.Enabled) {
                m_Sorted.Remove(timerNode);
                timerNode.Value.Enabled = false;
            }
            return true;
        }

        public bool Contains(string item)
        {
            return m_Items.ContainsKey(item);
        }

        public int NextExpiryOffset()
        {
            if (m_Sorted.Count == 0) return Timeout.Infinite;
            int clock = m_TimerSource.GetClock();
            unchecked {
                return m_Sorted.First.Value.Expiry - clock;
            }
        }

        private static readonly string[] EmptyList = new string[0];

        public IEnumerable<string> ExpungeExpired()
        {
            if (m_Sorted.Count == 0) return EmptyList;

            List<string> expired = new List<string>();
            LinkedListNode<TimerItem> node = m_Sorted.First;
            int clock = m_TimerSource.GetClock();
            unchecked {
                while (node != null && (node.Value.Expiry - clock) <= 0) {
                    LinkedListNode<TimerItem> cnode = node;
                    node = node.Next;

                    expired.Add(cnode.Value.Item);
                    m_Sorted.Remove(cnode);
                    cnode.Value.Enabled = false;
                }
            }
            return expired;
        }

        public void Clear()
        {
            m_Sorted.Clear();
            m_Items.Clear();
        }
    }
}

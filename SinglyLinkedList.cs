/*
 * Code Author - Sarang Date
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    #region Test class for my singly linked list

    /// <summary>
    /// Test class for my LinkedList implementation and to run its operations
    /// </summary>
    public class TestLinkedList
    {
        public static void TestMain()
        {
            SinglyLinkedList<string> sList = new SinglyLinkedList<string>();
            int option, index;
            string item;
            do
            {
                Console.WriteLine(" 1: Insert item at the front of the list");
                Console.WriteLine(" 2: Insert item at the back of the list");
                Console.WriteLine(" 3: Insert item at a particular index");
                Console.WriteLine(" 4: Remove item from the front of the list");
                Console.WriteLine(" 5: Remove item from the back of the list");
                Console.WriteLine(" 6: Remove item at a particular index");
                Console.WriteLine(" 7: Display list contents");
                Console.WriteLine(" 8: Search an item in the list");
                Console.WriteLine(" 9: Remove item if exists in the list");
                Console.WriteLine("10: Reverse the list");
                Console.WriteLine("11: Update the list");
                Console.WriteLine("12: Find if the list contains a circular loop");
                Console.WriteLine("13: Find the mid point of the list");
                Console.WriteLine("14: Create cycle in the list to mid item for testing purpose");
                Console.WriteLine("15: Clear the list");
                Console.WriteLine("16: Clear Screen");
                Console.WriteLine("17: Quit");

                Console.WriteLine("\n\nEnter your choice from 1 to 17");

                Int32.TryParse(Console.ReadLine(), out option);

                switch (option)
                {
                    case 1:
                        Console.WriteLine("\nEnter the item");
                        item = Console.ReadLine();
                        sList.InsertAtFront(item);
                        Console.WriteLine("\nItem " + item + " inserted at the front of the list");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 2:
                        Console.WriteLine("\nEnter the item");
                        item = Console.ReadLine();
                        sList.InsertAtBack(item);
                        Console.WriteLine("\nItem " + item + " inserted at the back of the list");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 3:
                        Console.WriteLine("\nEnter the item");
                        item = Console.ReadLine();
                        Console.WriteLine("\nEnter the index at which you want to insert");
                        Int32.TryParse(Console.ReadLine(), out index);
                        sList.InsertAt(index, item);
                        Console.WriteLine("\nItem " + item + " inserted at the index " + index + " of the list");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 4:
                        Console.WriteLine("\nRemoved item " + sList.RemoveFromFront() + " from the front of the list");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 5:
                        Console.WriteLine("\nRemoved item " + sList.RemoveFromBack() + " from the back of the list");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 6:
                        Console.WriteLine("\nEnter the index of the item which you want to remove");
                        Int32.TryParse(Console.ReadLine(), out index);
                        item = sList.RemoveAt(index).ToString();
                        Console.WriteLine("\nItem " + item + " removed at the index " + index + " of the list");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 7:
                        if (!sList.IsEmpty)
                        {
                            Console.WriteLine("\nList contents are");
                            Console.WriteLine(sList.ToString());
                        }
                        else
                            Console.WriteLine("List is Empty");
                        break;
                    case 8:
                        Console.WriteLine("Enter an item to search in the list");
                        item = Console.ReadLine();
                        if (sList.Contains(item))
                            Console.WriteLine("Item " + item + " found in the list");
                        else
                            Console.WriteLine("Item " + item + " doesn't exist in the list");
                        break;
                    case 9:
                        Console.WriteLine("Enter an item to remove from the list");
                        item = Console.ReadLine();
                        if (sList.Remove(item))
                            Console.WriteLine("Item " + item + " removed from the list");
                        else
                            Console.WriteLine("Item " + item + " doesn't exist in the list");
                        break;
                    case 10:
                        Console.WriteLine("Reversed List is");
                        sList.Reverse();
                        if (!sList.IsEmpty)
                        {
                            Console.WriteLine("\nList contents are");
                            Console.WriteLine(sList.ToString());
                        }
                        break;
                    case 11:
                        Console.WriteLine("Enter an item to update");
                        string oldItem = Console.ReadLine();
                        Console.WriteLine("Enter new item");
                        string newItem = Console.ReadLine();
                        if (sList.Update(oldItem, newItem))
                            Console.WriteLine("Item updated successfully");
                        else
                            Console.WriteLine("Item not found");
                        Console.WriteLine(sList.ToString());
                        break;
                    case 12:
                        if (sList.HasCycle())
                            Console.WriteLine("List contains a circular loop");
                        else
                            Console.WriteLine("List doesn't contain a circular loop");
                        break;
                    case 13:
                        Console.WriteLine("Middle item of the list is " + sList.GetMiddleItem().Item);
                        break;
                    case 14:
                        sList.CreateCycleInListToTest();
                        break;
                    case 15:
                        sList.Clear();
                        Console.WriteLine("List is cleared");
                        break;
                    case 16:
                        Console.Clear();
                        break;
                    case 17:
                        Environment.Exit(0);
                        break;

                    default:
                        break;
                }
                Console.WriteLine("\n\n");
            }
            while (option > 0 && option < 17);
        }
    }

    #endregion

    #region Generic ListNode class for my Singly LinkedList
    /// <summary>
    /// Generic ListNode class - avoiding boxing unboxing here by using generic implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListNode<T>
    {
        private ListNode<T> next;
        private T item;

        /// <summary>
        /// Property to hold pointer to next ListNode -  Self containing object
        /// </summary>
        public ListNode<T> Next
        {
            get { return next; }
            set { next = value; }
        }

        /// <summary>
        /// Property to hold value into the Node
        /// </summary>
        public T Item
        {
            get { return item; }
            set { item = value; }
        }

        /// <summary>
        /// Constructor with item init
        /// </summary>
        /// <param name="item"></param>
        public ListNode(T item)
            : this(item,null)
        {
        }

        /// <summary>
        /// Constructor with item and the next node specified
        /// </summary>
        /// <param name="item"></param>
        /// <param name="next"></param>
        public ListNode(T item, ListNode<T> next)
        {
            this.item = item;
            this.next = next;
        }

        /// <summary>
        /// Overriding ToString to return a string value for the item in the node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (item == null)
                return string.Empty;
            return item.ToString();
        }
    }

    #endregion

    #region My Generic Singly Linked List class and its operations with programming puzzles
    /// <summary>
    /// SinglyLinkedList class for generic implementation of LinkedList. Again, avoiding boxing unboxing here and using ICollection interface members. Believe this can be useful when applying other 
    /// operations such as sorting, searching etc. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SinglyLinkedList<T>:ICollection<T>
    {
        #region private variables

        private ListNode<T> firstNode;
        private ListNode<T> lastNode;

        private int count;
        
        #endregion

        /// <summary>
        /// Property to hold first node in the list
        /// </summary>
        public ListNode<T> FirstNode
        {
            get { return firstNode; }
        }

        /// <summary>
        /// Property to hold last node in the list
        /// </summary>
        public ListNode<T> LastNode
        {
            get { return lastNode; }
        }

        /// <summary>
        /// Indexer to iterate through the list and fetch the item
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get 
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException();
                
                ListNode<T> currentNode = firstNode;
                for (int i = 0; i < index; i++)
                {
                    if (currentNode.Next == null)
                        throw new ArgumentOutOfRangeException();
                    currentNode = currentNode.Next;
                }
                return currentNode.Item;
            }
        }

        /// <summary>
        /// Property to hold count of items in the list
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Property to determine if the list is empty or contains any item
        /// </summary>
        public bool IsEmpty
        {
            get 
            {
                lock (this)
                {
                    return firstNode == null;
                }
            }
        }

        /// <summary>
        /// default constructor initializing list
        /// </summary>
        public SinglyLinkedList() { }

        /// <summary>
        /// Operation inserts item at the front of the list
        /// </summary>
        /// <param name="item"></param>
        public void InsertAtFront(T item)
        {
            lock (this)
            {
                if (IsEmpty)
                    firstNode = lastNode = new ListNode<T>(item);
                else
                    firstNode = new ListNode<T>(item, firstNode);
                count++;
            }
        }

        /// <summary>
        /// Operation inserts item at the back of the list
        /// </summary>
        /// <param name="item"></param>
        public void InsertAtBack(T item)
        {
            lock (this)
            {
                if (IsEmpty)
                    firstNode = lastNode = new ListNode<T>(item);
                else
                    lastNode = lastNode.Next = new ListNode<T>(item);
                count++;
            }
        }

        /// <summary>
        /// Operation removes item from the front of the list
        /// </summary>
        /// <returns></returns>
        public object RemoveFromFront()
        {
            lock (this)
            {
                if (IsEmpty)
                    throw new ApplicationException("list is empty");
                object removedData = firstNode.Item;
                if (firstNode == lastNode)
                    firstNode = lastNode = null;
                else
                    firstNode = firstNode.Next;
                count--;
                return removedData;
            }
        }

        /// <summary>
        /// Operation removes item from the back of the list
        /// </summary>
        /// <returns></returns>
        public object RemoveFromBack()
        {
            lock (this)
            {
                if (IsEmpty)
                    throw new ApplicationException("list is empty");
                object removedData = lastNode.Item;
                if (firstNode == lastNode)
                    firstNode = lastNode = null;
                else
                {
                    ListNode<T> currentNode = firstNode;
                    while (currentNode.Next != lastNode)
                        currentNode = currentNode.Next;
                    lastNode = currentNode;
                    currentNode.Next = null;
                }
                count--;
                return removedData;
            }
        }

        /// <summary>
        /// Operation inserts item at the specified index in the list
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void InsertAt(int index, T item)
        {
            lock (this)
            {
                if (index > count || index < 0)
                    throw new ArgumentOutOfRangeException();
                if (index == 0)
                    InsertAtFront(item);
                else if (index == (count - 1))
                    InsertAtBack(item);
                else
                {
                    ListNode<T> currentNode = firstNode;
                    for (int i = 0; i < index - 1; i++)
                    {
                        currentNode = currentNode.Next;
                    }
                    ListNode<T> newNode = new ListNode<T>(item, currentNode.Next);
                    currentNode.Next = newNode;
                    count++;
                }
            }
        }

        /// <summary>
        /// Operation removes item from the specified index in the list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object RemoveAt(int index)
        {
            lock (this)
            {
                if (index > count || index < 0)
                    throw new ArgumentOutOfRangeException();
                object removedData;
                if (index == 0)
                    removedData = RemoveFromFront();
                else if (index == (count - 1))
                    removedData = RemoveFromBack();
                else
                {
                    ListNode<T> currentNode = firstNode;
                    for (int i = 0; i < index; i++)
                    {
                        currentNode = currentNode.Next;
                    }
                    removedData = currentNode.Item;
                    currentNode.Next = currentNode.Next.Next;
                    count--;
                }
                return removedData;
            }
        }

        /// <summary>
        /// Operation updates an item provided as an input with a new item (also provided as an input)
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        public bool Update(T oldItem, T newItem)
        {
            lock (this)
            {
                ListNode<T> currentNode = firstNode;
                while (currentNode != null)
                {
                    if (currentNode.ToString().Equals(oldItem.ToString()))
                    {
                        currentNode.Item = newItem;
                        return true;
                    }
                    currentNode = currentNode.Next;
                }
                return false;
            }
        }

        /// <summary>
        /// Operation resets the list and clears all its contents
        /// </summary>
        public void Clear()
        {
            firstNode = lastNode = null;
            count = 0;
        }

        /// <summary>
        /// Operation to reverse the contents of the linked list by resetting the pointers and swapping the contents
        /// </summary>
        public void Reverse()
        {
            if (firstNode == null || firstNode.Next == null)
                return;

            lastNode = firstNode;

            ListNode<T> prevNode = null;
            ListNode<T> currentNode = firstNode;
            ListNode<T> nextNode = firstNode.Next;

            while (currentNode != null)
            {
                currentNode.Next = prevNode;
                if (nextNode == null)
                    break;
                prevNode = currentNode;
                currentNode = nextNode;
                nextNode = nextNode.Next;
            }

            firstNode = currentNode;
        }

        /// <summary>
        /// Operation to get contents from the list. This has been duplicated as a simplification when I overridden ToString for the list
        /// </summary>
        /// <returns></returns>
        public string GetListContents()
        {
            string strListItems = String.Empty;
            ListNode<T> currentNode = firstNode;
            while (currentNode != null)
            {
                strListItems += currentNode.Item.ToString() + "-->";
                currentNode = currentNode.Next;
            }
            return strListItems;
        }

        #region ICollection<T> Members

        /// <summary>
        /// Add to the back of the list. Acts as an append operation
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            InsertAtBack(item);
        }

        /// <summary>
        /// Returns true if list contains the input item else false
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            lock (this)
            {
                ListNode<T> currentNode = firstNode;
                while (currentNode != null)
                {
                    if (currentNode.Item.ToString().Equals(item.ToString()))
                    {
                        return true;
                    }
                    currentNode = currentNode.Next;
                }
                return false;
            }
        }

        /// <summary>
        /// Interface member not implemented as of now
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Irrelevant for now
        /// </summary>
        public bool IsReadOnly
        {
			get { return false; }
        }

        /// <summary>
        /// Removes the input item if exists and returns true else false
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            if (firstNode.Item.ToString().Equals(item.ToString()))
            {
                RemoveFromFront();
                return true;
            }
            else if (lastNode.Item.ToString().Equals(item.ToString()))
            {
                RemoveFromBack();
                return true;
            }
            else
            {

                ListNode<T> currentNode = firstNode;

                while (currentNode.Next != null)
                {
                    if (currentNode.Next.Item.ToString().Equals(item.ToString()))
                    {
                        currentNode.Next = currentNode.Next.Next;
                        count--;
                        if (currentNode.Next == null)
                            lastNode = currentNode;
                        return true;
                    }
                    currentNode = currentNode.Next;
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Custom GetEnumerator method to traverse through the list and yield the current value
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            ListNode<T> currentNode = firstNode;
            while (currentNode != null)
            {
                yield return currentNode.Item;
                currentNode = currentNode.Next;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Operation ToString overridden to get the contents from the list
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsEmpty)
                return string.Empty;
            StringBuilder returnString = new StringBuilder();
            foreach (T item in this)
            {
                if (returnString.Length > 0)
                    returnString.Append("->");
                returnString.Append(item);
            }
            return returnString.ToString();
        }

        /// <summary>
        /// Operation to find if the linked list contains a circular loop
        /// </summary>
        /// <returns></returns>
        public bool HasCycle()
        {
            ListNode<T> currentNode = firstNode;
            ListNode<T> iteratorNode = firstNode;

            for (; iteratorNode != null && iteratorNode.Next != null; iteratorNode = iteratorNode.Next )
            {
                if (currentNode.Next == null || currentNode.Next.Next == null) return false;
                if (currentNode.Next == iteratorNode || currentNode.Next.Next == iteratorNode) return true;
                currentNode = currentNode.Next.Next;
            }
            return false;
        }

        /// <summary>
        /// Operation to find the midpoint of a list 
        /// </summary>
        /// <returns></returns>
        public ListNode<T> GetMiddleItem()
        {
            ListNode<T> currentNode = firstNode;
            ListNode<T> iteratorNode = firstNode;

            for (; iteratorNode != null && iteratorNode.Next != null; iteratorNode = iteratorNode.Next)
            {
                if (currentNode.Next == null || currentNode.Next.Next == null) return iteratorNode;
                if (currentNode.Next == iteratorNode || currentNode.Next.Next == iteratorNode) return null;
                currentNode = currentNode.Next.Next;
            }
            return firstNode;
        }

        /// <summary>
        /// Operation creates a circular loop in the linked list for testing purpose. Once this loop is created, other operations would probably fail.
        /// </summary>
        public void CreateCycleInListToTest()
        {
           // ListNode<T> currentNode = firstNode;
            ListNode<T> midNode = GetMiddleItem();
            lastNode.Next = midNode;
        }
    }

    #endregion
}

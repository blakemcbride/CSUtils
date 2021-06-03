/*
*  Copyright (c) 2015 Blake McBride (blake@mcbridemail.com)
*  All rights reserved.
*
*  Permission is hereby granted, free of charge, to any person obtaining
*  a copy of this software and associated documentation files (the
*  "Software"), to deal in the Software without restriction, including
*  without limitation the rights to use, copy, modify, merge, publish,
*  distribute, sublicense, and/or sell copies of the Software, and to
*  permit persons to whom the Software is furnished to do so, subject to
*  the following conditions:
*
*  1. Redistributions of source code must retain the above copyright
*  notice, this list of conditions, and the following disclaimer.
*
*  2. Redistributions in binary form must reproduce the above copyright
*  notice, this list of conditions and the following disclaimer in the
*  documentation and/or other materials provided with the distribution.
*
*  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
*  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
*  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
*  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
*  HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
*  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
*  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
*  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
*  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
*  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
*  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/


namespace CSUtils {
    /// <summary>
    /// Lisp-like linked list for only one level deep
    /// </summary>
    /// <typeparam name="T">The type of the items on the list</typeparam>
    public class SimpleLinkedList<T> {
        public T Item { get; set; }
        public SimpleLinkedList<T> Next { get; set; }

        public SimpleLinkedList() {
        }

        public SimpleLinkedList(T itm) {
            Item = itm;
        }

        public SimpleLinkedList(T itm, SimpleLinkedList<T> nxt) {
            Item = itm;
            Next = nxt;
        }

        /// <summary>
        /// Return the item on the first node of the list.
        /// </summary>
        /// <returns>the item on the first node of the list</returns>
        public T Car() {
            return Item;
        }

        /// <summary>
        /// Return the next node in the list
        /// </summary>
        /// <returns>the next link</returns>
        public SimpleLinkedList<T> Cdr() {
            return Next;
        }

        /// <summary>
        /// Add a new item to the beginning of the list.  But the list can't be null.
        /// </summary>
        /// <param name="newItem">item to be placed at the beginnging of the list</param>
        /// <returns>the new beginning of the list</returns>
        public SimpleLinkedList<T> Cons(T newItem) {
            return new SimpleLinkedList<T>(newItem, this);
        }

        /// <summary>
        ///  Add a new item to the beginning of a list.  Works with null.
        /// </summary>
        /// <param name="newItem">the first element of the list</param>
        /// <param name="lst">the rermainder of the list</param>
        /// <returns>the new beginning of the list</returns>
        public static SimpleLinkedList<T> Cons(T newItem, SimpleLinkedList<T> lst) {
            return new SimpleLinkedList<T>(newItem, lst);
        }

        public int Length() {
            int len;
            SimpleLinkedList<T> node = this;
            for (len = 0; node != null; node = node.Next)
                len++;
            return len;
        }

        public static int Length(SimpleLinkedList<T> lst) {
            return lst?.Length() ?? 0;
        }

        public static bool Null(SimpleLinkedList<T> lst) {
            return lst == null;
        }

        public SimpleLinkedList<T> Last() {
            SimpleLinkedList<T> node = this;
            while (node.Next != null)
                node = node.Next;
            return node;
        }

        public SimpleLinkedList<T> Reverse() {
            SimpleLinkedList<T> lst = null;
            SimpleLinkedList<T> node = this;
            for (; node != null; node = node.Next)
                lst = new SimpleLinkedList<T>(node.Item, lst);
            return lst;
        }

        public SimpleLinkedList<T> Append(SimpleLinkedList<T> lst) {
            Last().Next = lst;
            return this;
        }
    }
}
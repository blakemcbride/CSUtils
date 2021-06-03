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
    /// Generic Lisp-like linked list for any number of levels deep
    /// </summary>
    public class GenericLinkedList {
        public object Item { get; set; }
        public GenericLinkedList Next { get; set; }

        public GenericLinkedList() {
        }

        public GenericLinkedList(object itm) {
            Item = itm;
        }

        public GenericLinkedList(object itm, GenericLinkedList nxt) {
            Item = itm;
            Next = nxt;
        }

        /// <summary>
        /// Return the item on the first node of the list.
        /// </summary>
        /// <returns>the item on the first node of the list</returns>
        public object Car() {
            return Item;
        }

        /// <summary>
        /// Return the next node in the list
        /// </summary>
        /// <returns>the next link</returns>
        public GenericLinkedList Cdr() {
            return Next;
        }

        public object Caar() {
            return ((GenericLinkedList) Item).Item;
        }

        public object Cadr() {
            return Next.Item;
        }

        public GenericLinkedList Cddr() {
            return Next.Next;
        }

        public GenericLinkedList Cdar() {
            return ((GenericLinkedList) Item).Next;
        }

        /// <summary>
        /// Add a new item to the beginning of the list.
        /// </summary>
        /// <param name="newItem">item to be placed at the beginnging of the list</param>
        /// <returns>the new beginning of the list</returns>
        public GenericLinkedList Cons(object newItem) {
            return new GenericLinkedList(newItem, this);
        }

        public int Length() {
            int len;
            GenericLinkedList node = this;
            for (len = 0; node != null; node = node.Next)
                len++;
            return len;
        }

        public static int Length(GenericLinkedList lst) {
            return lst?.Length() ?? 0;
        }

        public static bool Null(GenericLinkedList lst) {
            return lst == null;
        }

        public GenericLinkedList Last() {
            GenericLinkedList node = this;
            while (node.Next != null)
                node = node.Next;
            return node;
        }

        public GenericLinkedList Reverse() {
            GenericLinkedList lst = null;
            GenericLinkedList node = this;
            for (; node != null; node = node.Next)
                lst = new GenericLinkedList(node.Item, lst);
            return lst;
        }

        public GenericLinkedList Append(GenericLinkedList lst) {
            Last().Next = lst;
            return this;
        }
    }
}
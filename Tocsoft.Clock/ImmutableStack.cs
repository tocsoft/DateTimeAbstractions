using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tocsoft.DateTimeAbstractions
{
    internal static class ImmutableStack
    {
        /// <summary>
        /// Returns an empty collection.
        /// </summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The immutable collection.</returns>
        public static ImmutableStack<T> Create<T>()
        {
            return ImmutableStack<T>.Empty;
        }

        /// <summary>
        /// Creates a new immutable collection prefilled with the specified item.
        /// </summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <param name="item">The item to prepopulate.</param>
        /// <returns>The new immutable collection.</returns>
        public static ImmutableStack<T> Create<T>(T item)
        {
            return ImmutableStack<T>.Empty.Push(item);
        }

    }
    internal class ImmutableStack<T>
    {
        private static readonly ImmutableStack<T> emptyField = new ImmutableStack<T>();
        private T head;
        private ImmutableStack<T> tail;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableStack{T}"/> class
        /// that acts as the empty stack.
        /// </summary>
        private ImmutableStack()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableStack{T}"/> class.
        /// </summary>
        /// <param name="head">The head element on the stack.</param>
        /// <param name="tail">The rest of the elements on the stack.</param>
        private ImmutableStack(T head, ImmutableStack<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }

        /// <summary>
        /// Gets the empty stack, upon which all stacks are built.
        /// </summary>
        public static ImmutableStack<T> Empty
        {
            get
            {
                return emptyField;
            }
        }

        /// <summary>
        /// Gets the empty stack, upon which all stacks are built.
        /// </summary>
        public ImmutableStack<T> Clear()
        {
            return Empty;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return tail == null; }
        }

        /// <summary>
        /// Gets the element on the top of the stack.
        /// </summary>
        /// <returns>
        /// The element on the top of the stack. 
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public T Peek()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("Collection empty");
            }

            return head;
        }

        /// <summary>
        /// Pushes an element onto a stack and returns the new stack.
        /// </summary>
        /// <param name="value">The element to push onto the stack.</param>
        /// <returns>The new stack.</returns>
        public ImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, this);
        }

        /// <summary>
        /// Returns a stack that lacks the top element on this stack.
        /// </summary>
        /// <returns>A stack; never <c>null</c></returns>
        /// <exception cref="InvalidOperationException">Thrown when the stack is empty.</exception>
        public ImmutableStack<T> Pop()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("Collection empty");
            }

            return tail;
        }

        /// <summary>
        /// Pops the top element off the stack.
        /// </summary>
        /// <param name="value">The value that was removed from the stack.</param>
        /// <returns>
        /// A stack; never <c>null</c>
        /// </returns>
        public ImmutableStack<T> Pop(out T value)
        {
            value = this.Peek();
            return this.Pop();
        }

    }
}

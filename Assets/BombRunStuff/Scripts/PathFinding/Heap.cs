using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is all from "A* Pathfinding (E04: heap optimization)" video by Sebastian Lague
// https://www.youtube.com/watch?v=3Dw5d7PlcTM
public class Heap<T> where T: IHeapItem<T>
{
    T[] _items;
    int _currentItemCount;

    public Heap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }
    public void Add(T item)
    {
        item.HeapIndex = _currentItemCount;
        _items[_currentItemCount] = item;
        SortUp(item);
        _currentItemCount++;
    }
    public T RemoveFirst()
    {
        // get the first item on the heap (has lowest value / highest priority)
        T firstItem = _items[0];
        _currentItemCount--;

        // now set the first item to the last item on the heap. Then sort down to re-organize the heap?
        _items[0] = _items[_currentItemCount];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);

        return firstItem;
    }
    public void UpdateItem(T item)
    {
        SortUp(item);
    }
    public int Count
    {
        get 
        {
            return _currentItemCount;
        }
    }
    public bool Contains(T item)
    {
        // check if an item exists in the heap by grabbing the item at the corresponding heap index and check if they are the same item???
        return Equals(_items[item.HeapIndex], item);
    }
    private void SortDown(T item)
    {
        while (true)
        {
            // get the item's two child objects on the heap
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;

            int swapIndex = 0;

            // check that the child index's exist on the heap by comparing to the _currentItem count
            // If there are two children, check which child has the higher priority / lowest value
            if (childIndexLeft < _currentItemCount)
            {
                swapIndex = childIndexLeft;
                // if the childIndexRight has the higher priority compared to the childIndexLeft, set the swap index to childIndexRight
                if (childIndexRight < _currentItemCount)
                {
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                // now check if the parent item has a lower priority than the highest priority child. If yes, swap the parent and highest priority child
                // if the parent has a higher priority than highest priority child, then parent is in correct position. Exit loop
                if (_items[item.HeapIndex].CompareTo(_items[swapIndex]) < 0)
                {
                    Swap(item, _items[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                // parent has no children so it is in the correct position. Exit from loop
                return;
            }
        }
    }
    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            T parentItem = _items[parentIndex];

            // higher priority > 0
            // same priority = 0
            // lower priority = -1
            // check if item has higher priority than parent item. If yes, swap the item with its parent
            if (item.CompareTo(parentItem) > 0)
            {
                // item has higher priority than the parent. For path finding, this means the item has a LOWER fcost
                Swap(item, parentItem);
            }
            else
            {
                // item is no longer higher priority than parent item. Stop checking to see if it needs to be swapped
                break;
            }
            // get the heap index of the new parent after the item was swapped
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }
    private void Swap(T itemA, T itemB)
    {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;

        // swap the heap index's on the items that were swapped so you can find them on the heap correctly
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}

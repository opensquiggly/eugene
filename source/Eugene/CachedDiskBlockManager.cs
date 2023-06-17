// namespace Eugene;
//
// using System.Collections.Generic;
//
// public class CachedDiskBlockManager<THeader> where THeader : struct
// {
//     private readonly DiskBlockManager<THeader> diskBlockManager;
//     private readonly int cacheCapacity;
//     // private readonly Dictionary<long, LinkedListNode<CacheItem>> cache;
//     // private readonly LinkedList<CacheItem> lruList;
//
//     public CachedDiskBlockManager(DiskBlockManager<THeader> diskBlockManager, int cacheCapacity)
//     {
//         this.diskBlockManager = diskBlockManager;
//         this.cacheCapacity = cacheCapacity;
//         this.cache = new Dictionary<long, LinkedListNode<CacheItem>>(cacheCapacity);
//         this.lruList = new LinkedList<CacheItem>();
//     }
//
//     public void ReadBlock<TStruct>(long address, out TStruct output) where TStruct : struct
//     {
//         // If the block is in the cache, move it to the front of the LRU list and return it
//         if (cache.TryGetValue(address, out var node))
//         {
//             output = node.Value.Data;
//             lruList.Remove(node);
//             lruList.AddFirst(node);
//             return;
//         }
//
//         // Otherwise, load the block from disk
//         diskBlockManager.ReadBlock(address, out output);
//
//         // If the cache is full, remove the least recently used item
//         if (cache.Count >= cacheCapacity)
//         {
//             cache.Remove(lruList.Last.Value.Address);
//             lruList.RemoveLast();
//         }
//
//         // Add the new block to the cache and the front of the LRU list
//         var newNode = new LinkedListNode<CacheItem>(new CacheItem { Address = address, Data = output });
//         cache.Add(address, newNode);
//         lruList.AddFirst(newNode);
//     }
//
//     public void WriteBlock<TStruct>(long address, TStruct data)
//     {
//         // Write the block to disk
//         diskBlockManager.WriteBlock(address, data);
//
//         // If the block is in the cache, update it and move it to the front of the LRU list
//         if (cache.TryGetValue(address, out var node))
//         {
//             node.Value.Data = data;
//             lruList.Remove(node);
//             lruList.AddFirst(node);
//             return;
//         }
//
//         // If the cache is full, remove the least recently used item
//         if (cache.Count >= cacheCapacity)
//         {
//             cache.Remove(lruList.Last.Value.Address);
//             lruList.RemoveLast();
//         }
//
//         // Add the new block to the cache and the front of the LRU list
//         var newNode = new LinkedListNode<CacheItem>(new CacheItem { Address = address, Data = data });
//         cache.Add(address, newNode);
//         lruList.AddFirst(newNode);
//     }
//
//     public void DeleteBlock(long address)
//     {
//         // Delete the block from disk
//         diskBlockManager.DeleteBlock(address);
//
//         // If the block is in the cache, remove it
//         if (cache.TryGetValue(address, out var node))
//         {
//             cache.Remove(address);
//             lruList.Remove(node);
//         }
//     }
//
//     public void AppendBlock(TStruct data, out long address)
//     {
//         // Append the block to disk and get its address
//         diskBlockManager.AppendBlock(data, out address);
//
//         // If the cache is full, remove the least recently used item
//         if (cache.Count >= cacheCapacity)
//         {
//             cache.Remove(lruList.Last.Value.Address);
//             lruList.RemoveLast();
//         }
//
//         // Add the new block to the cache and the front of the LRU list
//         var newNode = new LinkedListNode<CacheItem>(new CacheItem { Address = address, Data = data });
//         cache.Add(address, newNode);
//         lruList.AddFirst(newNode);
//     }
//
//     private class CacheItem<TStruct> where TStruct : struct
//     {
//         public long Address { get; set; }
//         public TStruct Data { get; set; }
//     }
// }
//

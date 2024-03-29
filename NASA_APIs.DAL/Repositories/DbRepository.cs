﻿using Microsoft.EntityFrameworkCore;
using NASA_APIs.DAL.Context;
using NASA_APIs.DAL.Entities.Base;
using NASA_APIs.Interfaces.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NASA_APIs.DAL.Repositories
{
    public class DbRepository<T> : IRepository<T> where T : Entity, new()
    {
        private readonly DataDB _db;

        protected DbSet<T> Set { get; }
        protected virtual IQueryable<T> Items => Set;

        public bool AutoSaveChanges { get; set; } = true;
        public DbRepository(DataDB db)
        {
            _db = db;
            Set = _db.Set<T>();
        }
        public async Task<T> Add(T item, CancellationToken Cancel = default)
        {
           if(item == null) throw new ArgumentNullException(nameof(item));
            await _db.AddAsync(item, Cancel).ConfigureAwait(false);
            if(AutoSaveChanges)
                await SaveChanges(Cancel).ConfigureAwait(false);
            return item;
        }

        public async Task<T> Delete(T item, CancellationToken Cancel = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!await ExistId(item.Id, Cancel))
                return null;
            _db.Remove(item);
            if (AutoSaveChanges)
                await SaveChanges(Cancel).ConfigureAwait(false);
            return item;
        }

        public async Task<T> DeleteById(int Id, CancellationToken Cancel = default)
        {
            var item = Set.Local.FirstOrDefault(i => i.Id == Id);
            if (item is null)
                item = await Set
                    .Select(i => new T { Id = i.Id })
                    .FirstOrDefaultAsync(i => i.Id == Id, Cancel)
                    .ConfigureAwait(false);
            if (item is null) return null;
            return await Delete(item, Cancel).ConfigureAwait(false);
        }

        public async Task<bool> Exist(T item, CancellationToken Cancel = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return await Items.AnyAsync(i => i.Id == item.Id, Cancel).ConfigureAwait(false); 
        }

        public async Task<bool> ExistId(int Id, CancellationToken Cancel = default)
        {
            return await Items.AnyAsync(item => item.Id == Id, Cancel).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            if (Count <= 0) return Enumerable.Empty<T>();
            IQueryable<T> query = Items switch
            {
                IOrderedQueryable<T> ordered_query => ordered_query,
                { } q => q.OrderBy(i => i.Id)
            };
            if (Skip > 0)
                query = query.Skip(Skip);
            return await query.Take(Count).ToArrayAsync(Cancel).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> GetAll(CancellationToken Cancel = default)
        {
            return await Items.ToArrayAsync(Cancel).ConfigureAwait(false);
        }

        public async Task<T> GetById(int Id, CancellationToken Cancel = default)
        {
            switch (Items)
            {
                case DbSet<T> set:
                    return await set.FindAsync(new object[] {Id}, Cancel).ConfigureAwait(false);
                case IQueryable<T> items:
                    return await items.FirstOrDefaultAsync(item => item.Id == Id, Cancel).ConfigureAwait(false);
                default:
                    throw new InvalidOperationException("Ошибка в определнии данных источника");
            }
           
        }

        public async Task<int> GetCount(CancellationToken Cancel = default)
        {
           return await Items.CountAsync(Cancel).ConfigureAwait(false); 
        }

        protected record Page(IEnumerable<T> Items, int TotalCount, int PageIndex, int PageSize ) : IPage<T>
        {
            public int TotalPagesCount => (int)Math.Ceiling((double)TotalCount / PageSize);
        }
        public async Task<IPage<T>> GetPage(int PageIndex, int PageSize, CancellationToken Cancel = default)
        {
            if (PageSize < 0)
                return new Page(Enumerable.Empty<T>(), await GetCount(Cancel).ConfigureAwait(false), PageIndex, PageSize);
            var query = Items;
            var total_count = await query.CountAsync(Cancel).ConfigureAwait(false);
            if (total_count == 0) return new Page(Enumerable.Empty<T>(), 0, PageIndex, PageSize);
            if (query is not IOrderedQueryable<T>)
                query = query.OrderBy(i => i.Id);
            if(PageIndex > 0)
                query = query.Skip(PageIndex * PageSize);
            query = query.Take(PageSize);
            var items = await query.ToArrayAsync(Cancel).ConfigureAwait(false);
            return new Page(items, total_count, PageIndex, PageSize);
        }

        public async Task<T> Update(T item, CancellationToken Cancel = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _db.Update(item);
            if (AutoSaveChanges)
                await SaveChanges(Cancel).ConfigureAwait(false);
            return item;
        }
        public async Task<int> SaveChanges(CancellationToken Cancel = default)
        {
            return await _db.SaveChangesAsync(Cancel).ConfigureAwait(false);
        }
    }
}

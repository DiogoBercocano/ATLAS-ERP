using System;
using System.Collections.Generic;
using System.Linq;

namespace ATLAS_ERP.Infrastructure
{
    public interface IPagedResult
    {
        int    Page       { get; }
        int    PageSize   { get; }
        int    TotalItems { get; }
        int    TotalPages { get; }
        int    FirstItem  { get; }
        int    LastItem   { get; }
        bool   HasPrevious{ get; }
        bool   HasNext    { get; }
        string Search     { get; }
    }

    public class PagedResult<T> : IPagedResult
    {
        public IReadOnlyList<T> Items     { get; }
        public int              Page      { get; }
        public int              PageSize  { get; }
        public int              TotalItems{ get; }
        public string           Search    { get; }

        public int  TotalPages  => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext     => Page < TotalPages;
        public int  FirstItem   => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int  LastItem    => Math.Min(Page * PageSize, TotalItems);

        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalItems, string search = null)
        {
            Items      = items ?? new List<T>();
            Page       = page < 1 ? 1 : page;
            PageSize   = pageSize < 1 ? 1 : pageSize;
            TotalItems = totalItems < 0 ? 0 : totalItems;
            Search     = search ?? "";
        }

        public static PagedResult<T> Empty(int pageSize)
            => new PagedResult<T>(new List<T>(), 1, pageSize, 0, "");
    }

    public static class PagingDefaults
    {
        public const int PageSize    = 20;
        public const int MaxPageSize = 100;

        public static int ClampPage(int page)
            => page < 1 ? 1 : page;

        public static int ClampPageSize(int pageSize)
        {
            if (pageSize < 1) return PageSize;
            if (pageSize > MaxPageSize) return MaxPageSize;
            return pageSize;
        }
    }

    public static class QueryablePagingExtensions
    {
        public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> query, int page, int pageSize, string search = null)
        {
            page     = PagingDefaults.ClampPage(page);
            pageSize = PagingDefaults.ClampPageSize(pageSize);

            var totalItems = query.Count();
            var skip       = (page - 1) * pageSize;
            var items      = query.Skip(skip).Take(pageSize).ToList();
            return new PagedResult<T>(items, page, pageSize, totalItems, search);
        }
    }
}

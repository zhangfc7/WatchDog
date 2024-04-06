﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WatchDog.src.Utilities;

namespace WatchDog.src.Helpers
{
    public class Page<T>
    {
        public long PageIndex { get; private set; }
        public long TotalPages { get; private set; }
        public IEnumerable<T> Data { get; private set; }
        public bool HasPreviousPage { get => (PageIndex > 1); }
        public bool HasNextPage { get => (PageIndex < TotalPages); }

        public Page(IEnumerable<T> items, long count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (long)Math.Ceiling(count / (double)pageSize);
            Data = items;
        }
    }
    public static class PageExtension
    {
        public static Page<T> ToPaginatedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize = Constants.PageSize)
        {
            var count = source.LongCount();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return new Page<T>(items, count, pageIndex, pageSize);
        }
    }
}

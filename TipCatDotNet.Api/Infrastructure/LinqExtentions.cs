using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TipCatDotNet.Api.Models.Common.Enums;

namespace TipCatDotNet.Api.Infrastructure;
public static class LinqExtentions
{
    public static IOrderedEnumerable<TSource> OrderByEnum<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, SortVariant variant)
        => (variant == SortVariant.ASC) ?
            source.OrderBy(keySelector) :
            source.OrderByDescending(keySelector);


    public static IQueryable<TSource> AsAsyncQueryable<TSource>(this IOrderedEnumerable<TSource> source) =>
            new AsyncQueryable<TSource>(source.AsQueryable());


    internal class AsyncQueryable<TSource> : IAsyncEnumerable<TSource>, IQueryable<TSource>
    {
        public AsyncQueryable(IQueryable<TSource> source)
        {
            Source = source;
        }


        public Type ElementType => typeof(TSource);


        public Expression Expression => Source.Expression;


        public IQueryProvider Provider => new AsyncQueryProvider<TSource>(Source.Provider);


        public IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumeratorWrapper<TSource>(Source.GetEnumerator());
        }


        public IEnumerator<TSource> GetEnumerator() => Source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private IQueryable<TSource> Source;
    }


    internal class AsyncQueryProvider<TSource> : IQueryProvider
    {
        public AsyncQueryProvider(IQueryProvider source)
        {
            Source = source;
        }


        public IQueryable CreateQuery(Expression expression) =>
            Source.CreateQuery(expression);


        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new AsyncQueryable<TElement>(Source.CreateQuery<TElement>(expression));


        public object Execute(Expression expression) => Execute<TSource>(expression)!;


        public TResult Execute<TResult>(Expression expression) =>
            Source.Execute<TResult>(expression);


        private readonly IQueryProvider Source;
    }


    internal class AsyncEnumeratorWrapper<TSource> : IAsyncEnumerator<TSource>
    {
        public AsyncEnumeratorWrapper(IEnumerator<TSource> source)
        {
            Source = source;
        }


        public TSource Current => Source.Current;


        public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);


        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(Source.MoveNext());


        private readonly IEnumerator<TSource> Source;
    }
}
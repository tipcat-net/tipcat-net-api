using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace TipCatDotNet.ApiTests.Utils;

public class DbSetMockProvider
{
    public static DbSet<T> GetDbSetMock<T>(IEnumerable<T> enumerable) where T : class
    {
        var list = enumerable is List<T> enumerableList ? enumerableList : enumerable.ToList();
        var mock = list.AsQueryable().BuildMockDbSet();

        mock.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(x =>
        {
            if (TryGetIdProperty(x, out var propertyInfo))
            {
                var currentId = GetId(propertyInfo!, list);
                SetId(propertyInfo!, x, currentId + 1);
            }
                
            list.Add(x);
        });

        return mock.Object;
    }


    private static bool TryGetIdProperty<T>(T target, out PropertyInfo? propertyInfo)
    {
        propertyInfo = default;

        if (target is null)
            return false;

        var type = target.GetType();

        if (IdentifiableTypes.TryGetValue(type, out propertyInfo))
            return propertyInfo is not null;

        propertyInfo = type.GetProperty(IdToken);
        IdentifiableTypes.TryAdd(type, propertyInfo);

        return propertyInfo is not null;
    }


    private static int GetId<T>(PropertyInfo propertyInfo, List<T> list)
    {
        if (!list.Any())
            return 0;

        return (int)propertyInfo.GetValue(list.Last(), null)!;
    }


    private static void SetId<T>(PropertyInfo propertyInfo, T target, int value) 
        => propertyInfo.SetValue(target, value);


    private const string IdToken = "Id";

    private static readonly ConcurrentDictionary<Type, PropertyInfo?> IdentifiableTypes = new();
}
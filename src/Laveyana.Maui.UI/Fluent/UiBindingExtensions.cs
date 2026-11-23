using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Laveyana.Maui.UI.Fluent;

/// <summary>Типизированный синтаксис для MAUI Binding; имена ошибок проверяются заранее.</summary>
public static class UiBindingExtensions
{
    private static readonly ConcurrentDictionary<(Type Owner, string Property), BindableProperty> Cache = new();

    /// <summary>Создаёт типизированную привязку MAUI к свойству модели.</summary>
    public static TTarget Bind<TTarget, TTargetValue, TSource, TSourceValue>(
        this TTarget view,
        Expression<Func<TTarget, TTargetValue>> target,
        TSource source,
        Expression<Func<TSource, TSourceValue>> property,
        BindingMode mode = BindingMode.Default)
        where TTarget : BindableObject
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(property);
        string targetName = PropertyName(target.Body);
        string path = PropertyPath(property.Body);
        var bindable = Cache.GetOrAdd((typeof(TTarget), targetName), static key =>
        {
            for (Type? current = key.Owner; current is not null; current = current.BaseType)
            {
                var field = current.GetField(key.Property + "Property", BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (field?.GetValue(null) is BindableProperty found) return found;
            }
            throw new InvalidOperationException($"Не найдено BindableProperty {key.Property}Property у {key.Owner}.");
        });
        view.SetBinding(bindable, new Binding(path, mode, source: source));
        return view;
    }

    private static string PropertyName(Expression expression)
        => expression is MemberExpression { Member: PropertyInfo prop } ? prop.Name
            : throw new ArgumentException("Требуется ссылка на свойство, например x => x.Text.");

    private static string PropertyPath(Expression expression)
    {
        var parts = new Stack<string>();
        while (expression is MemberExpression { Member: PropertyInfo prop } member)
        {
            parts.Push(prop.Name);
            expression = member.Expression!;
        }
        if (expression is not ParameterExpression || parts.Count == 0)
            throw new ArgumentException("Поддерживаются только пути свойств, например x => x.Profile.Name.");
        return string.Join('.', parts);
    }
}

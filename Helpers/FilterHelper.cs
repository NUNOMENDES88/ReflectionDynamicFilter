namespace ReflectionDynamicFilter.Helpers
{
    using Enumerations;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class FilterHelper
    {
        //Get the methods by reflection
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] {typeof(string)});
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});

        public static IEnumerable<T> Filter<T>(
            this IEnumerable<T> source,
            List<FilterModel> filterList)
        {
            if (source == null)
                return null;

            var expressionFilter = source.GenerateExpressionFilter(filterList);
            var predicate = expressionFilter.Compile();
            return source.Where(predicate);
        }

        private static Expression<Func<T, bool>> GenerateExpressionFilter<T>(
            this IEnumerable<T> source,
            List<FilterModel> filterList)
        {
            if (filterList.Count == 0)
                return null;

            ParameterExpression parameter = Expression.Parameter(typeof(T), "t");
            //Get expression for first parameter
            Expression exp = GetExpression<T>(parameter, filterList[0]);

            if (filterList.Count > 1)
            {
                for (int i = 1; i < filterList.Count; i++)
                {
                    Expression filter = GetExpression<T>(parameter, filterList[i]);
                    if (filterList[i].LogicalOperator == LogicalOperatorsEnum.And)
                    {
                        exp = Expression.And(exp, filter);
                    }
                    else
                    {
                        exp = Expression.Or(exp, filter);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(exp, parameter);
        }

        private static Expression GetExpression<T>(
            ParameterExpression parameterExpression,
            FilterModel filterModel)
        {
            MemberExpression property = Expression.Property(parameterExpression, filterModel.PropertyName);
            ConstantExpression constant = Expression.Constant(filterModel.Value);
            switch (filterModel.RelationalOperator)
            {
                case RelationalOperatorsEnum.Equal:
                    return Expression.Equal(property, constant);
                case RelationalOperatorsEnum.GreaterThan:
                    return Expression.GreaterThan(property, constant);
                case RelationalOperatorsEnum.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(property, constant);
                case RelationalOperatorsEnum.LessThan:
                    return Expression.LessThan(property, constant);
                case RelationalOperatorsEnum.LessThanOrEqual:
                    return Expression.LessThanOrEqual(property, constant);
                case RelationalOperatorsEnum.NotEqual:
                    return Expression.NotEqual(property, constant);
                case RelationalOperatorsEnum.Contains:
                    return Expression.Call(property, ContainsMethod, constant);
                case RelationalOperatorsEnum.StartsWith:
                    return Expression.Call(property, StartsWithMethod, constant);
                case RelationalOperatorsEnum.EndsWith:
                    return Expression.Call(property, EndsWithMethod, constant);
                default:
                    return null;
            }
        }

    }
}

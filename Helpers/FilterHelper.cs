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
        private static readonly MethodInfo _containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
        private static readonly MethodInfo _startsWithMethod = typeof(string).GetMethod("StartsWith", new[] {typeof(string)});
        private static readonly MethodInfo _endsWithMethod = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});

        public static IEnumerable<T> Filter<T>(
            this IEnumerable<T> source,
            List<FilterModel> filterList)
        {
            if (source == null)
                return null;
            Func<T, bool> predicate = GeneratePredicate<T>(filterList);
            return source.Where(predicate);
        }

        public static Func<T, bool> GeneratePredicate<T>(List<FilterModel> filterList)
        {
            var expressionFilter = GenerateExpressionFilter<T>(filterList);
            var predicate = expressionFilter.Compile();
            return predicate;
        }

        private static Expression<Func<T, bool>> GenerateExpressionFilter<T>(List<FilterModel> filterList)
        {
            if (filterList.Count == 0)
                return null;

            ParameterExpression objectParameter = Expression.Parameter(typeof(T), "t");
            //Get expression for first parameter
            Expression exp = GetExpression<T>(objectParameter, filterList[0]);

            if (filterList.Count > 1)
            {
                for (int i = 1; i < filterList.Count; i++)
                {
                    Expression filter = GetExpression<T>(objectParameter, filterList[i]);
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

            return Expression.Lambda<Func<T, bool>>(exp, objectParameter);
        }

        private static Expression GetExpression<T>(
            ParameterExpression objectParameter,
            FilterModel filterModel)
        {
            
            MemberExpression property = GetMemberExpression(objectParameter, filterModel.PropertyName);
            ConstantExpression constant = Expression.Constant(filterModel.Value);
            return filterModel.RelationalOperator switch
            {
                RelationalOperatorsEnum.Equal => Expression.Equal(property, constant),
                RelationalOperatorsEnum.GreaterThan => Expression.GreaterThan(property, constant),
                RelationalOperatorsEnum.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
                RelationalOperatorsEnum.LessThan => Expression.LessThan(property, constant),
                RelationalOperatorsEnum.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
                RelationalOperatorsEnum.NotEqual => Expression.NotEqual(property, constant),
                RelationalOperatorsEnum.Contains => Expression.Call(property, _containsMethod, constant),
                RelationalOperatorsEnum.StartsWith => Expression.Call(property, _startsWithMethod, constant),
                RelationalOperatorsEnum.EndsWith => Expression.Call(property, _endsWithMethod, constant),
                _ => null,
            };
        }

        public static MemberExpression GetMemberExpression(this Expression target, string fullPropertyName)
        {
            var listProperties = fullPropertyName.Split(".").ToList();

            string propertyName;
            PropertyInfo property;
            MemberExpression memberExpression;
            if (listProperties.Count < 1)
            {
                throw new InvalidOperationException("The value is empty");
            }
            propertyName = listProperties[0];
            property = target.Type.GetProperty(propertyName);
            if (property == null)
            {
                throw new MissingMemberException("Failed to retrieve a property with name '" + propertyName + "' in type '" + target.Type.FullName + "'");
            }
            memberExpression = Expression.MakeMemberAccess(target, property);
            target = memberExpression;
            for (int index = 1; index < listProperties.Count; index++)
            {
                propertyName = listProperties[index];
                property = target.Type.GetProperty(propertyName);
                if (property == null)
                {
                    throw new MissingMemberException("Failed to retrieve a property with name '" + propertyName + "' in type '" + target.Type.FullName + "'");
                }
                memberExpression = Expression.MakeMemberAccess(target, property);
                target = memberExpression;
            }
            return memberExpression;
        }
    }
}

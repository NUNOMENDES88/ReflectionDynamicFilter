namespace ReflectionDynamicFilter.Helpers
{
    using Enumerations;
    using Models;
    using ReflectionDynamicFilter.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    public static class FilterHelper
    {
        //Get the methods by reflection
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        public static IQueryable<T> Filter<T>(
            this IEnumerable<T> source,
            List<FilterModel> filterList)
        {
            if (!source.CheckNotNullAndAny())
            {
                return null;
            }

            if (!filterList.CheckNotNullAndAny())
            {
                return source.AsQueryable();
            }

            try
            {
                Func<T, bool> predicate = GeneratePredicate<T>(filterList);
                return source.Where(predicate).AsQueryable();
            }
            catch (Exception)
            {
                throw new Exception("Error Invalid Filter Field");
            }
        }

        public static Func<T, bool> GeneratePredicate<T>(List<FilterModel> filterList)
        {
            var expressionFilter = GenerateExpressionFilter<T>(filterList);
            var predicate = expressionFilter.Compile();
            return predicate;
        }

        public static Expression<Func<T, bool>> GenerateExpressionFilter<T>(
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
            FilterModel FilterModel)
        {
            Expression memberExpression = GetMemberExpression(parameterExpression, FilterModel.PropertyName);
            Expression property = memberExpression.ToLowerProperty();

            object filterValue = FilterModel.Value.ParseData(property.Type);
            ConstantExpression constant = filterValue.ToLowerContant();
            Expression filterData = constant;

            bool isNullableGenericType = (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
            if (isNullableGenericType)
            {
                filterData = Expression.Convert(constant, memberExpression.Type);
            }

            Expression selectedExpression;
            switch (FilterModel.RelationalOperator)
            {
                case RelationalOperatorsEnum.Equal:
                    selectedExpression = Expression.Equal(property, filterData);
                    break;
                case RelationalOperatorsEnum.GreaterThan:
                    selectedExpression = Expression.GreaterThan(property, filterData);
                    break;
                case RelationalOperatorsEnum.GreaterThanOrEqual:
                    selectedExpression = Expression.GreaterThanOrEqual(property, filterData);
                    break;
                case RelationalOperatorsEnum.LessThan:
                    selectedExpression = Expression.LessThan(property, filterData);
                    break;
                case RelationalOperatorsEnum.LessThanOrEqual:
                    selectedExpression = Expression.LessThanOrEqual(property, filterData);
                    break;
                case RelationalOperatorsEnum.NotEqual:
                    selectedExpression = Expression.NotEqual(property, filterData);
                    break;
                case RelationalOperatorsEnum.Contains:
                    selectedExpression = Expression.Call(property, ContainsMethod, filterData);
                    break;
                case RelationalOperatorsEnum.StartsWith:
                    selectedExpression = Expression.Call(property, StartsWithMethod, filterData);
                    break;
                case RelationalOperatorsEnum.EndsWith:
                    selectedExpression = Expression.Call(property, EndsWithMethod, filterData);
                    break;
                case RelationalOperatorsEnum.NotContains:
                    var expression = Expression.Call(property, ContainsMethod, filterData);
                    selectedExpression = Expression.Not(expression);
                    break;
                default:
                    selectedExpression = null;
                    break;
            }

            return selectedExpression.CheckIfPropertysIsNullAndIncrementInConditionExpression(parameterExpression, FilterModel.PropertyName);
        }

        private static Expression CheckIfPropertysIsNullAndIncrementInConditionExpression(this Expression selectedExpression, ParameterExpression parameterExpression, string fullPropertyName)
        {
            List<Expression> listExpressions = new List<Expression>();
            //Expression endCondition = selectedExpression;
            string propertyName = "";
            try
            {
                var listProperties = fullPropertyName.Split(".").ToList();
                if (listProperties.Count < 1)
                {
                    throw new InvalidOperationException("The value is empty");
                }

                Expression fatherProperty = Expression.PropertyOrField(parameterExpression, listProperties[0]);
                Expression childProperty = fatherProperty;
                bool isNullableGenericType;

                for (int index = 0; index < listProperties.Count; index++)
                {

                    propertyName = listProperties[index];
                    if (index > 0)
                    {
                        childProperty = Expression.PropertyOrField(fatherProperty, listProperties[index]);
                    }
                    fatherProperty = childProperty;
                    isNullableGenericType = (childProperty.Type.IsGenericType && childProperty.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));

                    if ((childProperty.Type == typeof(string) || isNullableGenericType || childProperty.Type.IsClass) && selectedExpression != null)
                    {
                        Expression notNull = Expression.NotEqual(childProperty, Expression.Constant(null));
                        listExpressions.Add(notNull);
                    }
                }

                Expression endExpression = selectedExpression;

                for (int index = listExpressions.Count(); index != 0; index--)
                {
                    int expressionIndex = index - 1;
                    endExpression = Expression.AndAlso(listExpressions[expressionIndex], endExpression);
                }

                return endExpression;
            }
            catch (ArgumentException)
            {
                throw new MissingMemberException("Failed to retrieve a property with name '" + propertyName + "' in type '" + selectedExpression.Type.FullName + "'");
            }
        }

        private static Expression ToLowerProperty(this Expression property)
        {

            if (property.Type == typeof(string))
            {
                var lowerExpression = Expression.Call(property, "ToLower", null);
                return lowerExpression;
            }

            return property;
        }

        private static ConstantExpression ToLowerContant(this object filterValue)
        {
            object newValue = filterValue;
            if (filterValue.GetType() == typeof(string))
            {
                newValue = filterValue.ToString().ToLower();
            }
            return Expression.Constant(newValue);
        }

        private static Expression GetMemberExpression(this Expression target, string fullPropertyName)
        {
            string propertyName = "";
            try
            {
                var listProperties = fullPropertyName.Split(".").ToList();
                if (listProperties.Count < 1)
                {
                    throw new InvalidOperationException("The value is empty");
                }

                propertyName = listProperties[0];
                Expression child = Expression.PropertyOrField(target, propertyName);
                Expression childProperty = child;

                for (int index = 1; index < listProperties.Count; index++)
                {
                    propertyName = listProperties[index];
                    childProperty = Expression.PropertyOrField(child, listProperties[index]);
                    child = childProperty;
                }
                return childProperty;
            }
            catch (ArgumentException)
            {
                throw new MissingMemberException("Failed to retrieve a property with name '" + propertyName + "' in type '" + target.Type.FullName + "'");
            }

        }
    }
}
namespace ReflectionDynamicFilter.Models
{
    using Enumerations;

    public class FilterModel
    {

        public FilterModel(
            string propertyName,
            object value,
            RelationalOperatorsEnum relationalOperator)
        {
            PropertyName = propertyName;
            Value = value;
            RelationalOperator = relationalOperator;
            LogicalOperator = LogicalOperatorsEnum.And;
        }

        public FilterModel(
            string propertyName,
            object value,
            RelationalOperatorsEnum relationalOperator,
            LogicalOperatorsEnum logicalOperator):this(propertyName, value, relationalOperator)
        {
            LogicalOperator = logicalOperator;
        }

        public string PropertyName { get; set; }
        public object Value { get; set; }
        public RelationalOperatorsEnum RelationalOperator { get; set; }
        public LogicalOperatorsEnum LogicalOperator { get; set; }
    }
}

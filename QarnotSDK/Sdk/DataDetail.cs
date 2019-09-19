using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace QarnotSDK
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class InternalDataApiName : Attribute
    {
        public  virtual string Name { get; set; } = null;
        public virtual bool IsFilterable { get; set; } = true;
        public virtual bool IsSelectable { get; set; } = true;
        public InternalDataApiName()
        { }
    }

    internal class DataDetailHelper {
        public static PropertyInfo GetPropertyInfo<T, P>(Expression<Func<T, P>> property)
        {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.Body is UnaryExpression unaryExp) {
                if (unaryExp.Operand is MemberExpression memberExp) {
                    return (PropertyInfo)memberExp.Member;
                }
            }
            else if (property.Body is MemberExpression memberExp) {
                return (PropertyInfo)memberExp.Member;
            }

            throw new ArgumentException($"The expression doesn't indicate a valid property. [ {property} ]");
        }


        /// <summary>
        ///     Gets the corresponding <see cref="PropertyInfo" /> from an <see cref="Expression" />.
        /// </summary>
        /// <param name="property">The expression that selects the property to get info on.</param>
        /// <returns>The property info collected from the expression.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The expression doesn't indicate a valid property."</exception>
        internal static string GetAPISelectPropertyName<T, P>(Expression<Func<T, P>> property)
        {
            var pi = GetPropertyInfo(property);
            var attr = pi.GetCustomAttribute<InternalDataApiName>();
            if (attr == null || !attr.IsSelectable)
                throw new Exception($"The property {pi.Name} should not be selected.");
            return attr.Name;
        }

        /// <summary>
        ///     Gets the corresponding <see cref="PropertyInfo" /> from an <see cref="Expression" />.
        /// </summary>
        /// <param name="property">The expression that selects the property to get info on.</param>
        /// <returns>The property info collected from the expression.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The expression doesn't indicate a valid property."</exception>
        internal static string GetAPIFilterPropertyName<T, P>(Expression<Func<T, P>> property)
        {
            var pi = GetPropertyInfo(property);
            var attr = pi.GetCustomAttribute<InternalDataApiName>();
            if (attr == null || !attr.IsFilterable)
                throw new Exception($"The property {pi.Name} should not be filtered.");
            return attr.Name;
        }
    }


    /// <summary>
    /// Object holding selections and filtering for a specified class
    /// </summary>
    /// <typeparam name="T">the concerned object</typeparam>
    public class QDataDetail<T>
    {
        internal DataDetailApi<T> _dataDetailApi = new DataDetailApi<T>();


        /// <summary>
        /// The filtering part, specification logical filters on the object properties
        /// </summary>
        /// <value>the filter</value>
        public QFilter<T> Filter
        {
            get => new QFilter<T>() { _filterApi = _dataDetailApi.Filter };
            set => _dataDetailApi.Filter = value._filterApi;
        }

        /// <summary>
        /// The Selection part, specificating the fields to retrieve
        /// </summary>
        /// <value>the select clause</value>
        public QSelect<T> Select
        {
            get => new QSelect<T>() { _selectApi = _dataDetailApi.Select };
            set => _dataDetailApi.Select = value._selectApi;
        }

        /// <summary>
        /// Maximum results number
        /// </summary>
        /// <value>the maximum result for the query</value>
        public int? MaximumResults
        {
            get => _dataDetailApi.MaximumResults;
            set => _dataDetailApi.MaximumResults = value;
        }

        /// <summary>
        /// Override to string
        /// </summary>
        /// <returns>string format</returns>
        public override string ToString()
        {
            return $"<Select: {Select}, Filter: {Filter}, MaxNumber: {MaximumResults}>";
        }
    }


    /// <summary>
    /// Select applied to the class T
    /// Only the choosen propertier are going to be retrieved
    /// </summary>
    /// <typeparam name="T">the class on which the selection apply</typeparam>
    public class QSelect<T>
    {
        internal SelectApi<T> _selectApi = new SelectApi<T>();

        /// <summary>
        /// Include a field in the projection
        /// </summary>
        /// <param name="property">the field to retrieve</param>
        public virtual QSelect<T> Include<P>(Expression<Func<T, P>> property)
        {
            // old
            var fieldName = DataDetailHelper.GetAPISelectPropertyName(property);
            _selectApi.Fields.Add(fieldName);
            return this;
        }

        /// <summary>
        /// Create a qselect object with a corresponding field
        /// </summary>
        /// <param name="property">the fields to retrieve</param>
        /// <returns>qselect filter</returns>
        public static QSelect<T> Select<P>(Expression<Func<T, P>> property)
        {
            var qselect = new QSelect<T>();
            return qselect.Include<P>(property);
        }

        /// <summary>
        /// Create an ampty qselect object
        /// </summary>
        /// <returns>qselect filter</returns>
        public static QSelect<T> Select() => new QSelect<T>();


        /// <summary>
        /// Override to string
        /// </summary>
        /// <returns>string format</returns>
        public override string ToString()
        {
            return $"<Fields: {(_selectApi == null ? default(string) : string.Join(",", _selectApi.Fields))}>";
        }
    }

    /// <summary>
    /// Filtering applied to the class T
    /// </summary>
    /// <typeparam name="T">The class on which thw filter apply</typeparam>
    public class QFilter<T>
    {
        internal FilterApi<T> _filterApi;

        /// <summary>
        /// Logical And operator between multiple filters
        /// </summary>
        /// <param name="filters">The filters that should be linked logically</param>
        /// <returns>the linked filters</returns>
        public static QFilter<T> And(params QFilter<T>[] filters)
        {
            var internalFilter = new Node<T>()
            {
                Operator = Node<T>.LogicalOperator.And,
                Filters = filters.Select(f => f._filterApi).ToArray()
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Or operator between multiple filters
        /// </summary>
        /// <param name="filters">The filters that should be linked logically</param>
        /// <returns>the linked filters</returns>
        public static QFilter<T> Or(params QFilter<T>[] filters)
        {
            var internalFilter = new Node<T>()
            {
                Operator = Node<T>.LogicalOperator.Or,
                Filters = filters.Select(f => f._filterApi).ToArray()
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Equal filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the value it should be equal to</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Eq<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.Equal,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Not Equal filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the value it should not be equal to</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Neq<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.NotEqual,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical In filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="values">The array of possible value for the property</param>
        /// <returns>the filter</returns>
        public static QFilter<T> In<VType>(Expression<Func<T, VType>> property, params VType[] values)
        {
            var internalFilter = new MultipleValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.In,
                Values = values
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Not In filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="values">The array of forbiden value for the property</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Nin<VType>(Expression<Func<T, VType>> property, params VType[] values)
        {
            var internalFilter = new MultipleValueLeaf<T,VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.NotIn,
                Values = values
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Less than filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the value it should less than</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Lt<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.LessThan,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Less than or equal filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the value it should less than or equal</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Lte<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.LessThanOrEqual,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Greater than or equal filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the value it should greater than or equal</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Gte<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.GreaterThanOrEqual,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Like regex filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the regex value it should match</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Like<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T, VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.Like,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Logical Greater than filter
        /// </summary>
        /// <param name="property">the property to filter</param>
        /// <param name="value">the value it should greater than</param>
        /// <returns>the filter</returns>
        public static QFilter<T> Gt<VType>(Expression<Func<T, VType>> property, VType value)
        {
            var internalFilter = new UnitValueLeaf<T,VType>
            {
                Field = DataDetailHelper.GetAPIFilterPropertyName(property),
                Operator = Leaf<T>.FilterOperator.GreaterThan,
                Value = value
            };
            return new QFilter<T>() { _filterApi = internalFilter };
        }

        /// <summary>
        /// Override to string
        /// </summary>
        /// <returns>string format</returns>
        public override string ToString()
        {
            return $"<Filtering: {_filterApi}>";
        }
    }
}
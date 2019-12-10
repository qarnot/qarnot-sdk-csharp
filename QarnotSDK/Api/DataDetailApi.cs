using System.Linq;
using System.Collections.Generic;

namespace QarnotSDK
{
    internal class DataDetailApi<T>
    {
        public SelectApi<T> Select;
        public FilterApi<T> Filter;
        public int? MaximumResults;
    }

    internal class SelectApi<T>
    {
        public List<string> Fields = new List<string>();
    }

    internal abstract class FilterApi<T> { }

    internal class Node<T> : FilterApi<T>
    {
        public class LogicalOperator
        {
            public static string And = "And";
            public static string Or = "Or";
        }

        public string Operator;
        public FilterApi<T>[] Filters;

        public override string ToString()
        {
            if (Filters == null) return "";
            var filterString = Filters == null ?
                default(string) :
                string.Join($" {Operator} ", Filters.Select(filter => $"({filter})"));
            return $"<Query: {filterString}>";
        }
    }

    internal abstract class Leaf<T> : FilterApi<T>
    {
        public class FilterOperator
        {
            public static readonly string Equal = "Equal";
            public static readonly string NotEqual = "NotEqual";
            public static readonly string In = "In";
            public static readonly string NotIn = "NotIn";
            public static readonly string LessThanOrEqual = "LessThanOrEqual";
            public static readonly string LessThan = "LessThan";
            public static readonly string GreaterThanOrEqual = "GreaterThanOrEqual";
            public static readonly string GreaterThan = "GreaterThan";
            public static readonly string Like = "Like";
        }

        public string Field;
        public string Operator;
    }

    internal class UnitValueLeaf<T, VType> : Leaf<T>
    {
        public VType Value;

        public override string ToString() => $"{Field} {Operator} {Value}";
    }

    internal class MultipleValueLeaf<T, VType> : Leaf<T>
    {
        public VType[] Values;

        public override string ToString() => $"{Field} {Operator} {(Values == null ? default(string) : string.Join(",", Values))}";
    }
}
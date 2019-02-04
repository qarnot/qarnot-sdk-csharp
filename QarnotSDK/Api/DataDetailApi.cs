using System;
using System.Linq;
using System.Collections.Generic;

namespace QarnotSDK
{
    internal class DataDetailApi<T>
    {
        public SelectApi<T> Select;
        public FilterApi<T> Filter;
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
            public static string Equal = "Equal";
            public static string NotEqual = "NotEqual";
            public static string In = "In";
            public static string NotIn = "NotIn";
            public static string LessThanOrEqual = "LessThanOrEqual";
            public static string LessThan = "LessThan";
            public static string GreaterThanOrEqual = "GreaterThanOrEqual";
            public static string GreaterThan = "GreaterThan";
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
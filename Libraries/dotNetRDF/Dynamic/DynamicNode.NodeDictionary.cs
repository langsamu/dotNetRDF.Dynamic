﻿namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Nodes;

    public partial class DynamicNode : IDictionary<INode, object>
    {
        private IEnumerable<IUriNode> PredicateNodes => Graph.GetTriplesWithSubject(this).Select(t => t.Predicate as IUriNode).Distinct();

        public object this[INode key]
        {
            get
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (Graph is null)
                {
                    throw new InvalidOperationException("Node must have graph");
                }

                if (!TryGetValue(key, out var objects))
                {
                    throw new KeyNotFoundException();
                }

                return objects;
            }

            set
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                this.Remove(key);

                if (value is null)
                {
                    return;
                }

                this.Add(key, value);
            }
        }

        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return PredicateNodes.ToArray();
            }
        }

        public void Add(INode key, object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Graph.Assert(this.ConvertToTriples(key, value));
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            if (item.Key is null)
            {
                throw new ArgumentNullException("key");
            }

            return Graph.GetTriplesWithSubjectPredicate(this, item.Key).WithObject(ConvertToNode(item.Value)).Any();
        }

        public bool ContainsKey(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return 
                Graph
                .GetTriplesWithSubjectPredicate(this, key)
                .Any();
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            (this as IEnumerable<KeyValuePair<INode, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return 
                PredicateNodes
                .ToDictionary(
                    predicate => predicate as INode,
                    predicate => this[predicate])
                .GetEnumerator();
        }

        public bool Remove(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, key).ToArray());
        }

        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            if (item.Key is null)
            {
                throw new ArgumentNullException("key");
            }

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, item.Key).WithObject(ConvertToNode(item.Value)).ToArray());
        }

        public bool TryGetValue(INode key, out object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            value = new DynamicObjectCollection(this, key);

            return true;
        }

        private IEnumerable<Triple> ConvertToTriples(INode predicateNode, object value)
        {
            if (value is null)
            {
                yield break;
            }

            if (value is string || !(value is IEnumerable enumerableValue)) // Strings are enumerable but not for our case
            {
                enumerableValue = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var item in enumerableValue)
            {
                // TODO: Maybe this should throw on null
                if (item != null)
                {
                    yield return new Triple(
                        subj: Node,
                        pred: predicateNode,
                        obj: ConvertToNode(item),
                        g: Node.Graph);
                }
            }
        }

        private INode ConvertToNode(object value)
        {
            switch (value)
            {
                case INode nodeValue:
                    return nodeValue.CopyNode(Graph);

                case Uri uriValue:
                    return Graph.CreateUriNode(uriValue);

                case bool boolValue:
                    return new BooleanNode(Graph, boolValue);

                case byte byteValue:
                    return new ByteNode(Graph, byteValue);

                case DateTime dateTimeValue:
                    return new DateTimeNode(Graph, dateTimeValue);

                case DateTimeOffset dateTimeOffsetValue:
                    return new DateTimeNode(Graph, dateTimeOffsetValue);

                case decimal decimalValue:
                    return new DecimalNode(Graph, decimalValue);

                case double doubleValue:
                    return new DoubleNode(Graph, doubleValue);

                case float floatValue:
                    return new FloatNode(Graph, floatValue);

                case long longValue:
                    return new LongNode(Graph, longValue);

                case int intValue:
                    return new LongNode(Graph, intValue);

                case string stringValue:
                    return new StringNode(Graph, stringValue);

                case char charValue:
                    return new StringNode(Graph, charValue.ToString());

                case TimeSpan timeSpanValue:
                    return new TimeSpanNode(Graph, timeSpanValue);

                default:
                    throw new InvalidOperationException($"Can't convert type {value.GetType()}");
            }
        }
    }
}
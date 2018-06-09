﻿namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using VDS.RDF;

    // TODO: Remove subjectBaseUri in favour of just BaseUri?
    public class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider, ISimpleDynamicObject
    {
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;

        private Uri SubjectBaseUri
        {
            get
            {
                return this.subjectBaseUri ?? this.BaseUri;
            }
        }

        private Uri PredicateBaseUri
        {
            get
            {
                return this.predicateBaseUri ?? this.SubjectBaseUri;
            }
        }


        public DynamicGraph(IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null) : base(graph)
        {
            this.subjectBaseUri = subjectBaseUri;
            this.predicateBaseUri = predicateBaseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);

        object ISimpleDynamicObject.GetIndex(object[] indexes)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var subjectIndex = indexes[0] ?? throw new ArgumentNullException("Can't work with null index", "indexes"); ;

            return this.GetIndex(subjectIndex) ?? throw new Exception("index not found");
        }

        object ISimpleDynamicObject.GetMember(string name)
        {
            if (this.SubjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't get member without baseUri.");
            }

            return this.GetIndex(name) ?? throw new Exception("member not found");
        }

        object ISimpleDynamicObject.SetIndex(object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            this.SetIndex(indexes[0], value);

            return value;
        }

        object ISimpleDynamicObject.SetMember(string name, object value)
        {
            if (this.SubjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            this.SetIndex(name, value);

            return value;
        }

        IEnumerable<string> ISimpleDynamicObject.GetDynamicMemberNames()
        {
            var subjects = this
                .Triples
                .Select(triple => triple.Subject)
                .UriNodes()
                .Distinct();

            return DynamicHelper.ConvertToNames(subjects, this.SubjectBaseUri);
        }

        private DynamicNode GetIndex(object subject)
        {
            var subjectNode = DynamicHelper.ConvertToNode(subject, this, this.SubjectBaseUri);

            return this.Triples
                .SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(subjectNode))
                .Select(node => node.AsDynamic(this.PredicateBaseUri))
                .SingleOrDefault();
        }

        private void SetIndex(object index, object value)
        {
            var targetNode = this.GetDynamicNodeByIndexOrCreate(index);

            if (value == null)
            {
                this.RetractWithSubject(targetNode);

                return;
            }

            var valueDictionary = DynamicGraph.ConvertToDictionary(value);
            var dynamicTarget = targetNode as dynamic;

            foreach (DictionaryEntry entry in valueDictionary)
            {
                dynamicTarget[entry.Key] = entry.Value;
            }
        }

        private DynamicNode GetDynamicNodeByIndexOrCreate(object subjectIndex)
        {
            var indexNode = DynamicHelper.ConvertToNode(subjectIndex, this, this.SubjectBaseUri);

            return this.GetIndex(indexNode) ?? indexNode.AsDynamic(this.PredicateBaseUri);
        }

        private void RetractWithSubject(DynamicNode targetNode)
        {
            var triples = this.GetTriplesWithSubject(targetNode).ToArray();

            this.Retract(triples);
        }

        private static IDictionary ConvertToDictionary(object value)
        {
            if (!(value is IDictionary valueDictionary))
            {
                valueDictionary = new Dictionary<object, object>();

                var properties = value.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Where(p => !p.GetIndexParameters().Any());

                if (!properties.Any())
                {
                    throw new ArgumentException($"Value type {value.GetType()} lacks readable public instance properties.", "value");
                }

                foreach (var property in properties)
                {
                    valueDictionary[property.Name] = property.GetValue(value);
                }
            }

            return valueDictionary;
        }
    }
}

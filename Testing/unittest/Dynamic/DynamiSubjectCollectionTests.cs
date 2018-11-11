﻿namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Linq;
    using VDS.RDF;
    using Xunit;

    public class DynamicSubjectCollectionTests
    {
        [Fact]
        public void Requires_predicate()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamicSubjectCollection(null, null)
            );
        }

        [Fact]
        public void Requires_object()
        {
            var g = new Graph();
            var p = g.CreateBlankNode();

            Assert.Throws<ArgumentNullException>(() =>
                new DynamicSubjectCollection(p, null)
            );
        }

        [Fact]
        public void Counts_by_predicate_and_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # 1
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> . # 2
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> . # 3
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(o);
            var c = new DynamicSubjectCollection(p, d);

            Assert.Equal(3, c.Count());
        }

        [Fact]
        public void Is_writable()
        {
            var g = new Graph();
            var p = g.CreateBlankNode();
            var o = g.CreateBlankNode();
            var d = new DynamicNode(o);
            var c = new DynamicSubjectCollection(p, d);

            Assert.False(c.IsReadOnly);
        }

        [Fact]
        public void Add_asserts_with_predicate_object_and_argument_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var g = new Graph();
            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(o);
            var c = new DynamicSubjectCollection(p, d);

            c.Add(s);

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Clear_retracts_by_predicate_and_object()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
# <urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
# <urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
# <urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # should retract
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> . # should retract
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> . # should retract
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(o);
            var c = new DynamicSubjectCollection(p, d);

            c.Clear();

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Contains_reports_by_predicate_object_and_argument_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # true
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> . # true
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> . # true
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(s);
            var c = new DynamicSubjectCollection(p, d);

            Assert.Contains(s, c);
            Assert.Contains(p, c);
            Assert.Contains(o, c);
            Assert.Equal(3, c.Count());
        }

        [Fact]
        public void Copies_subjects_by_predicate_and_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # 1
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> . # 2
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> . # 3
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(s);
            var c = new DynamicSubjectCollection(p, d);

            var objects = new object[5]; // +2 for padding on each side
            c.CopyTo(objects, 1); // start at the second item at destination

            Assert.Equal(
                new[] { null, s, p, o, null },
                objects);
        }

        [Fact]
        public void Enumerates_subjects_by_predicate_and_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # 1
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> . # 2
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> . # 3
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(s);
            var c = new DynamicSubjectCollection(p, d);

            var expected = new[] { s, p, o }.GetEnumerator();
            using (var actual = c.GetEnumerator())
            {
                while (expected.MoveNext())
                {
                    actual.MoveNext();

                    Assert.Equal(
                        expected.Current,
                        actual.Current);
                }
            }
        }

        [Fact]
        public void Remove_retracts_by_predicate_object_and_argument_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
# <urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # should retract
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(o);
            var c = new DynamicSubjectCollection(p, d);

            c.Remove(s);

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void IEnumerable_enumerates_subjects_by_predicate_and_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # 1
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> . # 2
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> . # 3
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(s);
            var c = new DynamicSubjectCollection(p, d) as IEnumerable;

            var expected = new[] { s, p, o }.GetEnumerator();
            var actual = c.GetEnumerator();
            while (expected.MoveNext())
            {
                actual.MoveNext();

                Assert.Equal(
                    expected.Current,
                    actual.Current);
            }
        }

        [Fact]
        public void Provides_meta_object()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");

            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var o = g.CreateUriNode(new Uri("urn:o"));
            var d = new DynamicNode(o);
            dynamic c = new DynamicSubjectCollection(p, d);

            Assert.Equal(s, c.Single());
        }
    }
}
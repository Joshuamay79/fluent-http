﻿using System;
using System.Collections.Specialized;
using System.Threading;
using NUnit.Framework;

namespace AonWeb.FluentHttp.Tests.Http
{
    [TestFixture]
    public class HelperTests
    {
        [Test]
        public void MergeAction_WhenCalled_ExpectMerge()
        {
            var action1Called = new DateTime?();
            var action2Called = new DateTime?();
            Action<int> action1 = i => { action1Called = DateTime.Now; Thread.Sleep(1); };
            Action<int> action2 = i => { action2Called = DateTime.Now; Thread.Sleep(1); };

            var actual = Helper.MergeAction(action1, action2);

            actual(1);

            Assert.NotNull(action1Called, "Action 1 was not called");
            Assert.NotNull(action2Called, "Action 2 was not called");
        }

        [Test]
        public void MergeAction_WhenCalled_ExpectCorrectOrder()
        {
            var action1Called = new DateTime?();
            var action2Called = new DateTime?();
            Action<int> action1 = i => { action1Called = DateTime.Now; Thread.Sleep(1); };
            Action<int> action2 = i => { action2Called = DateTime.Now; Thread.Sleep(1); };

            var actual = Helper.MergeAction(action1, action2);

            actual(1);

            Assert.Less(action1Called, action2Called);
        }

        [Test]
        public void MergeAction_WhenCalledWithNulls_ExpectNoException()
        {
            var actual = Helper.MergeAction<int>(null, null);

            actual(1);

            Assert.Pass();
        }

        [Test]
        public void MergeAction_WhenCalledWithFirstNull_ExpectNoException()
        {
            Action<int> action = i => { };

            var actual = Helper.MergeAction(null, action);

            actual(1);

            Assert.Pass();
        }

        [Test]
        public void MergeAction_WhenCalledWithSecondNull_ExpectNoException()
        {
            Action<int> action = i => { };

            var actual = Helper.MergeAction(action, null);

            actual(1);

            Assert.Pass();
        }

        [Test]
        [TestCase("http://somedomain.com", "q", "1", "http://somedomain.com?q=1")]
        [TestCase("http://somedomain.com", "q", "1 and 2", "http://somedomain.com?q=1+and+2")]
        [TestCase("http://somedomain.com", "q", null, "http://somedomain.com?q=")]
        [TestCase("http://somedomain.com", "q", "", "http://somedomain.com?q=")]
        [TestCase("http://somedomain.com", null, "1", "http://somedomain.com")]
        public void AppendToQueryString_WithValidInput_ExpectCorrectOutput(string url, string key, string value, string expected)
        {
            var uri = new Uri(url);
            var expectedUri = new Uri(expected);

            var actualUri = Helper.AppendToQueryString(uri, key, value);

            Assert.AreEqual(expectedUri, actualUri);
        }

        [Test]
        public void AppendToQueryString_WithMultipleKeys_ExpectCorrectOutput()
        {
            var uri = new Uri("http://www.somedomain.com?q1=1");
            var keys = new NameValueCollection { { "q1", "2" }, { "q2", "1 and 2" }, { "q3", "3" } };
            var expected = new Uri("http://www.somedomain.com?q1=2&q2=1+and+2&q3=3");

            var actual = Helper.AppendToQueryString(uri, keys);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void AppendToQueryString_WithNullValues_ExpectSameUri()
        {
            var expected = new Uri("http://www.somedomain.com");

            var actual = Helper.AppendToQueryString(expected, null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AppendToQueryString_WithNoValues_ExpectSameUri()
        {
            var expected = new Uri("http://www.somedomain.com");

            var actual = Helper.AppendToQueryString(expected, new NameValueCollection());

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AppendToQueryString_WhenValueAlreadyExists_ExpectValueReplaced()
        {
            var uri = new Uri("http://www.somedomain.com?q=1");
            var expected = new Uri("http://www.somedomain.com?q=2");

            var actual = Helper.AppendToQueryString(uri, new NameValueCollection { { "q", "2" } });

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppendToQueryString_WithUriNull_ExpectException()
        {

            Helper.AppendToQueryString(null, null);
        }
    }
}

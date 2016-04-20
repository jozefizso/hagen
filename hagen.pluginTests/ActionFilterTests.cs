﻿using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.CommandLine;
using System.Reactive.Linq;

namespace hagen.Tests
{
    [TestFixture()]
    public class ActionFilterTests
    {
        public ActionFilterTests()
        {
            sampleApp = new SampleApp();
            var p = Parser.SingleSource(sampleApp);
            af = new CommandLineParserActionSource(new hagen.Test.ContextMock(), p);
        }

        SampleApp sampleApp;
        CommandLineParserActionSource af;

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("SampleApp", af.ToString());
        }

        [Test]
        public void GetActions()
        {
            Assert.AreEqual(1, af.GetActions(Query.Parse("A")).ToEnumerable().Count());
            Assert.AreEqual(0, af.GetActions(Query.Parse("B")).ToEnumerable().Count());
            af.GetActions(Query.Parse("SomeAction")).FirstAsync().Wait().Action.Execute();
            Assert.IsTrue(sampleApp.SomeActionExecuted);
        }
    }
}
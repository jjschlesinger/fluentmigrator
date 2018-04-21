#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Diagnostics;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.DB2.iSeries;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2.iSeries;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Db2ISeries
{
    [TestFixture]
    [Category("Integration")]
    [Category("DB2 iSeries")]
    public class Db2ISeriesSchemaTests : BaseSchemaTests
    {
        static Db2ISeriesSchemaTests()
        {
            try { EnsureReference(); } catch { /* ignore */ }
        }

        public System.Data.IDbConnection Connection
        {
            get;
            set;
        }

        public Db2ISeriesProcessor Processor
        {
            get;
            set;
        }

        public Db2ISeriesQuoter Quoter
        {
            get;
            set;
        }

        [Test]
        public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
        {
            Processor.SchemaExists("DNE").ShouldBeFalse();
        }

        [Test]
        public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
        {
            using (new Db2ISeriesTestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.SchemaExists("TstSchma").ShouldBeTrue();
            }
        }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Db2.IsEnabled)
                Assert.Ignore();
            var factory = new Db2ISeriesDbFactory(serviceProvider: null);
            Connection = factory.CreateConnection(IntegrationTestOptions.Db2.ConnectionString);
            Quoter = new Db2ISeriesQuoter();
            Processor = new Db2ISeriesProcessor(Connection, new Db2ISeriesGenerator(), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), factory);
            Connection.Open();
        }

        [TearDown]
        public void TearDown()
        {
            Processor?.Dispose();
        }

        private static void EnsureReference()
        {
            // This is here to avoid the removal of the referenced assembly
            Debug.WriteLine(typeof(IBM.Data.DB2.DB2Factory));
        }
    }
}

﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.ViewCompilation
{
    public class ApplicationWithParseErrorsTest : IClassFixture<ApplicationWithParseErrorsTest.ApplicationWithParseErrorsFixture>
    {
        public ApplicationWithParseErrorsTest(ApplicationWithParseErrorsFixture fixture)
        {
            Fixture = fixture;
        }

        public ApplicationWithParseErrorsFixture Fixture { get; }

        public static TheoryData SupportedFlavorsTheoryData => RuntimeFlavors.SupportedFlavorsTheoryData;

        [Theory]
        [MemberData(nameof(SupportedFlavorsTheoryData))]
        public void PublishingPrintsParseErrors(RuntimeFlavor flavor)
        {
            var indexPath = Path.Combine(Fixture.ApplicationPath, "Views", "Home", "Index.cshtml");
            var viewImportsPath = Path.Combine(Fixture.ApplicationPath, "Views", "Home", "About.cshtml");
            var expectedErrors = new[]
            {
                indexPath + " (0): The code block is missing a closing \"}\" character.  Make sure you have a matching \"}\" character for all the \"{\" characters within this block, and that none of the \"}\" characters are being interpreted as markup.",
                viewImportsPath + " (1): A space or line break was encountered after the \"@\" character.  Only valid identifiers, keywords, comments, \"(\" and \"{\" are valid at the start of a code block and they must occur immediately following \"@\" with no space in between.",

            };
            using (var deployer = Fixture.CreateDeployment(flavor))
            {
                // Act & Assert
                Assert.Throws<Exception>(() => deployer.Deploy());

                // Assert
                var output = Fixture.TestSink.Writes.Select(w => w.State.ToString().Trim()).ToList();

                foreach (var error in expectedErrors)
                {
                    Assert.Contains(error, output);
                }
            }
        }


        public class ApplicationWithParseErrorsFixture : ApplicationTestFixture
        {
            public ApplicationWithParseErrorsFixture()
                : base("ApplicationWithParseErrors")
            {
            }

            public TestSink TestSink { get; } = new TestSink();

            protected override ILogger CreateLogger(RuntimeFlavor flavor)
            {
                return new TestLoggerFactory(TestSink, enabled: true).CreateLogger($"{ApplicationName}:{flavor}");
            }
        }
    }
}
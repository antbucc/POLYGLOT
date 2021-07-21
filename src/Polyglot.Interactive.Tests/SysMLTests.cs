using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.SysML;
using SysML.Interactive;
using System.Collections.Generic;

using System;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using Polyglot.Core;
using System.Threading;
using Xunit.Abstractions;

namespace Polyglot.Interactive.Tests
{
    public class SysMLTests
    {

        [Fact]
        public async Task can_find_definitions()
        {
            var metric = new DefinitionStructureMetric();

            var command = new SubmitCode(@"
#!sysml
package 'Part Definition Example' {
	import ScalarValues::*;
	
	part def Vehicle {
		attribute mass : Real;
		part eng : Engine;
	}
	
	part def Engine;	
}");

            var expected = new SysMLElement
            (
                "Part Definition Example",
               SysMLElementKind.PACKAGE,
               new List<SysMLElement>
                {
                    new
                    (
                        "Vehicle",
                        SysMLElementKind.PART_DEFINITION,
                        new List<SysMLElement>
                        {
                            new
                            (
                                "mass",
                                SysMLElementKind.ATTRIBUTE_USAGE,
                                new List<SysMLElement>(),
                                "Real"
                            ),
                            new
                            (
                                "eng",
                                SysMLElementKind.PART_USAGE,
                                new List<SysMLElement>(),
                                "Engine"
                            )
                        },
                        null
                    ),
                    new
                    (
                        "Engine",
                        SysMLElementKind.PART_DEFINITION,
                        new List<SysMLElement>(),
                        null
                    )
                },
               null
            );


            var kernel = new CompositeKernel().UseSysML();
            var events = new List<KernelEvent>();

            var subscription = kernel.KernelEvents.Subscribe(events.Add);
            await kernel.SendAsync(command);
            subscription.Dispose();

            var values = (await metric.CalculateAsync(command, kernel, events)) as IEnumerable<SysMLElement>;

            values.Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(expected);
        }

    }
}

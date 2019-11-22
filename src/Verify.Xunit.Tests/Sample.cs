﻿using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class Sample :
    VerifyBase
{
    [Fact]
    public async Task Simple()
    {
        await Verify("Foo");
    }

    public Sample(ITestOutputHelper output) :
        base(output)
    {
    }
}
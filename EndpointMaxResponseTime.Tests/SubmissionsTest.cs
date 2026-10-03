using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using EndpointMaxResponseTime.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace EndpointMaxResponseTime.Tests;

public class SubmissionsTests
{
    private WebApplicationUnderTest _application;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationUnderTest();
    }

    [TearDown]
    public void TearDown()
    {
        _application.Dispose();
    }

    [Test]
    public async Task ImmediateResponseTest()
    {
        var scenario = new SubmissionScenario(
            _application,
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(1)
        );

        var submissionFromCreate = await scenario.CreateSubmissionAsync();
        Assert.That(scenario.LastResponseTimeMilliseconds, Is.LessThan(500));
        AssertThatSubmissionIsValid(submissionFromCreate, true);

        var submissionFromGet = await scenario.GetSubmissionAsync();
        Assert.That(submissionFromGet, Is.EqualTo(submissionFromCreate).UsingPropertiesComparer());
    }

    [Test]
    public async Task SlowResponseAfterPointOfNoReturnTest()
    {
        var scenario = new SubmissionScenario(
            _application,
            TimeSpan.FromSeconds(0.6),
            TimeSpan.FromSeconds(0.6)
        );

        var submissionFromCreate = await scenario.CreateSubmissionAsync();
        Assert.That(scenario.LastResponseTimeMilliseconds, Is.InRange(1000, 1100));
        AssertThatSubmissionIsValid(submissionFromCreate, false);

        await Task.Delay(TimeSpan.FromSeconds(1));

        var submissionFromGet = await scenario.GetSubmissionAsync();
        AssertThatSubmissionIsValid(submissionFromGet, true);
        Assert.That(submissionFromGet.Id, Is.EqualTo(submissionFromCreate.Id));
    }

    [Test]
    public async Task SlowResponseBeforePointOfNoReturnTest()
    {
        var scenario = new SubmissionScenario(
            _application,
            TimeSpan.FromSeconds(1.1),
            TimeSpan.FromSeconds(1.1)
        );

        var allSubmissionsBefore = await scenario.GetAllSubmissionsAsync();
        Assert.That(allSubmissionsBefore, Is.Not.Null);

        await scenario.CreateSubmissionAsync();
        Assert.That(scenario.LastResponseStatusCode, Is.EqualTo(HttpStatusCode.GatewayTimeout));
        Assert.That(scenario.LastResponseTimeMilliseconds, Is.InRange(1000, 1100));

        await Task.Delay(TimeSpan.FromSeconds(2));

        var allSubmissionsAfter = await scenario.GetAllSubmissionsAsync();

        Assert.That(allSubmissionsAfter, Is.Not.Null);
        Assert.That(allSubmissionsAfter.Count, Is.EqualTo(allSubmissionsBefore.Count));
    }

    private static void AssertThatSubmissionIsValid(
        [NotNull] Submission? actual,
        bool isPhase2Completed
    )
    {
        Assert.That(actual, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(actual.Phase1CompletedAt, Is.Not.Null);
            Assert.That(actual.Phase2CompletedAt, isPhase2Completed ? Is.Not.Null : Is.Null);
        }
    }
}

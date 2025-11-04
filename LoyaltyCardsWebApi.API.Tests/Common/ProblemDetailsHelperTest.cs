using System.Diagnostics;
using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoyaltyCardsWebApi.API.Tests.Common;

[TestFixture]
public class ProblemDetailsHelperTest
{
    private static (HttpContext context, string? activityId) CreateHttpContext
        (
            string method,
            string path,
            string traceIdentifier
        )
    {
        HttpContext context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.TraceIdentifier = traceIdentifier;

        var activity = new Activity("test-activity");
        activity.Start();
        context.Features.Set<IHttpActivityFeature>(new TestActivityFeature(activity));
        return (context, activity.TraceId.ToString() ?? Activity.Current?.TraceId.ToString());
    }

    private class TestActivityFeature : IHttpActivityFeature
    {
        public Activity Activity { get; set; }
        public TestActivityFeature(Activity activity)
        {
            Activity = activity;
        }

    }
    public static IEnumerable<TestCaseData> CreateTestCases()
    {
        yield return new TestCaseData(
            "GET",
            "/auth/logout",
            "asasa321323",
            "title1",
            401,
            "details1"
            );
        yield return new TestCaseData(
            "POST",
            "/Cards",
            "as54321323",
            "title2",
            500,
            "details2"
            );
    }

    [Test, TestCaseSource(nameof(CreateTestCases))]
    public void CreateProblemDetails_CorrectInput_CorrectOutput(
        string method,
        string path,
        string traceIdentifier,
        string title,
        int statusCode,
        string details
        )
    {
        HttpContext context;
        string? activityId;
        (context, activityId) = CreateHttpContext(method, path, traceIdentifier);
        ProblemDetails problemDetails;

        problemDetails = ProblemDetailsHelper.CreateProblemDetails(context, title, statusCode, details);

        Assert.That(problemDetails.Instance, Is.EqualTo($"{method} {path}"));
        Assert.That(problemDetails.Status, Is.EqualTo(statusCode));
        Assert.That(problemDetails.Detail, Is.EqualTo(details));
        Assert.That(problemDetails.Title, Is.EqualTo(title));
        Assert.That(problemDetails.Extensions.ContainsKey("traceId"), Is.True);
        Assert.That(problemDetails.Extensions["traceId"], Is.EqualTo(activityId));
        Assert.That(problemDetails.Extensions.ContainsKey("requestId"), Is.True);
        Assert.That(problemDetails.Extensions["requestId"], Is.EqualTo(traceIdentifier));

    }

    public static IEnumerable<TestCaseData> CreateTestCases_MissingInput()
    {
        yield return new TestCaseData(
            "POST",
            "/Cards",
            "",
            "",
            500,
            ""
            );
    }
    [Test, TestCaseSource(nameof(CreateTestCases_MissingInput))]
    public void CreateProblemDetails_MissingInput_CorrectOutput(
        string method,
        string path,
        string traceIdentifier,
        string title,
        int statusCode,
        string details
        )
    {
        HttpContext context;
        string? activityTraceId;
        (context, activityTraceId) = CreateHttpContext(method, path, traceIdentifier);
        ProblemDetails problemDetails;

        problemDetails = ProblemDetailsHelper.CreateProblemDetails(context, title, statusCode, details);

        Assert.That(problemDetails.Instance, Is.EqualTo($"{method} {path}"));
        Assert.That(problemDetails.Status, Is.EqualTo(statusCode));
        Assert.That(problemDetails.Detail, Is.EqualTo(details));
        Assert.That(problemDetails.Title, Is.EqualTo(title));
        Assert.That(problemDetails.Extensions.ContainsKey("traceId"), Is.True);
        Assert.That(problemDetails.Extensions["traceId"], Is.EqualTo(activityTraceId));
        Assert.That(problemDetails.Extensions.ContainsKey("requestId"), Is.False);
    }
}
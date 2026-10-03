using EndpointMaxResponseTime.Models;
using NUnit.Framework.Constraints;

namespace EndpointMaxResponseTime.Tests;

public class SubmissionIsValidConstraint : Constraint
{
    private bool? _expectedIsPhase2Completed;

    public override string Description =>
        _expectedIsPhase2Completed switch
        {
            true => "submission is completed",
            false => "submission is not completed",
            null => "submission is valid",
        };

    public SubmissionIsValidConstraint WithPhase2Completed
    {
        get
        {
            _expectedIsPhase2Completed = true;
            return this;
        }
    }

    public SubmissionIsValidConstraint WithoutPhase2Completed
    {
        get
        {
            _expectedIsPhase2Completed = false;
            return this;
        }
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is not Submission submission)
        {
            throw new ArgumentException("Actual value must be of type Submission.", nameof(actual));
        }

        var isIdValid = submission.Id != Guid.Empty;
        var isPhase1Completed = submission.Phase1CompletedAt != null;
        var isPhase2Completed = submission.Phase2CompletedAt != null;
        var isPhase2CompletedMatchesExpectation =
            _expectedIsPhase2Completed == null || isPhase2Completed == _expectedIsPhase2Completed;
        var isValid = isIdValid && isPhase1Completed && isPhase2CompletedMatchesExpectation;
        return new ConstraintResult(this, actual, isValid);
    }
}

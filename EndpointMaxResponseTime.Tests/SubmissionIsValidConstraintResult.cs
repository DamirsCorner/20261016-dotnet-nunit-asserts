using NUnit.Framework.Constraints;

namespace EndpointMaxResponseTime.Tests;

public class SubmissionIsValidConstraintResult(
    IConstraint constraint,
    object actualValue,
    bool isSuccess,
    bool isIdValid,
    bool isPhase1Completed,
    bool isPhase2Completed,
    bool? expectedIsPhase2Completed
) : ConstraintResult(constraint, actualValue, isSuccess)
{
    public override void WriteMessageTo(MessageWriter writer)
    {
        writer.WriteLine("Submission validation failed:");
        if (!isIdValid)
        {
            writer.WriteLine("  Id is invalid (Guid.Empty).");
        }
        if (!isPhase1Completed)
        {
            writer.WriteLine("  Phase 1 is not completed.");
        }
        if (expectedIsPhase2Completed.HasValue)
        {
            if (isPhase2Completed != expectedIsPhase2Completed.Value)
            {
                writer.WriteLine(
                    $"  Phase 2 completion status does not match expectation. Expected: {expectedIsPhase2Completed.Value}, Actual: {isPhase2Completed}"
                );
            }
        }
    }
}

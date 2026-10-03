using NUnit.Framework.Constraints;

namespace EndpointMaxResponseTime.Tests;

public static class SubmissionIsValidConstraintExtensions
{
    extension(Is)
    {
        public static SubmissionIsValidConstraint ValidSubmission => new();
    }

    extension(ConstraintExpression expression)
    {
        public SubmissionIsValidConstraint ValidSubmission =>
            expression.Append(new SubmissionIsValidConstraint());
    }
}

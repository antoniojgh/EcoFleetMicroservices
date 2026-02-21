using FluentValidation.Results;

namespace EcoFleet.BuildingBlocks.Application.Exceptions;

public class ValidationErrorException : Exception
{
    public ValidationErrorException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
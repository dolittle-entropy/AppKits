using Common.Commands;
using Common.Exceptions;
using Common.Models;
using Common.Rejections;

namespace Common.Extensions;

public static class ValidationExtensions
{
    public static ValidationFailed EntityIsNullException(string entityName)
    {
        return new ValidationFailed($"Received empty/null {entityName}");
    }

    public static void ThrowRepositoryNotFound(string repositoryName)
    {
        throw new RepositoryNotFound($"Repository not found: {repositoryName}");
    }

    public static ValidationFailed IncorrectDateOrderException<TEntity>(this TEntity entity, string beforeFieldName, string afterFieldName)
    {
        var entityName = entity!.GetType().Name;
        return new ValidationFailed($"{entityName}.{beforeFieldName} cannot be after {entityName}.{afterFieldName}");
    }

    public static ValidationFailed FutureDateException<TEntity>(this TEntity entity, string fieldName)
    {
        var entityName = entity!.GetType().Name;
        return new ValidationFailed($"{entityName}.{fieldName} was set to the future. This is not allowed here");
    }

    public static ValidationFailed UnknownTypeException<TEntity>(this TEntity entity, string fieldName)
    {
        var entityName = entity!.GetType().Name;
        return new ValidationFailed($"{entityName}.{fieldName} cannot be UNKNOWN");
    }

    public static ValidationFailed ValueIsNullOrEmptyException<TEntity>(this TEntity entity, string fieldName)
    {
        var entityName = entity!.GetType().Name;
        return new ValidationFailed($"{entityName}.{fieldName} cannot be null or empty");
    }

    public static ValidationFailed ValueExceedsRangeException<TEntity>(this TEntity entity,
        string fieldName, string validationValue)
    {
        var entityName = entity!.GetType().Name;
        return new ValidationFailed(
            $"{entityName}.{fieldName}: Value exceeds maximum length of {validationValue} characters");
    }

    public static ValidationFailed ValueIsNotNumericException<TEntity>(this TEntity entity,
        string fieldName, string actualValue)
    {
        var entityName = entity!.GetType().Name;
        return new ValidationFailed($"{entityName}.{fieldName}: Expected a number, but got '{actualValue}'");
    }

    public static ValidationFailed ValueHasIncorrectFormatObjectException<TEntity>(this TEntity entity,
        string fieldName,
        string actualValue)
    {
        var entityName = entity!.GetType().Name;
        throw new ValidationFailed(
            $"{entityName}.{fieldName}: {actualValue} does not have correct format");
    }

    public static void Reject<TEntity>(this ICommand<TEntity> command,
        string rejectedBy, string fieldName, string fieldValue, string? customMessage = null) where TEntity : IEntity
    {
        var contextName = command.GetType().Assembly.GetName().Name;
        var commandName = command.GetType().Name;

        var defaultMessage = $"{commandName}.{fieldName} '{fieldValue}' is not accepted in the {contextName} Context.";
        var rejectionReason = string.IsNullOrEmpty(customMessage) ? defaultMessage : $"{defaultMessage} {customMessage}";

        var rejectable = command as Rejection;
        rejectable!.Rejected = true;
        rejectable.FromTenantId = command.TenantId.ToString();
        rejectable.RejectedBy = rejectedBy;
        rejectable.RejectionReason = rejectionReason;
        rejectable.FailingObject = command.Payload;
        rejectable.IsSynchronized = true;
    }
}
using System;
using Domain.Helpers.Exceptions;

namespace Domain.Helpers;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(
        string? value,
        string parameterName,
        int minLength = 1,
        int maxLength = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CustomException($"{parameterName} must not be empty or whitespace.");
        }

        string trimmed = value.Trim();

        if (trimmed.Length < minLength)
        {
            throw new CustomException($"{parameterName} must be at least {minLength} characters long.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new CustomException($"{parameterName} must not exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static string? AgainstTooLong(
        string? value,
        string parameterName,
        int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new CustomException($"{parameterName} must not exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static double AgainstOutOfRange(
        double value,
        string parameterName,
        double minimum,
        double maximum)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new CustomException($"{parameterName} must be a valid number.");
        }

        if (value < minimum || value > maximum)
        {
            throw new CustomException($"{parameterName} must be between {minimum} and {maximum}.");
        }

        return value;
    }

    public static int AgainstOutOfRange(
        int value,
        string parameterName,
        int minimum,
        int maximum)
    {
        if (value < minimum || value > maximum)
        {
            throw new CustomException($"{parameterName} must be between {minimum} and {maximum}.");
        }

        return value;
    }

    public static Guid AgainstEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new CustomException($"{parameterName} must not be empty.");
        }

        return value;
    }
}


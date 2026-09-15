namespace Shop.Domain.Exceptions;

public class InvalidRefreshTokenException() : Exception("The refresh token is invalid or expired");

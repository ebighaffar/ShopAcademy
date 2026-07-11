namespace Shop.Domain.Exceptions;

public class DuplicateUserNameException() : Exception("A user with this username already exists")
{
}

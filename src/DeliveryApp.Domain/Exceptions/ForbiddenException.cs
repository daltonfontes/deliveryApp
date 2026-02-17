namespace DeliveryApp.Domain.Exceptions;

public class ForbiddenException(string message = "Access denied.") : DomainException(message);

namespace DeliveryApp.Domain.Exceptions;

public class NotFoundException(string entity, object key)
    : DomainException($"{entity} with key '{key}' was not found.");

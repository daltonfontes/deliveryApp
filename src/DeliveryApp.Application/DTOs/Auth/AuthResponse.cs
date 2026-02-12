namespace DeliveryApp.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string Email,
    string FullName,
    string Role,
    DateTime ExpiresAt);

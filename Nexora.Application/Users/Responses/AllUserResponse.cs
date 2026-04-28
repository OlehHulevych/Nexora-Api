using Nexora.Domain.DTOs;

namespace Nexora.Application.Users.Commands.GettingUsers;

public record AllUserResponse(
    List<UserDto> users,
    int currentPage,
    int totalPages);
using System;

namespace DTO.Categories
{
    public record CategoryResponseDto
    (
        Guid Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool IsDeleted,
        DateTime? DeletedAt,
        byte[] RowVersion
    );
}

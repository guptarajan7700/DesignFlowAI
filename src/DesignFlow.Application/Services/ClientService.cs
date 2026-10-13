using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public sealed class ClientService : IClientService
{
    private readonly IClientRepository _clients;
    private readonly IUserRepository _users;
    private readonly IAuditRepository _audit;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IClientRepository clients, IUserRepository users, IAuditRepository audit, IUnitOfWork unitOfWork)
    {
        _clients = clients;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken cancellationToken) =>
        _clients.ListAsync(cancellationToken);

    public async Task<ClientDto> CreateAsync(CreateClientRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _clients.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("A client with that email already exists.");
        }

        if (request.PortalUserId is Guid portalUserId && await _users.FindByIdAsync(portalUserId, cancellationToken) is null)
        {
            throw new NotFoundException("Portal user was not found.");
        }

        var status = string.IsNullOrWhiteSpace(request.Status)
            ? ClientStatus.Active
            : EnumParser.Parse<ClientStatus>(request.Status, "Status");

        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            CompanyName = request.CompanyName.Trim(),
            ContactName = request.ContactName.Trim(),
            Email = email,
            Phone = request.Phone?.Trim(),
            Website = request.Website?.Trim(),
            AddressLine1 = request.AddressLine1?.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            Country = request.Country?.Trim(),
            Status = status,
            Notes = request.Notes?.Trim(),
            PortalUserId = request.PortalUserId,
            CreatedBy = actorId
        };

        await _clients.AddAsync(client, cancellationToken);
        await _audit.AddAsync(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = actorId,
            EntityType = "Client",
            EntityId = client.ClientId,
            Action = "Created",
            NewValues = $"{{\"company\":\"{client.CompanyName}\"}}",
            CreatedBy = actorId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(
            client.ClientId,
            client.CompanyName,
            client.ContactName,
            client.Email,
            client.Phone,
            client.City,
            client.Country,
            client.Status.ToString(),
            client.PortalUserId);
    }
}

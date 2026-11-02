using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;

        public required string NewName { get; set; } = string.Empty;
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            var name = Person.NormalizeName(request.Name);
            var newName = Person.NormalizeName(request.NewName);

            var person = await _context.People.FirstOrDefaultAsync(
                p => p.Name.ToLower() == name,
                cancellationToken);

            if (person is null)
            {
                return new UpdatePersonResult()
                {
                    Success = false,
                    Message = "Person not found.",
                    ResponseCode = (int)HttpStatusCode.NotFound
                };
            }

            if (newName.Length == 0)
            {
                return new UpdatePersonResult()
                {
                    Success = false,
                    Message = "Name is required.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                };
            }

            var personExists = await _context.People.AnyAsync(
                p => p.Id != person.Id && p.Name.ToLower() == newName,
                cancellationToken);

            if (personExists)
            {
                return new UpdatePersonResult()
                {
                    Success = false,
                    Message = "Person already exists.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                };
            }

            person.Name = newName;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdatePersonResult()
            {
                Id = person.Id,
                Message = "Person updated."
            };

        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}

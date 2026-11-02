using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;

        public CreatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            var name = Person.NormalizeName(request.Name);

            var existingPerson = await _context.People.FirstOrDefaultAsync(
                p => p.Name.ToLower() == name,
                cancellationToken);

            if (existingPerson is not null)
            {
                return new CreatePersonResult()
                {
                    Success = false,
                    Message = "Person already exists.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                };
            }

            var newPerson = new Person()
            {
                Name = name
            };

            await _context.People.AddAsync(newPerson, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePersonResult()
            {
                Id = newPerson.Id,
                Message = "Person created."
            };
          
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}

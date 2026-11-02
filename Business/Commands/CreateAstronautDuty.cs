using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var name = Person.NormalizeName(request.Name);

            var person = await _context.People.FirstOrDefaultAsync(p => p.Name.ToLower() == name, cancellationToken);

            if (person is null)
            {
                return new CreateAstronautDutyResult()
                {
                    Success = false,
                    Message = "Person not found.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                };
            }

            var duplicateDuty = await _context.AstronautDuties
                .FirstOrDefaultAsync(d =>
                d.PersonId == person.Id &&
                d.DutyTitle == request.DutyTitle &&
                d.DutyStartDate == request.DutyStartDate.Date,
                cancellationToken);

            if (duplicateDuty is not null)
            {
                return new CreateAstronautDutyResult()
                {
                    Success = false,
                    Message = "Duplicate duty.",
                    ResponseCode = (int)HttpStatusCode.BadRequest
                };
            }

            var astronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(d => d.PersonId == person.Id, cancellationToken);

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                if (request.DutyTitle == "RETIRED")
                {
                    //Updated to support Rule #7
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            //query = $"SELECT * FROM [AstronautDuty] WHERE {person.Id} = PersonId Order By DutyStartDate Desc";

            //var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query);

            var astronautDuty = await _context.AstronautDuties
                .Where(d => d.PersonId == person.Id)
                .OrderByDescending(d => d.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (astronautDuty != null)
            {
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}

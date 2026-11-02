using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Logging
{
    public class ProcessLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly StargateContext _context;

        public ProcessLoggingBehavior(StargateContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await next();

                var success = response is not BaseResponse baseResponse || baseResponse.Success;
                var message = response is BaseResponse loggedResponse ? loggedResponse.Message : "Successful";

                await WriteAsync(typeof(TRequest).Name, success, message);

                return response;
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                await WriteAsync(typeof(TRequest).Name, false, ex.Message);
                throw;
            }
        }

        private async Task WriteAsync(String processName, bool success, string message)
        {
            _context.ProcessLogs.Add(new ProcessLog
            {
                LoggedAt = DateTime.Now,
                Success = success,
                ProcessName = processName,
                Message = message
            });

            await _context.SaveChangesAsync();
        }
    }
}

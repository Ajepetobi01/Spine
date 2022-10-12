using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Helpers;
using Spine.Data;

namespace Spine.Core.Accounts.Jobs
{
    public class DisableDueAccountsCommand : IRequest
    {
    }

    public class DisableDueAccountsHandler : IRequestHandler<DisableDueAccountsCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<DisableDueAccountsHandler> _logger;

        public DisableDueAccountsHandler(SpineContext context, ILogger<DisableDueAccountsHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(DisableDueAccountsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var unverifiedAccounts = await _dbContext.Companies.Where(x => !x.IsVerified && !x.IsDeleted).ToListAsync();
                var dueAccounts = unverifiedAccounts.Where(d => (Constants.GetCurrentDateTime().Date - d.CreatedOn.Date).TotalDays > Constants.DaysToDisableAccount)
                                                                            .Select(x => x.Id).ToList();

                foreach (var item in unverifiedAccounts)
                {
                    if (dueAccounts.Contains(item.Id))
                        item.IsDeleted = true;
                    //not setting deleted by so we can use that to know if it was deleted by a job or deleted from the application
                }

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while disabling due accounts {ex.Message}");
            }

            return Unit.Value;
        }
    }
}
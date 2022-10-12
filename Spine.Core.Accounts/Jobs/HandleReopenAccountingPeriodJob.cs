using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities.Transactions;

namespace Spine.Core.Accounts.Jobs
{
    public class HandleReopenAccountingPeriodCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookClosingId { get; set; }
        
    }

    public class HandleReopenAccountingPeriodHandler : IRequestHandler<HandleReopenAccountingPeriodCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<HandleReopenAccountingPeriodHandler> _logger;

        public HandleReopenAccountingPeriodHandler(SpineContext context, ILogger<HandleReopenAccountingPeriodHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleReopenAccountingPeriodCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ledgerEntries = await _dbContext.GeneralLedgers.Where(x =>
                        x.CompanyId == request.CompanyId && x.IsClosingEntry && x.BookClosingId == request.BookClosingId).ToListAsync();

                var openingBalance = await _dbContext.OpeningBalances
                    .Where(x => x.CompanyId == request.CompanyId && x.BookClosingId == request.BookClosingId)
                    .ToListAsync();
                
                _dbContext.GeneralLedgers.RemoveRange(ledgerEntries);
                _dbContext.OpeningBalances.RemoveRange(openingBalance);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while reopening accounting period {ex.Message}");
            }

            return Unit.Value;
        }
    }
}
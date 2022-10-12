using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Services
{
    public interface ISerialNumberHelper
    {
        Task<int> GetLastUsedDailyTransactionNo(SpineContext dbContext, Guid companyId, DateTime date, int count);
        
        Task<int> GetLastUsedGoodsReceivedNo(SpineContext dbContext, Guid companyId, int count);
        Task<int> GetLastUsedPurchaseOrderNo(SpineContext dbContext, Guid companyId, int count);
        Task<int> GetLastUsedJournalNo(SpineContext dbContext, Guid companyId, int count);
    }

    public class SerialNumberHelper : ISerialNumberHelper
    {
        private static async Task<CompanySerial> GetCompanySerialRecord(SpineContext dbContext, Guid companyId)
        {
            return await dbContext.CompanySerials.FirstAsync(x => x.CompanyId == companyId);
        }
        
        public async Task<int> GetLastUsedPurchaseOrderNo(SpineContext dbContext, Guid companyId, int count)
        {
            var record = await GetCompanySerialRecord(dbContext, companyId);
            record.LastUsedPO += count;
            
            await dbContext.SaveChangesAsync();
            return record.LastUsedPO - count;
        }
        
        public async Task<int> GetLastUsedJournalNo(SpineContext dbContext, Guid companyId, int count)
        {
            var record = await GetCompanySerialRecord(dbContext, companyId);
            record.LastUsedJournal += count;
            
            await dbContext.SaveChangesAsync();
            return record.LastUsedJournal - count;
        }

        public async Task<int> GetLastUsedGoodsReceivedNo(SpineContext dbContext, Guid companyId, int count)
        {
            var record = await GetCompanySerialRecord(dbContext, companyId);
            record.LastUsedGR += count;
            
            await dbContext.SaveChangesAsync();
            return record.LastUsedGR - count;
        }



        public async Task<int> GetLastUsedDailyTransactionNo(SpineContext dbContext, Guid companyId, DateTime date, int count)
        {
            var lastRecord = await GetCompanySerialRecord(dbContext, companyId);
           
            if (lastRecord.CurrentDate == date)
                lastRecord.LastUsedTransactionNo += count; // just increase count for current date
            else
            {
                lastRecord.CurrentDate = date.Date;
                lastRecord.LastUsedTransactionNo = count; // reset if it's a new day
            }

            await dbContext.SaveChangesAsync();
            return lastRecord.LastUsedTransactionNo - count;
        }
    }
}

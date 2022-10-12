using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries.Product
{
    public static class GetProductById
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            public Guid Id { get; set; }

        }

        public class Response
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public string SerialNo { get; set; }
            public string SKU { get; set; }
            public int ReorderLevel { get; set; }
            public bool NotifyOutOfStock { get; set; }
            public int Quantity { get; set; }
            public DateTime LastRestockDate { get; set; }
            public DateTime InventoryDate { get; set; }
            public decimal PurchasePrice { get; set; }
            public string InventoryStatus { get; set; }
            public decimal SalesPrice { get; set; }
            public int? MeasurementUnitId { get; set; }
            public Guid? CategoryId { get; set; }
            public DateTime CreatedOn { get; set; }

            public List<NoteModel> Notes { get; set; }
            
            public List<Allocation> Allocations { get; set; }
        }

        public class Allocation
        {
            public Guid LocationId { get; set; }
            public int Quantity { get; set; }
        }
        
        public class NoteModel
        {
            public Guid Id { get; set; }
            public string Note { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var item = await (from inv in _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId &&
                        x.InventoryType == InventoryType.Product
                        && x.Id == request.Id && !x.IsDeleted)
                    join cat in _dbContext.ProductCategories on inv.CategoryId equals cat.Id
                    select new Response
                    {
                        Id = inv.Id,
                        Name = inv.Name,
                        Category = cat.Name,
                        SerialNo = inv.SerialNo,
                        SKU = inv.SKU,
                        InventoryStatus = inv.Status.GetDescription(),
                        ReorderLevel = inv.ReorderLevel,
                        NotifyOutOfStock = inv.NotifyOutOfStock,
                        LastRestockDate = inv.LastRestockDate,
                        InventoryDate = inv.InventoryDate,
                        CreatedOn = inv.CreatedOn,
                        SalesPrice = inv.UnitSalesPrice,
                        PurchasePrice = inv.UnitCostPrice,
                        CategoryId = inv.CategoryId,
                        MeasurementUnitId = inv.MeasurementUnitId,
                        Quantity = inv.QuantityInStock,
                        Description = inv.Description,
                    }).SingleOrDefaultAsync();

                if (item != null)
                {
                    item.Notes = await _dbContext.InventoryNotes.Where(x => x.CompanyId == request.CompanyId
                                                                            && !x.IsDeleted && x.InventoryId == item.Id)
                        .Select(x => new NoteModel {Note = x.Note, Id = x.Id})
                        .ToListAsync();

                    item.Allocations = await _dbContext.ProductLocations.Where(x => x.CompanyId == request.CompanyId
                            && x.InventoryId == item.Id)
                        .Select(x => new Allocation
                            {LocationId = x.LocationId, Quantity = x.QuantityInStock}).ToListAsync();
                }

                return item;
            }
        }
    }
}

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Inventories.Queries
{
    public static class GetInventoryQRCode
    {
        public class Query : IRequest<Response>
        {
            public Guid InventoryId { get; set; }
        }

        public class Response
        {
            public MemoryStream OutputStream { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query message, CancellationToken token)
            {
                var item = await _dbContext.Inventories.SingleOrDefaultAsync(x => x.Id == message.InventoryId && !x.IsDeleted);
                if (item != null)
                {
                    string qrString = "";
                    if (item.InventoryType == InventoryType.Product)
                    {
                        var model = new
                        {
                            item.Name,
                            item.Description,
                            item.QuantityInStock,
                            item.UnitSalesPrice,
                            item.SKU,
                            item.SerialNo,
                            item.LastRestockDate,
                            Status = item.Status.GetDescription(),
                        };
                        qrString = JsonSerializer.Serialize(model);
                    }
                    else if (item.InventoryType == InventoryType.Service)
                    {
                        var model = new
                        {
                            item.Name,
                            item.Description,
                            item.UnitSalesPrice,
                            Status = item.Status.GetDescription(),
                        };
                        qrString = JsonSerializer.Serialize(model);
                    }

                    var generator = new QRCodeGenerator();
                    var qrCodeData = generator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        var qrCodeImage = qrCode.GetGraphic(20);
                        var outputStream = new MemoryStream();
                        qrCodeImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        outputStream.Seek(0, SeekOrigin.Begin);
                        return new Response
                        {
                            OutputStream = outputStream
                        };
                    }
                }

                return null;
            }
        }

    }
}

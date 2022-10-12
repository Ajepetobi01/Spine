using Microsoft.EntityFrameworkCore;
using Spine.Data.Documents.Models;

namespace Spine.Data.Documents
{
    public class UploadsDbContext : DbContext
    {
        public UploadsDbContext(DbContextOptions<UploadsDbContext> options) : base(options)
        {
        }

        //dbsets
        public DbSet<Document> Documents { get; set; }


        //invoice customization stuff
        public DbSet<InvoiceBanner> InvoiceBanners { get; set; }
        public DbSet<CompanyInvoiceLogo> CompanyInvoiceLogos { get; set; }
        public DbSet<CompanyInvoiceSignature> CompanyInvoiceSignatures { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}

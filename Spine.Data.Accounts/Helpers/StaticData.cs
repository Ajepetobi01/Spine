using System;
using System.Collections.Generic;
using Spine.Common.Helper;
using Spine.Data.Accounts.Entities;

namespace Spine.Data.Accounts.Helpers
{
    public static class StaticData
    {
        public static List<BusinessType> BusinessTypes()
        {
            return new List<BusinessType>
            {
                new BusinessType {Id = 1, Type = "Sole Proprietorship"},
                new BusinessType {Id = 2, Type = "Partnership"},
                new BusinessType {Id = 3, Type = "Limited Liability Company (LLC)"},
                new BusinessType {Id = 4, Type = "Corporation"},
                new BusinessType {Id = 5, Type = "Nonprofit Organization"},
            };
        }

        public static List<OperatingSector> OperatingSectors()
        {
            return new List<OperatingSector>
            {
                new OperatingSector {Id = 1, Sector = "Aerospace"},
                new OperatingSector {Id = 2, Sector = "Entertainment"},
                new OperatingSector {Id = 3, Sector = "Agriculture"},
                new OperatingSector {Id = 4, Sector = "Financial Services"},
                new OperatingSector {Id = 5, Sector = "Courier"},
                new OperatingSector {Id = 6, Sector = "Government/Public Sector Services"},
                new OperatingSector {Id = 7, Sector = "Purchase Stock"},
                new OperatingSector {Id = 8, Sector = "Healthcare"},
                new OperatingSector {Id = 9, Sector = "Data Analytics/Data Science"},
                new OperatingSector {Id = 10, Sector = "Insurance"},
                new OperatingSector {Id = 11, Sector = "Education"},
                new OperatingSector {Id = 12, Sector = "IT"},
                new OperatingSector {Id = 13, Sector = "Manufacturing"},
                new OperatingSector {Id = 14, Sector = "Retail"},
                new OperatingSector {Id = 15, Sector = "Media"},
                new OperatingSector {Id = 16, Sector = "Wholesale"},
                new OperatingSector {Id = 17, Sector = "Print/Publishing"},
                new OperatingSector {Id = 18, Sector = "Energy"},
                new OperatingSector {Id = 19, Sector = "Fashion and accessories"},
                new OperatingSector {Id = 20, Sector = "E-commerce"},
                new OperatingSector {Id = 21, Sector = "Recruitment Services"},
                new OperatingSector {Id = 22, Sector = "Import/Export"},
                new OperatingSector {Id = 23, Sector = "Transportation Services"},
                new OperatingSector {Id = 24, Sector = "Logistics Stock"},
                new OperatingSector {Id = 25, Sector = "Construction"}
            };
        }
    }
}

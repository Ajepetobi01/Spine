using System;
using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Models;
using Spine.Core.Transactions.Commands;
using Spine.Core.Transactions.Queries;
using Spine.Data.Entities.Transactions;
using Spine.Services.Mono;

namespace Spine.Core.Transactions.MappingProfiles
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<List<GetBankTransactions.Model>, GetBankTransactions.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetTransactions.Model>, GetTransactions.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            
            CreateMap<CreateBankAccount.Command, BankAccount>(MemberList.Destination)
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
              .ForMember(dest => dest.AccountType, opt => opt.Ignore())
              .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId));

            CreateMap<ImportBankTransaction.ImportTransactionModel, BankTransaction>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.Pending))
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.UserReferenceNo, opt => opt.MapFrom(src => src.ReferenceNumber))
                .ForMember(dest => dest.ReferenceNo, opt => opt.Ignore())
                .ForMember(dest => dest.ChequeNo, opt => opt.MapFrom(src => src.ChequeNumber))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.AmountReceived))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.AmountSpent))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AmountReceived + src.AmountSpent));

            CreateMap<ImportBankTransaction.ImportTransactionModel, Transaction>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => PaymentMode.Account))
                .ForMember(dest => dest.TransactionGroupId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.UserReferenceNo, opt => opt.MapFrom(src => src.ReferenceNumber))
                .ForMember(dest => dest.ReferenceNo, opt => opt.Ignore())
                .ForMember(dest => dest.ChequeNo, opt => opt.MapFrom(src => src.ChequeNumber))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.AmountReceived))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.AmountSpent))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AmountReceived + src.AmountSpent));

            CreateMap<ImportBankTransactionFromMono.ImportTransactionModel, BankTransaction>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.Pending))
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.AmountReceived))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.AmountSpent))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AmountReceived + src.AmountSpent));

            CreateMap<ImportBankTransactionFromMono.ImportTransactionModel, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => PaymentMode.Account))
                .ForMember(dest => dest.TransactionGroupId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.AmountReceived))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.AmountSpent))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AmountReceived + src.AmountSpent));

            
            CreateMap<GetAccountTransactions.Model, PreviewTransactionImport>()
          .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.Date))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Narration))
              .ForMember(dest => dest.AmountReceived, opt => opt.MapFrom(src => src.Type == "credit" ? src.Amount : 0))
              .ForMember(dest => dest.AmountSpent, opt => opt.MapFrom(src => src.Type == "debit" ? src.Amount : 0))
              .ForMember(dest => dest.ReferenceNumber, opt => opt.MapFrom(src => src.Id));
        }
    }
}

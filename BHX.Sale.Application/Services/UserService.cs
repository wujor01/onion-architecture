using BHX.Sale.Application.Models.Requests;
using BHX.Sale.Application.Models.Responses;
using BHX.Sale.Domain.Entities;
using BHX.Sale.Domain.Enums;
using BHX.Sale.Domain.Exceptions;
using BHX.Sale.Application.Models.DTOs;
using BHX.Sale.Application.Interfaces;
using BHX.Sale.Application.Core.Services;
using BHX.Sale.Domain.Core.Repositories;

namespace BHX.Sale.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerService _loggerService;

        public UserService(IUnitOfWork unitOfWork, ILoggerService loggerService)
        {
            _unitOfWork = unitOfWork;
            _loggerService = loggerService;
        }
    }
}
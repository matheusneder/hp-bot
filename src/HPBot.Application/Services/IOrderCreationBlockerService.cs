using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application.Services
{
    public interface IOrderCreationBlockerService
    {
        Task<bool> ShouldCreateANewOrderAsync();
    }
}

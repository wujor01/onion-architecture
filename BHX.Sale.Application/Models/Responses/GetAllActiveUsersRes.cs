using BHX.Sale.Application.Models.DTOs;

namespace BHX.Sale.Application.Models.Responses
{
    public class GetAllActiveUsersRes
    {
        public IList<UserDTO> Data { get; set; }
    }
}

using GenCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Repositories.Interface
{
    public interface IUserTokensRepository
    {
        string InsertOrUpdate(long userId, string token);

        UserToken Get(long userId);

        User GetUser(string token);
    }
}

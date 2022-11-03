using GenCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Repositories.Interface
{
    public interface IUserRepository
    {
        long Insert(User user);

        int UpdateLastActive(long userId);
    }
}

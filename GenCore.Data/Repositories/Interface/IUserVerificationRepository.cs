using GenCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Repositories.Interface
{
    public interface IUserVerificationRepository
    {
        int Insert(UserVerification userVerification);

        int UpdateEmailVerified(long userId);

        int UpdatePhoneVerified(long userId);

        UserVerification Get(string username);
    }
}

//using ApiBackend.Models;
//using ApiBackend.Models.Context;

//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;


//namespace ApiBackend.Services
//{
//    public class AuthService(UserContext context,IConfiguration configuration): IAuthService
//    {
//        public async Task<string?> LoginAsync(UserDTO request)
//        {
//            var user = await context.UserEntity.FirstOrDefaultAsync(u => u.Username == request.Username);
//            if (user is null)
//            {
//                return null;
//            }

//            if (new PasswordHasher<User>().VerifyHashedPassword(user,user.PasswordHash,request.Password) 
//                    == PasswordVerificationResult.Failed)
//            {
//                return null;
//            }

//            return CreateToken(user);
//        }

//        public async Task<User?> RegisterAsync(UserDTO request)
//        {

//            if (await context.UserEntity.AnyAsync(u => u.Username == request.Username))
//            {
//                return null;

//            }

//            var user = new User();
//            var hashedPassword = new PasswordHasher<User>()
//                .HashPassword(user, request.Password);

//            user.Username = request.Username;
//            user.PasswordHash = hashedPassword;

//            context.UserEntity.Add(user);

//            await context.SaveChangesAsync();

//            return user;
//        }

//        private string CreateToken(User user)
//        {
//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name,user.Username),
//                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
//            };

//            var key = new SymmetricSecurityKey(
//                    Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

//            var tokenDescriptor = new JwtSecurityToken(
//                    issuer: configuration.GetValue<string>("AppSettings:Issuer"),
//                    audience: configuration.GetValue<string>("AppSettings:Audience"),
//                    claims: claims,
//                    expires: DateTime.UtcNow.AddDays(1),
//                    signingCredentials: creds
//                    );
//            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
//        }


//    }
//}

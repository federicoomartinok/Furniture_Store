using API.Mastery.Udemy.Configuration;
using FurnitureStoreModels.Auth;
using FurnitureStoreModels.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Mastery.Udemy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AuthenticationController(UserManager<IdentityUser> userManager, IOptions<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
        {
            //Este modelState verifica que las DataAnotation de DTO se cumplan "[Required]"
            if (!ModelState.IsValid) return BadRequest();

            //Chequear si el email Existe
            var emailExist = await _userManager.FindByEmailAsync(request.Email);
            if (emailExist != null)
            {
                return BadRequest(new AuthResult()
                {
                    Result = true,
                    Errors = new List<string>()
                    {
                        "Email Already Exist"
                    }
                });
            }

            //Crear el usuario si el email no existe
            var user = new IdentityUser()
            {
                Email = request.Email,
                UserName = request.Email
            };

            //Aca lo inserta en la DB
            var isCreated = await _userManager.CreateAsync(user, request.Password);

            if (isCreated.Succeeded)
            {
                var token = GenerateToken(user);
                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = token
                });
            }
            else
            {
                var errors = new List<string>();
                foreach (var error in isCreated.Errors)
                    errors.Add(error.Description);


                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = errors
                });

            }        

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            //1° se chequea que el user exista
            //2° se comparan las password para que funcione 

            if (!ModelState.IsValid) BadRequest();
            //Principal chequear que el usuario exista

            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser == null) 
                return BadRequest(new AuthResult
                {
                    Errors = new List<string>() { "Invalid Paylod"},
                    Result = false
                });


            var checkUser = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!checkUser) BadRequest(new AuthResult
            {
                Errors = new List<string>() { "Invalid Credentials" },
                Result = false
            });

            var token = GenerateToken(existingUser);
            return Ok(new AuthResult
            {
                Token = token,
                Result = true,
            });
        }


        private string GenerateToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
                {
                    new Claim ("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    //Identificador unico de ID para el token
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                })),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }

    }
}

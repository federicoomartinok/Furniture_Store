using API.Mastery.Udemy.Configuration;
using FurnitureStoreData;
using FurnitureStoreModels;
using FurnitureStoreModels.Auth;
using FurnitureStoreModels.Common;
using FurnitureStoreModels.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace API.Mastery.Udemy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailSender _emailSender;
        private readonly APIcontext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;
        
        public AuthenticationController (UserManager<IdentityUser> userManager, 
                                        IOptions<JwtConfig> jwtConfig, 
                                        IEmailSender emailSender,
                                        APIcontext context,
                                        TokenValidationParameters tokenValidationParameters)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
            _emailSender = emailSender;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;
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
                UserName = request.Email,
                EmailConfirmed = false
            };

            //Aca lo inserta en la DB
            var isCreated = await _userManager.CreateAsync(user, request.Password);

            if (isCreated.Succeeded)
            {
                //var token = GenerateToken(user);

                await SendVerificationEmail(user);

                return Ok(new AuthResult()
                {
                    Result = true,
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
                    Errors = new List<string>() { "Invalid Paylod." },
                    Result = false
                });

            //Checkea si esta confirmado el email
            if (!existingUser.EmailConfirmed)
                return BadRequest(new AuthResult
                {
                    Errors = new List<string>() { "Email needs to be confirmed." },
                    Result = false
                });

            //Checkea la credencial del usuario
            var checkUser = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!checkUser) BadRequest(new AuthResult
            {
                Errors = new List<string>() { "Invalid Credentials." },
                Result = false
            });

            //Checkea el token
            var token = GenerateTokenAsync(existingUser);
            return Ok(token);
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult>RefreshToken([FromBody]TokenRequest tokenRequest)
        {
            if(!ModelState.IsValid) 
                return BadRequest(new AuthResult
                {
                    Errors = new List<string>{ "Invalid Parameters" },
                    Result = false
                });

            var results = VerifyAndGenerateTokenAsync(tokenRequest);

            if(results == null)
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid Token" }
                });

            return Ok(results);

        }



        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invaild email confirmation URL" },
                    Result = false
                });


            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with Id '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            var status = result.Succeeded ? "Tanks you for confirming your email"
                                                 : "There has been an error confirming your email";
            return Ok(status);

            
        }


        private async Task<AuthResult> GenerateTokenAsync(IdentityUser user)
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
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                Token = RandomGenerator.GenerateRandomString(23),//Token random generado
                AddDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id,
            };

            await _context.RefreshTokens.AddAsync(refreshToken);//Se agrega a la db
            await _context.SaveChangesAsync();//se guarda con entity

            return new AuthResult
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }

        private async Task SendVerificationEmail (IdentityUser user)
        {
            //Recibe token de verificacion
            var verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //Codifica la verificacion con una url base 64
            verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationCode));

            // Ejemplo de callback URL: https://localhost:8008/api/authentication/verifyemail/userId=exampleuserId&code=examplecode

            var callbackUrl = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
                                                                new { userId = user.Id, code = verificationCode })}";

            var emailBody = $"Please confirm your account by <a href ='{HtmlEncoder.Default.Encode(callbackUrl)}'><strong>clicking here </strong></a>";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your Email", emailBody);
        }

        private async Task<AuthResult> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;
                var tokenBeingVerified = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if(validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if(!result || tokenBeingVerified == null)
                    {
                        throw new Exception("Invalid Token");
                    }
                }

                var utcExpiryDate = long.Parse(tokenBeingVerified.Claims.
                    FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;
                if(expiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                var storedToken = await _context.RefreshTokens.
                    FirstOrDefaultAsync(t => t.Token == tokenRequest.RefreshToken);

                if (storedToken == null)
                    throw new Exception("Invalid Token");

                if (storedToken.IsRevoked || storedToken.IsRevoked)
                    throw new Exception("Invalid Token");

                var jti = tokenBeingVerified.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp).Value;

                if (jti != storedToken.JwtId)
                    throw new Exception("Invalid Token");

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);

                return await GenerateTokenAsync(dbUser);

            }
            catch (Exception e)
            {

                var message = e.Message == "Invalid Token" || e.Message == "Token Expired"
                    ? e.Message
                    : "Internal Server Error";
                return new AuthResult
                {
                    Result = true,
                    Errors = new List<string> { message }
                };
            }
        }

    }
}

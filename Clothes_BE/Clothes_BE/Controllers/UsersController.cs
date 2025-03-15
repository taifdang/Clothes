using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Clothes_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _config;
        public UsersController(DatabaseContext databaseContext,IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _config = configuration;

        }
        [HttpGet("admin")]
        public IActionResult get_all()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return BadRequest(new Response { status = 400, message = "Thất bại" });
            var useClaim = identity.Claims;

            return Ok(new UserDTO
            {
                name = useClaim.FirstOrDefault(a => a.Type == ClaimTypes.Name)?.Value,
                email = useClaim.FirstOrDefault(a => a.Type == ClaimTypes.Email)?.Value,
                avatar = useClaim.FirstOrDefault(a => a.Type == ClaimTypes.Role)?.Value
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> get()
        {
            var users = await _databaseContext.users.ToListAsync();
            if (users == null) return BadRequest(new Response { status = 400, message = "Thất bại" });
            return Ok(new Response { status = 200, message = "Thành công", data = users });
        }        
        [HttpPost("login")]
        public async Task<ActionResult> login([FromForm] LoginDTO DTO)
        {
            try
            {
                var user = await _databaseContext.users
                    .Where(x => x.email == DTO.email && x.password == HashPassword(DTO.password))
                    .FirstOrDefaultAsync();
                //
                if (user is null) return BadRequest(new Response { status = 400, message = "Thất bại" });
                //create token             
                var tokenHandler = new JwtSecurityTokenHandler();
                //
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                //
                var tokenDecriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                            new Claim(ClaimTypes.Name,user.name),
                            new Claim(ClaimTypes.Email,user.email),
                            new Claim(ClaimTypes.Role,user.role)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                };
                //
                var token = tokenHandler.CreateToken(tokenDecriptor);

                return Ok(new Response
                {
                    status = 200,
                    message = "Thành công",
                    data = tokenHandler.WriteToken(token),
                });
            }
            catch
            {
                return BadRequest(new Response
                {
                    status = 400,
                    message = "Thất bại"
                });
            }
        }
        [HttpPost("register")]
        public async Task<ActionResult> register([FromForm] UserDTO DTO)
        {

            var existEmail = await _databaseContext.users.Where(e => e.email == DTO.email).FirstOrDefaultAsync();
            if (existEmail != null) return BadRequest(new Response { status = 400, message = "Email đã tồn tại" });
            //
            try
            {
                _databaseContext.users.Add(new Users
                {
                    name = DTO.name,
                    email = DTO.email,
                    phone = DTO.phone,
                    password = HashPassword(DTO.password),
                    avatar = "",
                    role = "user",
                });
                await _databaseContext.SaveChangesAsync();
            }
            catch
            {
                return BadRequest(new Response { status = 400, message = "Thất bại" });
            }
            return Ok(new Response { status = 200, message = "Thành công" });
        }     
        [NonAction]
        public string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}

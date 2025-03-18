using AutoMapper;
using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Clothes_BE.Controllers
{
    [Route("api/cart-items")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _config;
        public CartItemsController(DatabaseContext context, IConfiguration configuration)
        {
            _databaseContext = context;
            _config = configuration;
        }
        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Carts>>> GetCart()
        {
           
            var cart = await _databaseContext.carts.ToListAsync();          
            return Ok(new Response { status = 200, message = "Thành công ", data = cart });
        }
        [HttpGet("merge-cart")]
        public async Task<ActionResult> MergeCart()
        {
            using var transactions = _databaseContext.Database.BeginTransaction();
            try
            {
                int? user_id = HttpContext.User.Identity.IsAuthenticated ? int.Parse(User.FindFirst(ClaimTypes.Name)?.Value) : null;
                string session_id = HttpContext.Request.Cookies["guest_id"];

                var userCart = await _databaseContext.carts.Include(c => c.cartItems).FirstOrDefaultAsync(x=>x.user_id == user_id);
                var sessionCart = await _databaseContext.carts.Include(c => c.cartItems).FirstOrDefaultAsync(x => x.session_id == session_id);

                if(sessionCart != null)
                {
                    if(userCart == null)
                    {
                        sessionCart.user_id = user_id;
                    }
                    else
                    {
                        foreach (var item in sessionCart.cartItems)
                        {
                            var cart_item = userCart.cartItems
                                .FirstOrDefault(c => c.product_variant_id == item.product_variant_id);

                            if (cart_item != null) cart_item.quantity += item.quantity;
                            item.cart_id = userCart.id;
                        }
                        _databaseContext.carts.Remove(sessionCart);
                    }
                    await _databaseContext.SaveChangesAsync();
                }
                transactions.Commit();
                return Ok("Thành công");
            }
            catch
            {
                transactions.Rollback();
                return Ok("Thất bại");
            }
           
        }
        //??
        [HttpPost("add-to-cart")]
        public async Task<ActionResult> AddItemToCart([FromForm]AddToCartDTO DTO) 
        {
            using var transactions = _databaseContext.Database.BeginTransaction();
            try
            {
                var data = HttpContext.Items["auth"];
                int? user_id = HttpContext.User.Identity.IsAuthenticated ? int.Parse(User.FindFirst(ClaimTypes.Name)?.Value) : null;
                string check_session = HttpContext.Request.Cookies["guest_id"];
                var session_id = "";
                //var check_session = HttpContext.Request.Cookies.TryGetValue("guest_id", out string session_id) ? session_id : null;
                if (check_session is null)
                {
                    string sessionId = Guid.NewGuid().ToString();
                    session_id = sessionId;
                       HttpContext.Response.Cookies.Append("guest_id", sessionId, new CookieOptions { 
                       HttpOnly = false,
                       Secure = true,
                       IsEssential = true,
                       Expires = DateTime.UtcNow.AddDays(30)
                    });
                }
                //var cart = await _databaseContext.carts.FirstOrDefaultAsync(c => c.user_id == user_id || (c.session_id == session_id&& c.user_id == null));
                var cart = await _databaseContext.carts.Where(c => c.session_id == session_id).FirstOrDefaultAsync();
                //create cart
                if (cart is null)
                {
                    _databaseContext.carts.Add(cart = new Carts{session_id = session_id});
                    await _databaseContext.SaveChangesAsync();
                }

                //else if (cart.user_id == null && user_id != null)
                //{
                //    cart.user_id = user_id;
                //    //session_id = "";
                //    _databaseContext.SaveChanges();
                //}
                var cart_item = await _databaseContext.cart_items
                       .Where(a => a.cart_id == cart.id && a.product_variant_id == DTO.product_variant_id)
                       .FirstOrDefaultAsync();

                 if (cart_item != null) cart_item.quantity += DTO.quantity;
                 else _databaseContext.cart_items.Add(cart_item = new CartItems { cart_id = cart.id,product_variant_id = DTO.product_variant_id,quantity = DTO.quantity});
                 //save
                 await _databaseContext.SaveChangesAsync();


                transactions.Commit();
                
                return Ok(new Response { status = 200, message = "Thành công",data=new { user_id,session_id, cart_item,data } });
                
            }
            catch
            {
                transactions.Rollback();
                return BadRequest(new Response { status = 400, message = "Thất bại"});
            }
  
        }
        [HttpPut("update-cart")]
        public async Task<ActionResult> UpdateCartitem([FromForm]CartDTO DTO,int id)
        {
            try
            {
                int? user_id = HttpContext.User.Identity.IsAuthenticated ? int.Parse(User.FindFirst(ClaimTypes.Name)?.Value) : null;
                string session_id = HttpContext.Request.Cookies["guest_id"];
                //get cart current
                var cart = await _databaseContext.carts.FirstOrDefaultAsync(c => c.user_id == user_id || (c.session_id == session_id && c.user_id == null));

                var cart_item = await _databaseContext.cart_items
                    .Where(x => x.id == DTO.id)
                    .FirstOrDefaultAsync();
                //change
                cart_item.product_variant_id = DTO.product_variant_id;
                cart_item.quantity = DTO.quantity;
                await _databaseContext.SaveChangesAsync();
                return Ok(new Response { status = 200, message = "Thành công" });
            }
            catch
            {
                return BadRequest(new Response { status = 400, message = "Thất bại" });
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveCartItem(int id)
        {
            try
            {
                var cart_item = await _databaseContext.cart_items
                .Where(x => x.id == id)
                .FirstOrDefaultAsync();
                //remove
                if(cart_item == null) return BadRequest();
                _databaseContext.cart_items.Remove(cart_item);

                await _databaseContext.SaveChangesAsync();

                 return Ok(new Response { status = 200, message = "Thành công" });
            }
            catch
            {
                return BadRequest(new Response { status = 400, message = "Thất bại" });
            }
            
            
           
        }
    }
}

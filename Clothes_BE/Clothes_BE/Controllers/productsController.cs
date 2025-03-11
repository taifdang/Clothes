
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Clothes_BE.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class productsController : ControllerBase
    {
        // GET: api/<productsController>
        public readonly DatabaseContext _databaseContext;
        public productsController(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Products>>> Get()
        {
            //var data = _databaseContext.products.Include(x=>x.product_attributes).Include(x => x.product_variants).ToList();

            //var product = data.Select(x => new
            //{
            //   x.id,
            //   x.title,
            //   x.price,
            //   x.old_price,
            //   attributes = x.product_attributes.ToList(),
            //   variants = x.product_variants.Select(variant =>
            //   {
            //       int lastSpaceIndex = variant.title.LastIndexOf(' ');
            //       return new ProductVariantDTO
            //       {
            //           id = variant.id,
            //           title = variant.title,
            //           price = variant.price,
            //           old_price = variant.old_price,
            //           quantity = variant.quantity,
            //           option1 = variant.title.Substring(0,lastSpaceIndex).Trim(),
            //           option2 = variant.title.Substring(lastSpaceIndex+1).Trim(),
            //           product_id = variant.product_id,
            //           product_title = variant.products.title,
            //           percent = variant.percent,

            //       };
            //   }).ToList(),
            //   options = x.product_variants
            //        .Select(p =>
            //        {
            //            int lastSpaceIndex = p.title.LastIndexOf(' ');
            //            return new { 
            //                title = "color",
            //                value = p.title.Substring(0, lastSpaceIndex).Trim(),
            //            };

            //        }) // Lấy phần màu sắc từ title
            //        .Distinct()
            //        .GroupBy(x=>x.title == "color")
            //        .Select(g => new
            //         {                       
            //             values = g.Select(x => x.value).Distinct().ToList()
            //        })
            //        .ToList()
            //}).ToList();
            Response response = new Response();
            //var data = await _databaseContext.products.Include(p=>p.product_option_images).Include(v=>v.product_variants).ThenInclude(p=>p.variants).ToListAsync();
            var data = await _databaseContext.products.Include(p => p.product_option_images).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
           

            return Ok(data);
        }
        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<Products>>> get_all_pagination()
        {
            var data = await _databaseContext.products.Include(p => p.product_option_images).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
            var list_data = Pagination(data, 1, 5);
            return Ok(list_data);
        }
        [HttpGet("filter")]
        public async Task<ActionResult> filter(int? productType,int? categoryId,SortType sortType,int currentPage, int limit)
        {
              
            try
            {
                //get prroducts
                var products = await _databaseContext.products.Include(c=>c.categories).Include(v=>v.product_variants).Include(i=>i.product_option_images).ToListAsync();
                if (products == null) return BadRequest(new Response { status = 404, message = "Thất bại" });
                if (categoryId != null) products = products.Where(c => c.category_id == categoryId).ToList();
                switch (sortType)
                {
                    case SortType.Default:                    
                        break;
                    case SortType.Ascending:
                        products = products.OrderBy(p => p.price).ToList();
                        break;
                    case SortType.Descending:
                        products = products.OrderByDescending(p => p.price).ToList();
                        break;
                    case SortType.Percent:
                        products = products.Where(p => p.old_price % p.price> 0).ToList();
                        break;
                    default:
                        break;
                }
                var list_products = Pagination(products, currentPage, limit);
                return Ok(new Response{ status = 200, message = "Thành công", data = list_products } );
            }
            catch  { return BadRequest(new Response { status = 404, message = "Thất bại"}); }
          

        }

        // GET api/<productsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<productsController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<productsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<productsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        [NonAction]
        public object Pagination(ICollection<Products> product, int currentPage, int limit)
        {

            int total_item = product.Count();
            int skip = (currentPage - 1) * limit;
            //làm tròn sau dấu . =>Math.ceiling / phần nguyên + phần dư !=0 => +1
            int total_page = (total_item / limit) + (total_item % limit == 0 ? 0 : 1);

            var products = product.Skip(skip).Take(limit);
            var count_data = products.Count();

            return new { products, pagination = new { totalItem = count_data, currentPage, totalPage = total_page } };
        }
    }
}

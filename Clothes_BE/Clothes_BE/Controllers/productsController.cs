
using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

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
         
            //var data = await _databaseContext.products.Include(p=>p.product_option_images).Include(v=>v.product_variants).ThenInclude(p=>p.variants).ToListAsync();
            var products = await _databaseContext.products.Include(p => p.product_option_images).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
            if (products == null) return BadRequest(new Response { status = 400, message ="Thất bại"});
            return Ok(new Response { status = 200, message="Thành công",data = products});
        }
        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<Products>>> get_all_pagination()
        {
            var data = await _databaseContext.products.Include(p => p.product_option_images).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
            var list_data = Pagination(data, 1, 5);
            return Ok(list_data);
        }
        [HttpGet("filter")]
        public async Task<ActionResult> filter(int? productType,int? categoryId, SortType sortType,int currentPage, int limit)
        {
            //HttpContext.Response.StatusCode
            try
            {              
                var products = await _databaseContext.products.Include(c => c.categories).Include(v => v.product_variants).ThenInclude(vr=>vr.variants).Include(i => i.product_option_images).ToListAsync();
                if (products == null) return BadRequest(new Response { status = 400, message = "Thất bại" });
                if (productType != null)
                {
                    var isProductType = products.FirstOrDefault(pt => pt.categories.product_types_id == productType);
                    if(isProductType == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                    products = products.Where(pt => pt.categories.product_types_id == productType).ToList();
                }

                if (categoryId != null)
                {
                    var isCategoryId = products.FirstOrDefault(c => c.category_id == categoryId);
                    if(isCategoryId == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                    products = products.Where(c => c.category_id == categoryId).ToList();
                }
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
            catch  { return BadRequest(new Response { status = 400, message = "Thất bại"}); }
          

        }

        // GET api/<productsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> get_id(int id)
        {
            var data = await _databaseContext.products.Where(p=>p.id == id).Include(p => p.product_option_images).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
            return Ok(data);
        }

        // POST api/<productsController>
        [HttpPost]
        public async Task<ActionResult> Post([FromForm]ProductDTO productDTO)
        {
            using (var transactions = _databaseContext.Database.BeginTransaction())
            {
                try
                {
                    //check category
                    var isCategory = await _databaseContext.categories.FindAsync(productDTO.category_id);
                    if(isCategory == null) return BadRequest(new Response { status = 404, message = "Không tìm thấy category" });
                    //product
                    Products products = new Products()
                    {
                        id = productDTO.id,
                        category_id = productDTO.category_id,
                        title = productDTO.title,
                        price = productDTO.price,
                        old_price = productDTO.old_price,
                        description = productDTO.description,
                    };
                    _databaseContext.products.Add(products);
                    await _databaseContext.SaveChangesAsync();
                    //check option nếu option(string) không khớp option_id(string) sẽ không add được.                
                    //Sau khi savechange() product_id bị EF xóa
                    _databaseContext.product_options.AddRange(
                        new ProductOptions { product_id = products.id, option_id = productDTO.option1},
                        new ProductOptions { product_id = products.id ,option_id = productDTO.option2}
                    );                  
                    await _databaseContext.SaveChangesAsync();
                    //lưu ảnh

                    //save db
                    transactions.Commit();
                }
                catch(Exception ex)
                {
                    transactions.Rollback();
                    return BadRequest(new Response { status = 404, message = "Thất bại" });
                }
            }
            return Ok(new Response { status = 200, message = "Thành công" });

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
            //var count_data = products.Count();

            return new { products, pagination = new { totalItem = total_item, currentPage, totalPage = total_page } };
        }
        [NonAction]
        public async Task<ActionResult> SaveImage(IFormFileCollection files,Products product)
        {
            foreach (var file in files)
            {
                var file_name = file.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", file_name);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {                
                   await file.CopyToAsync(stream);                   
                }
            }          
            return Ok();


        }
    }
}


using AutoMapper;
using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Clothes_BE.Controllers
{
    [Route("api/products")]
    [ApiController]
    //[ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, NoStore = false)]
    public class productsController : ControllerBase
    {
        // GET: api/<productsController>
        public readonly DatabaseContext _databaseContext;
        public readonly IMemoryCache _cache;
        public readonly ILogger<productsController> _logger;
        private readonly IMapper _mapper;
        public readonly string key_cache = "productkey@112";
        public productsController(DatabaseContext databaseContext, IMemoryCache memoryCache, ILogger<productsController> logger,IMapper mapper)
        {
            this._databaseContext = databaseContext;
            this._cache = memoryCache;
            _logger = logger;
            _mapper = mapper;
        }
       
        //[ResponseCache(CacheProfileName = "API_CACHING")]
        [HttpGet]             
        public async Task<ActionResult<IEnumerable<Products>>> Get()
        {          
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            if (_cache.TryGetValue(key_cache, out IEnumerable<Products> product))
            {
                //Có cache
                _logger.Log(LogLevel.Information, "Product is have cache");
            }
            else
            {
                //Chưa có cache
                _logger.Log(LogLevel.Warning, "Product not found cache");
                product = await _databaseContext.products                    
                    .Include(p => p.product_option_images)
                    .Include(c => c.product_options)
                    .Include(v => v.product_variants)
                        .ThenInclude(p => p.variants)                                     
                    .ToListAsync();
                //var product_list = _mapper.Map<ProductDTO>(product);
                    
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30)) // thời gian hết hạn từ request cuối
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // thời gian hết hạn từ khi tạo
                    .SetPriority(CacheItemPriority.Normal) // độ ưu tiên
                    .SetSize(10) // giới hạn cache
                    .RegisterPostEvictionCallback((key, value, reason, state) => //callback khi xóa cache
                    {
                        Console.WriteLine($"Cache deleted: {key}, Reason: {reason} - {state}");
                    }); ;
                _cache.Set(key_cache, product, cacheEntryOptions); // thêm vào cache
            }
            //var products = await _databaseContext.products.Include(p => p.product_option_images).Include(c => c.product_options).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
            //if (products == null) return BadRequest(new Response { status = 400, message = "Thất bại" });
            stopWatch.Stop();
            _logger.Log(LogLevel.Information, "Pass Time " + stopWatch.ElapsedMilliseconds);
            //mapper
            //_mapper.Map<List<ProductResultDTO>>(product)
            return Ok(new Response { status = 200, message="Thành công",data = product });
        }
        [HttpGet("remove-cache")]
        public IActionResult clearCache()
        {
            _cache.Remove(key_cache);
            _logger.Log(LogLevel.Information, "Remove cache successfull " + DateTime.UtcNow);
            return Ok("remove cache");
        }
        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<Products>>> get_all_pagination()
        {
            var data = await _databaseContext.products.Include(p => p.product_option_images).Include(c => c.product_options).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
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
            var data = await _databaseContext.products.Where(p=>p.id == id).Include(p => p.product_option_images).Include(c => c.product_options).Include(v => v.product_variants).ThenInclude(p => p.variants).ToListAsync();
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
                    //Sau khi savechange() product_id bị EF xóa???
                    //Điều kiện ?
                    foreach(var option in productDTO.options)
                    {
                        _databaseContext.product_options.AddRange(new ProductOptions { product_id = products.id, option_id = option });
                    }  
                    await _databaseContext.SaveChangesAsync();
                    //save db
                    transactions.Commit();
                }
                catch
                {
                    transactions.Rollback();
                    return BadRequest(new Response { status = 404, message = "Thất bại" });
                }
            }
            return Ok(new Response { status = 200, message = "Thành công" });

        }

        // PUT api/<productsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm]ProductDTO productDTO)
        {
            using (var transactions = _databaseContext.Database.BeginTransaction())
            {
                try
                {    
                    if(id != productDTO.id) return BadRequest(new Response { status = 400, message=" Không khớp id cần thay đổi"});
                    var isProduct = await _databaseContext.products.FindAsync(id);
                    if (isProduct == null) return BadRequest(new Response { status = 400, message = " Không tìm thấy id" });
                    //
                  
                    //mapper
                    isProduct.title = productDTO.title;
                    isProduct.category_id = productDTO.category_id;
                    isProduct.price = productDTO.price;
                    isProduct.old_price = productDTO.old_price;
                    isProduct.description = productDTO.description;
                    var isOption = await _databaseContext.product_options.Where(x => x.product_id == isProduct.id).ToListAsync();
                    foreach (var option in isOption)
                    {
                        _databaseContext.product_options.RemoveRange(option);
                    }
                    _databaseContext.SaveChanges();
                    //?
                    foreach (var option in productDTO.options)
                    {
                        _databaseContext.product_options.AddRange(new ProductOptions { product_id = productDTO.id, option_id = option });
                    }
                    await _databaseContext.SaveChangesAsync();

                    _databaseContext.SaveChanges();
                    transactions.Commit();
                }
                catch
                {
                    transactions.Rollback();
                    return BadRequest();
                }
            }
            return Ok();
        }

        // DELETE api/<productsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            // xóa product => xóa ảnh, product_option, product_variant, variant_options, file images
            using(var transactions = _databaseContext.Database.BeginTransaction())
            {
                try
                {
                    var isProduct = await _databaseContext.products.Include(c=>c.categories).FirstOrDefaultAsync(x=>x.id == id);
                    if (isProduct == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                    //delete image                 
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "images", isProduct.categories.label+"-"+isProduct.id);
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path,true);
                    }
                    //

                    _databaseContext.Remove(isProduct);
                    await _databaseContext.SaveChangesAsync();
                    transactions.Commit();
                    return Ok(new Response { status = 200, message = "Thành công"  });
                }
                catch
                {
                    transactions.Rollback();
                    return BadRequest(new Response { status = 400, message = "Thất bại" });
                }
            }          
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
        public async Task<ActionResult> SaveImage(IFormFile[] files,OptionValues option_value,Products product)
        {
            foreach (var file in files)
            {
                var file_name = product.categories.label + product.id + option_value.label;
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

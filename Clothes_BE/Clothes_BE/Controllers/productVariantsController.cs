using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Clothes_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class productVariantsController : ControllerBase
    {
        public readonly DatabaseContext _databaseContext;
        public productVariantsController(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }
        // GET: api/<productVariantsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVariants>>> Get()
        {
            var data = await _databaseContext.product_variants.Include(o => o.variants).ToListAsync();
            return Ok(data);
        }

        // GET api/<productVariantsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var ishas = await _databaseContext.product_variants.FindAsync(id);
            if (ishas == null) return BadRequest(new Response { status = 400, message = "Thất bại" });

            return Ok(new Response { status = 200, message = "Thành công", data = ishas });
        }
        [HttpGet("filter")]
        public async Task<ActionResult> Get(int ?id,int? product_id)
        {
            try
            {
                var data = await _databaseContext.product_variants.ToListAsync();
                if (product_id != null)
                {
                    var ishas = data.FirstOrDefault(x => x.product_id == product_id);
                    if(ishas == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                    data = data.Where(x => x.product_id == product_id).ToList();
                }

                if (id != null)
                {
                    var ishas = data.FirstOrDefault(x => x.id == id);
                    if (ishas == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                    data = data.Where(x => x.id == id).ToList();
                }
                

                return Ok(new Response { status = 200, message = "Thành công", data = data });
            }
            catch
            {
                return BadRequest(new Response { status = 400, message = "Thất bại" });
            }
           
           
        }

        //??? CONSTRAINT OPTION OF PRODUCT_OPTION_IMAGES => PRODUCT_VARIANT =>CHOOSE OPTION
        //(EXAMPLE:PRODUCT_OPTION_IMAGES:{"product_id":1,"option_value_id:"10"}) => PRODUCT_VARIANT => option include option_value_id =10
        [HttpPost]
        public async Task<ActionResult> Post([FromForm]ProductVariantDTO DTO)
        {         
            var check_data = _databaseContext.product_options               
                .Where(x => x.product_id == DTO.product_id)              
                .Select(g => g.option_id).ToList();
            var option_value = _databaseContext.option_values.ToList();
            var isProduct = _databaseContext.products.Include(c => c.categories).FirstOrDefault(p => p.id == DTO.product_id);
            //
            using (var transactions = _databaseContext.Database.BeginTransaction())
            {
                //constraint: product_variants  <= variants <= products_option          
                try
                {                   
                    var step1 = new ProductVariants
                    {
                        product_id = DTO.product_id,
                        title = "",
                        price = DTO.price,
                        old_price = DTO.old_price,
                        percent = Math.Ceiling(((DTO.old_price - DTO.price) / DTO.old_price) * 100),
                        quantity = DTO.quantity,
                        sku = $"{isProduct.categories.label}.{isProduct.id}",
                    };
                    _databaseContext.product_variants.Add(step1);
                    await _databaseContext.SaveChangesAsync();
                    //
                    foreach (var option in DTO.options)
                    {
                        var option_item = option_value.FirstOrDefault(p => p.id == option);
                        if (!check_data.Contains(option_item.option_id)) return BadRequest(new Response { status = 400, message = "Option không có trong ràng buộc của sản phẩm" });
                        //
                        var step2 = new Variants
                        {
                            product_variant_id = step1.id,
                            option_value_id = option
                        };
                        //
                        step1.title = string.IsNullOrWhiteSpace(step1.title) 
                                       ? option_item.value
                                       : step1.title+ " / " + option_item.value;
                        step1.sku = step1.sku + "." +option_item.label;
                        //
                        _databaseContext.variants.Add(step2);                        
                        await _databaseContext.SaveChangesAsync();
                    }
                   
                    transactions.Commit();
                }
                catch
                {
                    transactions.Rollback();
                    return BadRequest(new Response { status = 400, message = "Thất bại" });
                    
                }
            }
            return Ok(new Response { status = 200, message = "Thành công" ,data = check_data });
        }

        // PUT api/<productVariantsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<productVariantsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            //constraint => delete id => delete all table constranit
            try
            {
                var ishas =  await _databaseContext.product_variants.FindAsync(id);
                if (ishas == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                _databaseContext.product_variants.Remove(ishas);
                await _databaseContext.SaveChangesAsync();
            }
            catch
            {
                return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
            }           
            return Ok(new Response { status = 200, message = "Thành công" });
        }
    }
}
